using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Extensions;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Stem.Experimental;

// TODO: add an interface and make this class internal

public class ImageTextureRegistry {
    private Dictionary<string, int> __loadedHandles = new();

    private HashSet<int> __allHandles = new();

    private List<int> __boundHandles = (-1).Repeated(32).ToList();
    
    private void Bind(int unit, int handle) {
        if (__boundHandles[unit] == handle) return;

        GL.ActiveTexture(TextureUnit.Texture0 + unit);
        GL.BindTexture(TextureTarget.Texture2D, handle);
        __boundHandles[unit] = handle;
    }

    public void Prepare(IEnumerable<int> handles) {
        handles.Take(32).Indexed().ForEach(pair => Bind(pair.Index, pair.Item));
    }

    public int GetHandle(Span<byte> imageBytes, int width, int height) {
        GL.ActiveTexture(TextureUnit.Texture0);
        var handle = GL.GenTexture();
        __allHandles.Add(handle);

        Bind(0, handle);
        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, imageBytes.ToArray());

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        
        SetLinearFilter(handle);

        GenerateMipmap(handle);

        return handle;
    }

    public void Update(int handle, Span<byte> imageBytes, int width, int height) {
        GL.ActiveTexture(TextureUnit.Texture0);
        
        Bind(0, handle);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, imageBytes.ToArray());
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        
        SetLinearFilter(handle);

        GenerateMipmap(handle);
    }

    public int GetHandle(string imagePath) {
        if (__loadedHandles.ContainsKey(imagePath)) return __loadedHandles[imagePath];
        
        GL.ActiveTexture(TextureUnit.Texture0);
        var handle = __loadedHandles[imagePath] = GL.GenTexture();
        __allHandles.Add(handle);
        
        Bind(0, handle);

        var image = SixLabors.ImageSharp.Image.Load<Rgba32>(imagePath);
        image.Mutate(context => context.Flip(FlipMode.Vertical));

        var pixels = new List<byte>(4 * image.Width * image.Height);

        for (int y = 0; y < image.Height; ++y)
        {
            var row = image.GetPixelRowSpan(y);

            for (int x = 0; x < image.Width; ++x)
            {
                pixels.Add(row[x].R);
                pixels.Add(row[x].G);
                pixels.Add(row[x].B);
                pixels.Add(row[x].A);
            }
        }

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        SetLinearFilter(handle);

        GenerateMipmap(handle);
        
        return handle;
    }

    public void SetLinearFilter(int handle) {
        TextureParameter(handle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        TextureParameter(handle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    }

    public void SetNearestFilter(int handle) {
        TextureParameter(handle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
        TextureParameter(handle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    }

    public void TextureParameter(int handle, TextureParameterName name, int value) {
        Bind(0, handle);
        GL.TexParameter(TextureTarget.Texture2D, name, value);
    }

    public void GenerateMipmap(int handle) {
        Bind(0, handle);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    public void Cleanup() {
        __allHandles.ForEach(handle => GL.DeleteTexture(handle));
    }
}
