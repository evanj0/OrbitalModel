using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalModel.Graphics;

public class Shader : IDisposable
{
    private int _program;

    public Shader(int program)
    {
        _program = program;
    }

    public void Activate()
    {
        GL.UseProgram(_program);
    }

    public int GetUniformLocation(string name)
    {
        return GL.GetUniformLocation(_program, name);
    }

    public void Dispose()
    {
        GL.DeleteProgram(_program);
    }
}

public class ShaderBuilder
{
    private List<int> _shaders;

    public ShaderBuilder()
    {
        _shaders = new List<int>();
    }

    public ShaderBuilder AddVertex(string source)
    {
        var shader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(shader, source);
        _shaders.Add(shader);
        return this;
    }

    public ShaderBuilder AddFragment(string source)
    {
        var shader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(shader, source);
        _shaders.Add(shader);
        return this;
    }

    public ShaderBuilder AddVertexFromFile(string path)
    {
        return AddVertex(File.ReadAllText(path));
    }

    public ShaderBuilder AddFragmentFromFile(string path)
    {
        return AddFragment(File.ReadAllText(path));
    }

    public Shader Compile()
    {
        var program = GL.CreateProgram();
        foreach (var shader in _shaders)
        {
            GL.CompileShader(shader);
            GL.AttachShader(program, shader);
        }
        GL.LinkProgram(program);
        foreach (var shader in _shaders)
        {
            GL.DetachShader(program, shader);
            GL.DeleteShader(shader);
        }
        return new Shader(program);
    }
}