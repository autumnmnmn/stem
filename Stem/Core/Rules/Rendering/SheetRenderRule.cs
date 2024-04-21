using Stem.Aspects;
using System;
using System.Collections.Generic;

namespace Stem.Rules.Rendering;

public struct SheetVertexData
{
    public float x, y, z, u, v;
}

public struct SheetInstanceData
{
    public float xOffset, yOffset, scale; 
    public int index;
}

public struct SheetAppearance
{
    public int index;
    public float scale;
}

public class SheetRenderRule<TSheetAppearance> : InstancedRenderRule<SheetVertexData, SheetInstanceData>
{
    private Func<TSheetAppearance, (int index, float scale)> AppearanceDecipherer { get; }

    public SheetRenderRule(string assetPath, int rowSize, int columnSize, Func<TSheetAppearance, (int index, float scale)> appearanceDecipherer)
    {
        SamplerSources = new() 
        {
            ("image", @$"Assets/{assetPath}"),
        };

        FragmentShaderCode =
        @$"
            #version 330 core

            in vec2 UV;
            flat in int passedIndex;

            uniform sampler2D image;

            out vec4 FragColor;

            void main()
            {{
                int x = int(mod(passedIndex, {rowSize}));
                int y = {columnSize - 1} - int((passedIndex - x) / {rowSize});
                FragColor = texture(image, vec2((UV.x + x) / {rowSize}, (UV.y + y) / {columnSize}));
            }}
        ";

        AppearanceDecipherer = appearanceDecipherer;
    }

    protected override SheetVertexData[] VertexData { get; } =
    {
        new() { x =  1f, y =  1f, z = 0.0f, u = 1.0f, v = 1.0f },  // top right
        new() { x =  1f, y = -1f, z = 0.0f, u = 1.0f, v = 0.0f },  // bottom right
        new() { x = -1f, y = -1f, z = 0.0f, u = 0.0f, v = 0.0f },  // bottom left
        new() { x = -1f, y =  1f, z = 0.0f, u = 0.0f, v = 1.0f },  // top left
    };

    protected override uint[] Indices { get; } =
    {
        0, 1, 3,   // first triangle
        1, 2, 3    // second triangle
    };

    protected override ShaderLayoutInfo[] VertexDataLayout { get; } =
    {
        new (0, typeof(float), 3),
        new (1, typeof(float), 2),
    };

    protected override ShaderLayoutInfo[] InstanceDataLayout { get; } =
    {
        new (2, typeof(float), 2),
        new (3, typeof(float), 1),
        new (4, typeof(int), 1),
    };

    protected override List<(string SamplerName, string FilePath)> SamplerSources { get; }

    protected override string VertexShaderCode { get; } =
        @"
            #version 330 core

            // Vertex
            layout (location = 0) in vec3 position;
            layout (location = 1) in vec2 vertexUV;

            // Instance
            layout (location = 2) in vec2 offset;
            layout (location = 3) in float scale;
            layout (location = 4) in int index;

            uniform mat2 toScreenSpace;
            uniform vec2 cameraPosition;

            out vec2 UV;
            flat out int passedIndex;

            void main()
            {
                gl_Position = vec4(toScreenSpace * (scale * position.xy + offset - cameraPosition), 0.0, 1.0);
                UV = vertexUV;
                passedIndex = index;
            }
        ";

    protected override string FragmentShaderCode { get; }

    protected override Archetype Archetype => Archetype.Create<Position2D, SheetAppearance>();

    protected override IEnumerable<SheetInstanceData> GetInstanceData(int entity, IEntityStore store, TickTime time)
    {
        var sheet = store.GetAspect<SheetAppearance>(entity);
        var position = store.GetAspect<Position2D>(entity);
        yield return new() { xOffset = (float)position.X, yOffset = (float)position.Y, scale = sheet.scale, index = sheet.index };
    }

    protected override void SetUniforms(StemWindow window)
    {
        SetScreenSpaceTransform2D(window, "toScreenSpace");
        SetCameraPosition(window, "cameraPosition");
    }

    // TODO: delete this
    private Action? Replacer = null;
    
    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        if (Replacer is not null) {
            Replacer();
            Replacer = null;
        }

        base.OnTick(entities, store, time);
    }

    // TODO: delete this
    public void ReplaceTexture(Span<byte> imageBytes, int w, int h) {
        var arr = imageBytes.ToArray();
        Replacer = () => {
            ImageTextureRegistry.Update(TextureHandles[0], arr, w, h);
        };
    }
}
