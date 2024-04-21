using Sandbox.Ants.Aspects;
using Stem;
using Stem.Aspects;
using Stem.Rules;
using Stem.Rules.Rendering;

namespace Sandbox.Ants.Rules.Render;

public struct FoodVertexData
{ public float x, y, z, u, v; }

public struct FoodInstanceData
{ public float x, y, scale; }

public class FoodRenderRule : InstancedRenderRule<FoodVertexData, FoodInstanceData>
{
    protected override string VertexShaderCode { get; } =
        @"
            #version 330 core

            // Vertex
            layout (location = 0) in vec3 position;
            layout (location = 1) in vec2 vertexUV;

            // Instance
            layout (location = 2) in vec2 offset;
            layout (location = 3) in float scale;

            uniform mat2 toScreenSpace;

            uniform vec2 cameraPosition;

            out vec2 UV;

            void main()
            {
                gl_Position = vec4(toScreenSpace * (scale * position.xy + offset - cameraPosition), 0.0, 1.0);
                UV = vertexUV;
            }
        ";

    protected override string FragmentShaderCode { get; } =
        @"
            #version 330 core

            in vec2 UV;

            out vec4 FragColor;

            void main()
            {
                if (distance(UV, vec2(0.5, 0.5)) > 0.5) {
                    discard;
                }
                FragColor = vec4(0, 0.5, 0, 0.5);
            }
        ";

    protected override Archetype Archetype => Archetype.Create<Position2D, FoodAppearance, Nutrients>();

    protected override FoodVertexData[] VertexData { get; } =
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
    };

    protected override List<(string SamplerName, string FilePath)> SamplerSources { get; } = new() 
    {
    };

    protected override IEnumerable<FoodInstanceData> GetInstanceData(int entity, IEntityStore store, TickTime time)
    {
        var position = store.GetAspect<Position2D>(entity);
        var scale = store.GetAspect<Nutrients>(entity).calories;
        yield return new() { x = (float) position.X, y = (float) position.Y, scale = scale / 333f };
    }

    protected override void SetUniforms(StemWindow window)
    {
        SetScreenSpaceTransform2D(window, "toScreenSpace");
        SetCameraPosition(window, "cameraPosition");
    }
}
