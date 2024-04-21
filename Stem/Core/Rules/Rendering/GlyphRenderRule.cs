using Stem.Aspects;
using System.Collections.Generic;

namespace Stem.Rules.Rendering;

public struct GlyphVertexData
{
    public float x, y, z, u, v;
}

public struct GlyphInstanceData
{
    public float xOffset, yOffset, depth, scale; 
    public int textIndex;
}

public class UIGlyphRenderRule : GlyphRenderRule 
{
    protected override string VertexShaderCode { get; } =
        @"
            #version 330 core

            // Vertex
            layout (location = 0) in vec3 position;
            layout (location = 1) in vec2 vertexUV;

            // Instance
            layout (location = 2) in vec2 offset;
            layout (location = 3) in float depth;
            layout (location = 4) in float scale;
            layout (location = 5) in int textIndex;

            uniform mat2 toScreenSpace;
            uniform vec2 cameraPosition;

            out vec2 UV;
            flat out int index;

            void main()
            {
                gl_Position = vec4(toScreenSpace * (scale * position.xy + offset), depth, 1.0);
                UV = vertexUV;
                index = textIndex;
            }
        ";

    protected override Archetype Archetype => Archetype.Create<Position2D, UIGlyphAppearance>();

    protected override IEnumerable<GlyphInstanceData> GetInstanceData(int entity, IEntityStore store, TickTime time)
    {
        var position = store.GetAspect<Position2D>(entity);
        var glyph = store.GetAspect<UIGlyphAppearance>(entity);
        var text = glyph.glyphIndex;
        yield return new() { xOffset = (float)position.X, yOffset = (float)position.Y, depth = glyph.depth, scale = 20f, textIndex = text };
    }

    protected override void SetUniforms(StemWindow window)
    {
        SetScreenSpaceTransformUI(window, "toScreenSpace");
        SetCameraPosition(window, "cameraPosition");
    }
}

public class GlyphRenderRule : InstancedRenderRule<GlyphVertexData, GlyphInstanceData>
{
    protected override GlyphVertexData[] VertexData { get; } =
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
        new (4, typeof(float), 1),
        new (5, typeof(int), 1),
    };

    protected override List<(string SamplerName, string FilePath)> SamplerSources { get; } = new() 
    {
        ("image", @"Assets/FontAtlas/Inconsolata.png"),
    };

    protected override string VertexShaderCode { get; } =
        @"
            #version 330 core

            // Vertex
            layout (location = 0) in vec3 position;
            layout (location = 1) in vec2 vertexUV;

            // Instance
            layout (location = 2) in vec2 offset;
            layout (location = 3) in float depth;
            layout (location = 4) in float scale;
            layout (location = 5) in int textIndex;

            uniform mat2 toScreenSpace;
            uniform vec2 cameraPosition;

            out vec2 UV;
            flat out int index;

            void main()
            {
                gl_Position = vec4(toScreenSpace * (scale * position.xy + offset - cameraPosition), depth, 1.0);
                UV = vertexUV;
                index = textIndex;
            }
        ";

    protected override string FragmentShaderCode { get; } =
        @"
            #version 330 core

            in vec2 UV;
            flat in int index;

            uniform sampler2D image;

            out vec4 FragColor;

            void main()
            {
                int x = int(mod(index, 16));
                int y = 15 - int((index - x) / 16.0);
                FragColor = vec4(1.0, 1.0, 1.0, texture(image, vec2((UV.x + x) / 16, (UV.y + y) / 16)).a);
            }
        ";

    protected override Archetype Archetype => Archetype.Create<Position2D, GlyphAppearance>();

    protected override IEnumerable<GlyphInstanceData> GetInstanceData(int entity, IEntityStore store, TickTime time)
    {
        var position = store.GetAspect<Position2D>(entity);
        var glyph = store.GetAspect<GlyphAppearance>(entity);
        var text = glyph.glyphIndex;
        yield return new() { 
            xOffset = (float)(position.X + glyph.offset.X), 
            yOffset = (float)(position.Y + glyph.offset.Y), 
            depth = glyph.depth, 
            scale = glyph.scale, 
            textIndex = text 
        };
    }

    protected override void SetUniforms(StemWindow window)
    {
        SetScreenSpaceTransform2D(window, "toScreenSpace");
        SetCameraPosition(window, "cameraPosition");
    }
}
