using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalModel.Graphics;

public class Mesh
{
    public float[] Vertices { get; set; }
    public int[] Indices { get; set; }
    public Shader Shader { get; set; }

    private Vao _vao;

    public Mesh(float[] vertices, int[] indices, Shader shader)
    {
        Vertices = vertices;
        Indices = indices;
        Shader = shader;

        _vao = new Vao();
        _vao.Bind();
        var vbo1 = new Vbo(vertices);
        var ebo1 = new Ebo(indices);

        _vao.LinkAttrib(vbo1, 0, 3, VertexAttribPointerType.Float, 6 * sizeof(float), 0);
        _vao.LinkAttrib(vbo1, 1, 3, VertexAttribPointerType.Float, 6 * sizeof(float), 3 * sizeof(float));
        _vao.Unbind();
        vbo1.Unbind();
        ebo1.Unbind();

        _vao.Unbind();
    }

    public void Render(Camera camera, Matrix4 transform)
    {
        Shader.Activate();
        var matrix = transform * camera.GetMatrix();
        var cameraUniform = GL.GetUniformLocation(shader, "camera");
        GL.UniformMatrix4(cameraUniform, false, ref matrix);
        _vao.Bind();
        GL.DrawElements(PrimitiveType.Triangles, Vertices.Length, DrawElementsType.UnsignedInt, 0);
        _vao.Unbind();
    }
}
