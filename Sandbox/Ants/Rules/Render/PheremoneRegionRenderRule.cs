using Stem;
using Stem.Rules;
using Stem.Rules.Rendering;

namespace Sandbox.Ants.Rules.Render;

public struct PheromoneRegionVertexData
{ public float x, y, z, u, v; }

public struct PheromoneRegionInstanceData
{ public float xOffset, yOffset, scale, opacity; }

public class PheromoneRegionRenderRule : InstancedRenderRule<PheromoneRegionVertexData, PheromoneRegionInstanceData>
{
    protected override PheromoneRegionVertexData[] VertexData { get; } =
    {
        new() { x =  1f, y =  1f, z = 0.0f, u = 1.0f, v = 1.0f },  // top right
        new() { x =  1f, y =  0f, z = 0.0f, u = 1.0f, v = 0.0f },  // bottom right
        new() { x =  0f, y =  0f, z = 0.0f, u = 0.0f, v = 0.0f },  // bottom left
        new() { x =  0f, y =  1f, z = 0.0f, u = 0.0f, v = 1.0f },  // top left
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
            layout (location = 4) in float opacity;

            uniform mat2 toScreenSpace;
            uniform vec2 cameraPosition;

            out vec2 UV;
            out float Opacity;

            void main()
            {
                gl_Position = vec4(toScreenSpace * (scale * position.xy + offset - cameraPosition), 0.001, 1.0);
                UV = vertexUV;
                Opacity = opacity;
            }
        ";

    protected override string FragmentShaderCode { get; } =
        @"
            #version 330 core

            in vec2 UV;
            in float Opacity;

            out vec4 FragColor;

            void main()
            {
                //if (distance(UV, vec2(0.5, 0.5)) > 0.5) {
                //    discard;
                //}
                FragColor = vec4(0.8, 0, 0.8, Opacity);
            }
        ";

    protected override Archetype Archetype => Archetype.NoRead;

    protected override IEnumerable<PheromoneRegionInstanceData> GetInstanceData(int entity, IEntityStore store, TickTime time)
    {
        var grid = Global.Get<PheromoneGrid>();
        var keyValuePairs = grid.ToList();
        foreach (var keyValuePair in keyValuePairs)
        {
            if (keyValuePair.Value == 0)
            {
                grid.Remove(keyValuePair.Key);
                continue;
            }
            yield return new()
            {
                xOffset = 50 * keyValuePair.Key.Item1,
                yOffset = 50 * keyValuePair.Key.Item2,
                scale = 50,
                opacity = 0 * MathF.Min(keyValuePair.Value / 1000f, 0.8f)
                // TODO: do these calculations inside the shader!
            };
        }
    }

    protected override void SetUniforms(StemWindow window)
    {
        SetScreenSpaceTransform2D(window, "toScreenSpace");
        SetCameraPosition(window, "cameraPosition");
    }
}
