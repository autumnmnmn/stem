using Sandbox.Ants.Aspects;
using Stem;
using Stem.Aspects;
using Stem.Rules;
using Stem.Rules.Rendering;

namespace Sandbox.Ants.Rules.Render;

public struct AntVertexData
{ public float x, y, z; }

public struct AntInstanceData
{ public float x, y, r, g, b; }

public class AntRenderRule : InstancedRenderRule<AntVertexData, AntInstanceData>
{
    protected override string VertexShaderCode { get; } =
        @"
            #version 330 core

            // Vertex
            layout (location = 0) in vec3 position;

            // Instance
            layout (location = 2) in vec2 offset;
            layout (location = 3) in vec3 instanceColor;

            out vec3 color;
            out vec2 uv;

            uniform mat2 rotationMatrix;

            uniform mat2 toScreenSpace;

            uniform vec2 cameraPosition;

            uniform float magnification;

            void main()
            {
                color = instanceColor;
                gl_Position = vec4(toScreenSpace * (position.xy * magnification + offset - cameraPosition), 0.0, 1.0);
                uv = toScreenSpace * (position.xy * magnification + offset - cameraPosition);
                uv = vec2((uv.x + 1.0) / 2.0, (uv.y + 1.0) / 2.0);
            }
        ";

    protected override string FragmentShaderCode { get; } =
        @"
            #version 330 core

            in vec3 color;
            in vec2 uv;

            uniform sampler2D assistant;

            out vec4 FragColor;

            void main()
            {
                FragColor = vec4(color, 1);
            }
        ";

    protected override Archetype Archetype => Archetype.Create<Position2D, AntAppearance>();

    protected override AntVertexData[] VertexData { get; } =
    {
        new() { x =  0.5f, y =  0.5f, z = 0.0f },  // top right
        new() { x =  0.5f, y = -0.5f, z = 0.0f },  // bottom right
        new() { x = -0.5f, y = -0.5f, z = 0.0f },  // bottom left
        new() { x = -0.5f, y =  0.5f, z = 0.0f },  // top left
    };

    protected override uint[] Indices { get; } =
    {
        0, 1, 3,   // first triangle
        1, 2, 3    // second triangle
    };

    protected override ShaderLayoutInfo[] VertexDataLayout { get; } =
    {
        new(0, typeof(float), 3),
        new(1, typeof(float), 2),
    };

    protected override ShaderLayoutInfo[] InstanceDataLayout { get; } =
    {
        new(2, typeof(float), 2),
        new(3, typeof(float), 3),
    };

    protected override List<(string SamplerName, string FilePath)> SamplerSources { get; } = new() 
    {
        ("assistant", @"Assets/assistant.jpg"),
    };

    protected override IEnumerable<AntInstanceData> GetInstanceData(int entity, IEntityStore store, TickTime time)
    {
        var position = store.GetAspect<Position2D>(entity);
        //var appearance = store.GetAspect<AntAppearance>(entity);

        //var wander = store.GetAspect<WanderMovement>(entity);
        var r = 0.9f; //(MathF.Cos(wander.Direction) + 1f) / 2f;
        var g = 0.9f; //(MathF.Sin(wander.Direction) + 1f) / 2f;
        var b = 0.9f; //MathF.Sqrt(1f - r * r - g * g);

        if (store.HasAspect<DirectMovement>(entity))
        {
            r = 0.1f;
            g = 1f;
            b = 0.1f;
        }

        yield return new()
        {
            x = (float) position.X,
            y = (float) position.Y,
            r = r,
            g = g,
            b = b
        };
    }

    private float __theta = 0.0f;

    protected override void SetUniforms(StemWindow window)
    {
        shaderProgram.SetMatrix2("rotationMatrix",
            new()
            {
                M11 = MathF.Cos(__theta),
                M12 = -MathF.Sin(__theta),
                M21 = MathF.Sin(__theta),
                M22 = MathF.Cos(__theta)
            }
        );
        __theta += 0.01f;

        SetScreenSpaceTransform2D(window, "toScreenSpace");
        SetCameraPosition(window, "cameraPosition");
        shaderProgram.SetFloat("magnification", 1f / window.magnification);
    }
}
