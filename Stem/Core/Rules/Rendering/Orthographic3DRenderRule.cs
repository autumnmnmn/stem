using Stem.Aspects;
using System.Collections.Generic;

namespace Stem.Rules.Rendering;

public class Orthographic3DRenderRule : InstancedRenderRule<Simple3DVertexData, Simple3DInstanceData>
{
    private const float D = 0.5f;
    private const float Y_TOP = D;
    private const float Y_BOTTOM = -D;
    private const float X_RIGHT = D;
    private const float X_LEFT = -D;
    private const float Z_BACK = D;
    private const float Z_FRONT = -D;

    protected override Simple3DVertexData[] VertexData { get; } =
    {
        new() { X = X_RIGHT, Y = Y_TOP,    Z = Z_FRONT, U = 1.0f, V = 1.0f },
        new() { X = X_RIGHT, Y = Y_BOTTOM, Z = Z_FRONT, U = 1.0f, V = 0.0f },
        new() { X = X_LEFT,  Y = Y_BOTTOM, Z = Z_FRONT, U = 0.0f, V = 0.0f },
        new() { X = X_LEFT,  Y = Y_TOP,    Z = Z_FRONT, U = 0.0f, V = 1.0f },

        new() { X = X_RIGHT, Y = Y_TOP,    Z = Z_BACK, U = 0.0f, V = 0.0f },
        new() { X = X_RIGHT, Y = Y_BOTTOM, Z = Z_BACK, U = 0.0f, V = 1.0f },
        new() { X = X_LEFT,  Y = Y_BOTTOM, Z = Z_BACK, U = 1.0f, V = 1.0f },
        new() { X = X_LEFT,  Y = Y_TOP,    Z = Z_BACK, U = 1.0f, V = 0.0f },
    };

    protected override uint[] Indices { get; } =
    {
        0, 1, 3,   // first triangle,  front
        1, 2, 3,   // second triangle, front

        7, 6, 4,   // first triangle,  back
        6, 5, 4,   // second triangle, back

        3, 2, 7,   // first triangle,  left
        2, 6, 7,   // second triangle, left

        4, 5, 0,   // first triangle,  right
        5, 1, 0,   // second triangle, right

        4, 0, 7,   // first triangle,  top
        0, 3, 7,   // second triangle, top

        1, 5, 2,   // first triangle,  bottom
        5, 6, 2,   // second triangle, bottom
    };

    protected override ShaderLayoutInfo[] VertexDataLayout { get; } =
    {
        new (0, typeof(float), 3),
        new (1, typeof(float), 2),
    };

    protected override ShaderLayoutInfo[] InstanceDataLayout { get; } =
    {
        new (2, typeof(float), 3),
        new (3, typeof(float), 1),
    };

    protected override List<(string SamplerName, string FilePath)> SamplerSources { get; } = new() 
    {
        ("assistant", @"Assets/assistant.jpg"),
    };

    protected override string VertexShaderCode { get; } =
        @"
            #version 330 core

            // Vertex
            layout (location = 0) in vec3 position;
            layout (location = 1) in vec2 vertexUV;

            // Instance
            layout (location = 2) in vec3 offset;
            layout (location = 3) in float yRotation;

            uniform mat2 toScreenSpace;
            uniform vec2 cameraPosition;

            out vec2 UV;

            mat3 yrot(float theta)
            {
                return mat3(cos(theta), 0, -sin(theta), 0, 1, 0, sin(theta), 0, cos(theta));
            }

            mat3 xrot(float theta)
            {
                return mat3(1, 0, 0, 0, cos(theta), -sin(theta), 0, sin(theta), cos(theta));
            }

            void main()
            {
                vec3 worldPosition = yrot(yRotation) * xrot((yRotation + 3.14159) / 2.0) * position * 200f + offset;
                gl_Position = vec4(toScreenSpace * (worldPosition.xy - cameraPosition), worldPosition.z / 1000f, 1.0);
                UV = vertexUV;
            }
        ";

    protected override string FragmentShaderCode { get; } =
        @"
            #version 330 core

            in vec2 UV;

            uniform sampler2D assistant;

            out vec4 FragColor;

            void main()
            {
                FragColor = texture(assistant, UV);
            }
        ";

    protected override Archetype Archetype => Archetype.NoRead;

    private float __theta = 0f;

    protected override IEnumerable<Simple3DInstanceData> GetInstanceData(int entity, IEntityStore store, TickTime time)
    {
        __theta += (float)time.dt;
        yield return new() { X = 0, Y = 0, Z = 100f, rotation = __theta * 3.14159f / 4f };
    }

    protected override void SetUniforms(StemWindow window)
    {
        SetScreenSpaceTransform2D(window, "toScreenSpace");
        SetCameraPosition(window, "cameraPosition");
    }
}
