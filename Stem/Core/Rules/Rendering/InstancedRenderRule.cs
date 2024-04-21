using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Stem.Experimental;
using System.Linq;
using Utilities.Extensions;

namespace Stem.Rules.Rendering;

public abstract class InstancedRenderRule<TVertexData, TInstanceData> : RenderRule 
    where TVertexData : struct
    where TInstanceData : struct
{
    protected record struct ShaderLayoutInfo(int Location, Type Type, int Count);

    protected record struct TextureInfo(string SamplerName, int TextureUnit, string? FilePath, int? Handle = null, bool LinearFiltering = false);

    private int __vertexArray;

    private int __vertexBuffer;

    private int __elementBuffer;

    private int __instanceBuffer;

    protected Shader shaderProgram;

    protected ImageTextureRegistry ImageTextureRegistry { get; private set; }

    protected abstract TVertexData[] VertexData { get; }

    protected abstract uint[] Indices { get; }

    protected abstract ShaderLayoutInfo[] VertexDataLayout { get; }

    protected abstract ShaderLayoutInfo[] InstanceDataLayout { get; }

    protected abstract string VertexShaderCode { get; }

    protected abstract string FragmentShaderCode { get; }

    protected abstract void SetUniforms(StemWindow window);

    protected abstract List<(string SamplerName, string FilePath)> SamplerSources { get; }

    protected List<int> TextureHandles { get; private init; } = new();

    protected void SetScreenSpaceTransform2D(StemWindow window, string uniformName)
    {
        var size = window.ClientSize;
        shaderProgram.SetMatrix2(uniformName, new(2 * window.magnification / size.X, 0, 0, 2 * window.magnification / size.Y));
    }
    protected void SetScreenSpaceTransformUI(StemWindow window, string uniformName)
    {
        var size = window.ClientSize;
        shaderProgram.SetMatrix2(uniformName, new(2f / size.X, 0, 0, 2f / size.Y));
    }

    protected void SetScreenSpaceTransform3D(StemWindow window, string uniformName, float pixelsPerUnit)
    {
        var size = window.ClientSize;
        shaderProgram.SetMatrix3(uniformName, new(2 * window.magnification * pixelsPerUnit / size.X, 0, 0, 0, 2 * window.magnification * pixelsPerUnit / size.Y, 0, 0, 0, window.magnification * pixelsPerUnit / size.X));
    }

    protected void SetScreenSpaceTransform4D(StemWindow window, string uniformName, float pixelsPerUnit)
    {
        var windowPixels = window.ClientSize;
        var screensPerUnitX = 2 * pixelsPerUnit / windowPixels.X; // (pix / unit) / (pix / screenWidth) = (screenWidths / unit)
        var screensPerUnitY = 2 * pixelsPerUnit / windowPixels.Y; // (pix / unit) / (pix / screenHeight) = (screenHeights / unit)
        var screensPerUnitZ = (screensPerUnitX + screensPerUnitY) / 2f; // depth scale == avg of length and width scale
        shaderProgram.SetMatrix4(uniformName,
            new( screensPerUnitX,               0,               0,     0,
                               0, screensPerUnitY,               0,     0,
                               0,               0, screensPerUnitZ,     0,
                               0,               0,               0,     1));
    }

    protected void SetPerspectiveMatrix4D(string uniformName, float nearPlaneDistance, float farPlaneDistance)
    {
        var frustumDepth = farPlaneDistance - nearPlaneDistance;
        var alpha = farPlaneDistance / frustumDepth;
        var beta = nearPlaneDistance * alpha;
        shaderProgram.SetMatrix4(uniformName,
            new( 1,    0,      0,    0,
                 0,    1,      0,    0,
                 0,    0, -alpha,   -1,
                 0,    0,  -beta,    0  ));
    }

    protected void SetCameraToPerspectiveMatrix4D(StemWindow window, string uniformName, float pixelsPerUnit, float nearPlaneDistance, float farPlaneDistance)
    {
        var windowPixels = window.ClientSize;
        var screensPerUnitX = pixelsPerUnit / windowPixels.X; // (pix / unit) / (pix / screenWidth) = (screenWidths / unit)
        var screensPerUnitY = pixelsPerUnit / windowPixels.Y; // (pix / unit) / (pix / screenHeight) = (screenHeights / unit)
        var screensPerUnitZ = (screensPerUnitX + screensPerUnitY) / 2f; // depth scale == avg of length and width scale
        var frustumDepth = farPlaneDistance - nearPlaneDistance;
        var alpha = farPlaneDistance / frustumDepth;
        var beta = nearPlaneDistance * alpha;
        shaderProgram.SetMatrix4(uniformName,
            new( screensPerUnitX,               0,                         0,     0,
                               0, screensPerUnitY,                         0,     0,
                               0,               0, -alpha * screensPerUnitZ + 0.5f,    -beta * screensPerUnitZ, 
                               0,               0,  - 1f,     0));
    }

    protected void SetMagnification(StemWindow window, string uniformName)
    {
        shaderProgram.SetFloat(uniformName, window.magnification);
    }

    protected void SetCameraPosition(StemWindow window, string uniformName)
    {
        shaderProgram.SetFloat2(uniformName, window.center);
    }

    private readonly Dictionary<Type, VertexAttribPointerType> __getVertexAttribPointerType = new()
    {
        { typeof(Half), VertexAttribPointerType.HalfFloat },
        { typeof(float), VertexAttribPointerType.Float },
        { typeof(double), VertexAttribPointerType.Double }
    };

    private readonly Dictionary<Type, VertexAttribIntegerType> __getVertexAttribIntegerType = new()
    {
        { typeof(uint), VertexAttribIntegerType.UnsignedInt },
        { typeof(int), VertexAttribIntegerType.Int },
    };

    protected internal override void Setup(IEntityStore store)
    {
        if (!Global.IsSet<ImageTextureRegistry>()) {
            Global.Set(new ImageTextureRegistry());
        }
        ImageTextureRegistry = Global.Get<ImageTextureRegistry>();

        __vertexBuffer = GL.GenBuffer();

        __vertexArray = GL.GenVertexArray();

        __elementBuffer = GL.GenBuffer();

        __instanceBuffer = GL.GenBuffer();

        GL.BindVertexArray(__vertexArray);

        GL.BindBuffer(BufferTarget.ArrayBuffer, __vertexBuffer);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, __elementBuffer);

        GL.BufferData(BufferTarget.ArrayBuffer, VertexData.Length * Marshal.SizeOf<TVertexData>(), VertexData, BufferUsageHint.DynamicDraw);
        GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices, BufferUsageHint.StaticDraw);

        int offset = 0;
        foreach (var input in VertexDataLayout)
        {
            if (input.Type == typeof(int) || input.Type == typeof(uint))
            {
                GL.VertexAttribIPointer(input.Location, input.Count, __getVertexAttribIntegerType[input.Type], Marshal.SizeOf<TInstanceData>(), (IntPtr)offset);
            } else {
                GL.VertexAttribPointer(input.Location, input.Count, __getVertexAttribPointerType[input.Type], false,
                    Marshal.SizeOf<TVertexData>(), offset);
            }
            GL.EnableVertexAttribArray(input.Location);
            offset += input.Count * Marshal.SizeOf(input.Type);
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, __instanceBuffer);

        offset = 0;
        foreach (var input in InstanceDataLayout)
        {
            if (input.Type == typeof(int) || input.Type == typeof(uint))
            {
                GL.VertexAttribIPointer(input.Location, input.Count, __getVertexAttribIntegerType[input.Type], Marshal.SizeOf<TInstanceData>(), (IntPtr)offset);
            }
            else
            {
                GL.VertexAttribPointer(input.Location, input.Count, __getVertexAttribPointerType[input.Type], false,
                    Marshal.SizeOf<TInstanceData>(), offset);
            }
            GL.EnableVertexAttribArray(input.Location);
            GL.VertexAttribDivisor(input.Location, 1);
            offset += input.Count * Marshal.SizeOf(input.Type);
        }

        shaderProgram = new Shader();
        shaderProgram.Compile(VertexShaderCode, FragmentShaderCode);

        foreach (var ((samplerName, filePath), index) in SamplerSources.Indexed())
        {
            TextureHandles.Add(ImageTextureRegistry.GetHandle(filePath));
            shaderProgram.SetInt(samplerName, index, true);
        }        
    }

    protected void SetupTexture(ref TextureInfo textureInfo) {
        if (textureInfo.Handle is null) textureInfo.Handle = ImageTextureRegistry.GetHandle(textureInfo.FilePath);

        var handle = textureInfo.Handle.Value;

        if (textureInfo.LinearFiltering) {
            ImageTextureRegistry.SetLinearFilter(handle);
        } else {
            ImageTextureRegistry.SetNearestFilter(handle);
        }

        ImageTextureRegistry.GenerateMipmap(handle);
        
        shaderProgram.SetInt(textureInfo.SamplerName, textureInfo.TextureUnit, true);
    }

    protected abstract IEnumerable<TInstanceData> GetInstanceData(int entity, IEntityStore store, TickTime time);

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        shaderProgram.Use();

        var window = Global.Get<StemWindow>();

        ImageTextureRegistry.Prepare(TextureHandles);

        SetUniforms(window); // TODO: pass all the OnTick parameters into setuniforms

        var instances = new List<TInstanceData>();

        if (Archetype == Archetype.NoRead)
        {
            instances.AddRange(GetInstanceData(-1, store, time));
        }
        else
        {
            for (int index = 0; index < entities.Length; ++index)
            {
                instances.AddRange(GetInstanceData(entities[index], store, time));
            }
        }

        TInstanceData[] instanceArray = instances.ToArray();

        GL.BindVertexArray(__vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, __instanceBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, instanceArray.Length * Marshal.SizeOf<TInstanceData>(), instanceArray, BufferUsageHint.DynamicDraw);

        GL.BindBuffer(BufferTarget.ArrayBuffer, __vertexBuffer);

        GL.DrawElementsInstanced(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, (IntPtr)0, instanceArray.Length);
    }

    protected internal override void Cleanup()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GL.BindVertexArray(0);
        GL.DeleteBuffer(__vertexBuffer);
        GL.DeleteBuffer(__elementBuffer);
        GL.DeleteBuffer(__instanceBuffer);
        GL.DeleteVertexArray(__vertexArray);

        shaderProgram.Dispose();
    }
}
