using Stem.Aspects;
using System.Collections.Generic;

namespace Stem.Rules.Rendering;

public class Perspective3DRenderRule : InstancedRenderRule<Simple3DVertexData, Simple3DInstanceData>
{
    private const float D = 1f;
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
        new (4, typeof(float), 1),
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
            layout (location = 3) in float rotation;
            layout (location = 4) in float scale;

            uniform vec2 cameraPosition;
            uniform mat4 cameraToPerspective;

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
                vec3 rotatedLocal = yrot(rotation) * xrot((rotation + 3.14159) / 2) * position * scale;
                vec3 truePosition = rotatedLocal + offset;
                vec4 cameraSpacePosition = vec4((truePosition.xy - cameraPosition / 100), truePosition.z, 1);
                gl_Position = cameraToPerspective * cameraSpacePosition;
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
        __theta += (float) time.dt;
        yield return new() { X = 0, Y = 0, Z = -10f, rotation = __theta * 3.14159f / 4f, scale = 1f };
        yield return new() { X = 3, Y = 3, Z = -5f, rotation = __theta * 3.14159f / 4.1f, scale = 1f };
        yield return new() { X = -6, Y = -4, Z = -15f, rotation = __theta * 3.14159f / 4.2f, scale = 1f };
        yield return new() { X = 5, Y = -7, Z = -9f, rotation = __theta * 3.14159f / 4.3f, scale = 1f };
        yield return new() { X = -2, Y = 9, Z = -7f, rotation = __theta * 3.14159f / 4.4f, scale = 2f };
        yield return new() { X = -2, Y = 9, Z = -100f, rotation = __theta * 3.14159f / 4.7f, scale = 2f };
        yield return new() { X = -2, Y = 9, Z = -200f, rotation = __theta * 3.14159f / 5.0f, scale = 2f };
        yield return new() { X = -2, Y = 9, Z = -400f, rotation = __theta * 3.14159f / 5.3f, scale = 2f };
        yield return new() { X = -2, Y = 9, Z = -800f, rotation = __theta * 3.14159f / 5.7f, scale = 2f };
    }

    protected override void SetUniforms(StemWindow window)
    {
        SetCameraToPerspectiveMatrix4D(window, "cameraToPerspective", 1000, 1, 1000);
        SetCameraPosition(window, "cameraPosition");
    }
}
