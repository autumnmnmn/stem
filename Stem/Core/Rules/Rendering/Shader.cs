using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

#nullable enable

namespace Stem.Rules.Rendering;

public class Shader : IDisposable
{
    public int ProgramID { get; private set; }

    public Shader()
    {
    }

    public void Use()
    {
        GL.UseProgram(ProgramID);
    }

    public void Compile(string vertexSource, string fragmentSource, string? geometrySource = null)
    {
        ProgramID = GL.CreateProgram();

        int vertex = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertex, vertexSource);
        GL.CompileShader(vertex);

        string infoLogVertex = GL.GetShaderInfoLog(vertex);
        if (infoLogVertex != string.Empty)
        {
            Console.WriteLine(infoLogVertex);
        }

        GL.AttachShader(ProgramID, vertex);

        int fragment = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragment, fragmentSource);
        GL.CompileShader(fragment);

        string infoLogFragment = GL.GetShaderInfoLog(fragment);
        if (infoLogFragment != string.Empty)
        {
            Console.WriteLine(infoLogFragment);
        }

        GL.AttachShader(ProgramID, fragment);

        int? geometry = null;
        if (geometrySource is not null)
        {
            geometry = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(geometry.Value, geometrySource);
            GL.CompileShader(geometry.Value);

            string infoLogGeometry = GL.GetShaderInfoLog(geometry.Value);
            if (infoLogGeometry != string.Empty)
            {
                Console.WriteLine(infoLogGeometry);
            }

            GL.AttachShader(ProgramID, geometry.Value);
        }

        GL.LinkProgram(ProgramID);

        GL.DetachShader(ProgramID, vertex);
        GL.DetachShader(ProgramID, fragment);
        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);
        if (geometry is not null)
        {
            GL.DetachShader(ProgramID, geometry.Value);
            GL.DeleteShader(geometry.Value);
        }
    }

    public void SetFloat(string name, float value, bool useShader = false)
    {
        if (useShader)
        {
            Use();
        }
        GL.Uniform1(GL.GetUniformLocation(ProgramID, name), value);
    }

    public void SetInt(string name, int value, bool useShader = false)
    {
        if (useShader)
        {
            Use();
        }
        GL.Uniform1(GL.GetUniformLocation(ProgramID, name), value);
    }

    public void SetFloat2(string name, Vector2 value, bool useShader = false)
    {
        if (useShader)
        {
            Use();
        }
        GL.Uniform2(GL.GetUniformLocation(ProgramID, name), value);
    }

    public void SetInt2(string name, Vector2i value, bool useShader = false)
    {
        if (useShader)
        {
            Use();
        }
        GL.Uniform2(GL.GetUniformLocation(ProgramID, name), value);
    }

    public void SetFloat3(string name, Vector3 value, bool useShader = false)
    {
        if (useShader)
        {
            Use();
        }
        GL.Uniform3(GL.GetUniformLocation(ProgramID, name), value);
    }

    public void SetInt3(string name, Vector3i value, bool useShader = false)
    {
        if (useShader)
        {
            Use();
        }
        GL.Uniform3(GL.GetUniformLocation(ProgramID, name), value);
    }

    public void SetFloat4(string name, Vector4 value, bool useShader = false)
    {
        if (useShader)
        {
            Use();
        }
        GL.Uniform4(GL.GetUniformLocation(ProgramID, name), value);
    }

    public void SetInt4(string name, Vector4i value, bool useShader = false)
    {
        if (useShader)
        {
            Use();
        }
        GL.Uniform4(GL.GetUniformLocation(ProgramID, name), value);
    }

    public void SetMatrix4(string name, Matrix4 value, bool useShader = false)
    {
        if (useShader)
        {
            Use();
        }
        GL.UniformMatrix4(GL.GetUniformLocation(ProgramID, name), false, ref value);
    }

    public void SetMatrix3(string name, Matrix3 value, bool useShader = false)
    {
        if (useShader)
        {
            Use();
        }
        GL.UniformMatrix3(GL.GetUniformLocation(ProgramID, name), false, ref value);
    }

    public void SetMatrix2(string name, Matrix2 value, bool useShader = false)
    {
        if (useShader)
        {
            Use();
        }
        GL.UniformMatrix2(GL.GetUniformLocation(ProgramID, name), false, ref value);
    }

    private bool __isDisposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!__isDisposed)
        {
            GL.DeleteProgram(ProgramID);

            __isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
