using System.Collections.Generic;
using Stem.Aspects;
using Stem.Rules.Rendering;

namespace Stem.Rules.UI.Rendering;

public struct UIRectVertexData
{
    public float x, y, z, u, v;
}

public struct UIRectInstanceData
{
    public float xOffset, yOffset, xScale, yScale, r, g, b;
}

public struct UIRectAppearance
{
    public float xScale, yScale;
    public float r, g, b;
}

public class UIRectRenderRule : InstancedRenderRule<UIRectVertexData, UIRectInstanceData>
{
    protected override UIRectVertexData[] VertexData { get; } =
    {
        new() { x =  0.5f, y =  0.5f, z = 0.0f, u = 1.0f, v = 1.0f },  // top right
        new() { x =  0.5f, y = -0.5f, z = 0.0f, u = 1.0f, v = 0.0f },  // bottom right
        new() { x = -0.5f, y = -0.5f, z = 0.0f, u = 0.0f, v = 0.0f },  // bottom left
        new() { x = -0.5f, y =  0.5f, z = 0.0f, u = 0.0f, v = 1.0f },  // top left
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
        new (3, typeof(float), 2),
        new (4, typeof(float), 3),
    };

    protected override List<(string SamplerName, string FilePath)> SamplerSources { get; } = new() 
    {
        ("image", @"Assets/emoji.jpg"),
    };

    protected override string VertexShaderCode { get; } =
        @"
            #version 330 core

            // Vertex
            layout (location = 0) in vec3 position;
            layout (location = 1) in vec2 vertexUV;

            // Instance
            layout (location = 2) in vec2 offset;
            layout (location = 3) in vec2 scale;
            layout (location = 4) in vec3 color;

            uniform mat2 toScreenSpace;
            uniform vec2 cameraPosition;

            out vec2 UV;
            out vec3 passColor;

            void main()
            {
                vec2 xy = vec2(position.x * scale.x, position.y * scale.y) + offset;
                gl_Position = vec4(toScreenSpace * xy, 0.5, 1.0);
                UV = vertexUV;
                passColor = color;
            }
        ";

    protected override string FragmentShaderCode { get; } =
        @"
            #version 330 core

            in vec2 UV;
            in vec3 passColor;

            uniform sampler2D image;

            out vec4 FragColor;

            void main()
            {
                FragColor = vec4(passColor, 1); //texture(image, UV);
            }
        ";

    protected override Archetype Archetype => Archetype.Create<Position2D, UIRectAppearance>();

    protected override IEnumerable<UIRectInstanceData> GetInstanceData(int entity, IEntityStore store, TickTime time)
    {
        var position = store.GetAspect<Position2D>(entity);
        var appearance = store.GetAspect<UIRectAppearance>(entity);
        yield return new()
        {
            xOffset = (float) position.X,
            yOffset = (float) position.Y,
            xScale = appearance.xScale,
            yScale = appearance.yScale,
            r = appearance.r,
            g = appearance.g,
            b = appearance.b
        };
    }

    protected override void SetUniforms(StemWindow window)
    {
        SetScreenSpaceTransformUI(window, "toScreenSpace");
        SetCameraPosition(window, "cameraPosition");
    }
}
