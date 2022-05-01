using OpenTK.Graphics.OpenGL;

namespace OrbitalModel.Graphics;

public class Vao
{
    public Vao()
    {
        GL.GenVertexArrays(1, out int id);
        Id = id;
    }

    public int Id { get; set; }

    public void LinkAttrib(Vbo vbo, int layout, int numComponents, VertexAttribPointerType type, int stride, int offset)
    {
        vbo.Bind();
        GL.VertexAttribPointer(layout, numComponents, type, false, stride, offset);
        GL.EnableVertexAttribArray(layout);
        vbo.Unbind();
    }

    public void Bind()
    {
        GL.BindVertexArray(Id);
    }

    public void Unbind()
    {
        GL.BindVertexArray(0);
    }

    public void Delete()
    {
        GL.DeleteBuffer(Id);
    }
}