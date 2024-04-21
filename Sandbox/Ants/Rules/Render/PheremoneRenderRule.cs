using Sandbox.Ants.Aspects;
using Stem;
using Stem.Aspects;
using Stem.Rules;
using Stem.Rules.Rendering;

namespace Sandbox.Ants.Rules.Render;

public struct PheromoneVertexData
{ public float x, y, z, u, v; }

public struct PheromoneInstanceData
{ public float xOffset, yOffset, scale; }

public class PheromoneRenderRule : InstancedRenderRule<PheromoneVertexData, PheromoneInstanceData>
{
    protected override PheromoneVertexData[] VertexData { get; } =
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
                float scaleAdjusted = max(2, scale);
                gl_Position = vec4(toScreenSpace * (scaleAdjusted * position.xy + offset - cameraPosition), 0.001, 1.0);
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
                FragColor = vec4(0.8, 0, 0.8, 0.01);
            }
        ";

    protected override Archetype Archetype => Archetype.Create<Position2D, Pheromone>();

    protected override IEnumerable<PheromoneInstanceData> GetInstanceData(int entity, IEntityStore store, TickTime time)
    {
        var position = store.GetAspect<Position2D>(entity);
        var strength = store.GetAspect<Pheromone>(entity).strength;
        yield return new() { xOffset = (float) position.X, yOffset = (float) position.Y, scale = strength / 500f };
    }

    protected override void SetUniforms(StemWindow window)
    {
        SetScreenSpaceTransform2D(window, "toScreenSpace");
        SetCameraPosition(window, "cameraPosition");
    }
}
