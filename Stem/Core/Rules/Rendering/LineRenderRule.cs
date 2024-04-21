using Stem.Aspects;
using System;
using System.Collections.Generic;

namespace Stem.Rules.Rendering;

public struct LineVertexData
{ public float x, y, z, u, v; }

public struct LineInstanceData
{ public float theta, length, thickness, xOffset, yOffset; }

public class LineRenderRule : InstancedRenderRule<LineVertexData, LineInstanceData>
{
    protected override LineVertexData[] VertexData { get; } =
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
        new (2, typeof(float), 1),
        new (3, typeof(float), 1),
        new (4, typeof(float), 1),
        new (5, typeof(float), 2),
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
            layout (location = 5) in vec2 offset;
            layout (location = 2) in float theta;
            layout (location = 3) in float segmentLength;
            layout (location = 4) in float thickness;

            uniform mat2 toScreenSpace;
            uniform vec2 cameraPosition;
            uniform float magnification;

            out vec2 UV;

            mat2 rotate2d(float _theta)
            {
                return mat2(cos(_theta), -sin(_theta), sin(_theta), cos(_theta));
            }

            mat2 scale2d(float xScale, float yScale)
            {
                return mat2(xScale, 0, 0, yScale);
            }

            void main()
            {
                mat2 rotator = rotate2d(-theta);
                mat2 scaler = scale2d(segmentLength, thickness * magnification);
                gl_Position = vec4(toScreenSpace * (rotator * scaler * position.xy + offset - cameraPosition), 0.0, 1.0);
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
                FragColor = vec4(0.3, 0.3, 0.3, 1);
            }
        ";

    protected override Archetype Archetype => Archetype.Create<LineAppearance>();

    protected override IEnumerable<LineInstanceData> GetInstanceData(int entity, IEntityStore store, TickTime time)
    {
        // TODO: dashed lines, rounded end caps, color
        var segments = store.GetAspect<LineAppearance>(entity).segments;
        for (int i = 0; i < segments.Length; ++i)
        {
            var segment = segments[i];
            // TODO: More of this calculation can be done inside the shader to improve performance
            var dx = segment.end.x - segment.start.x;
            var dy = segment.end.y - segment.start.y;
            yield return new()
            {
                length = (float)Math.Sqrt(dx * dx + dy * dy) / 2f,
                theta = (float)Math.Atan2(dy, dx),
                thickness = 1f,
                xOffset = (float)((segment.start.x + segment.end.x) / 2f),
                yOffset = (float)((segment.start.y + segment.end.y) / 2f)
            };
        }
    }

    protected override void SetUniforms(StemWindow window)
    {
        // TODO: Refactor base class (InstancedRenderRule.cs) to automatically pass certain uniforms, such as screen size, camera position
        SetScreenSpaceTransform2D(window, "toScreenSpace");
        SetCameraPosition(window, "cameraPosition");
        shaderProgram.SetFloat("magnification", 1f / window.magnification);
    }
}
