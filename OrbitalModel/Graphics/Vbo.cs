using OpenTK.Graphics.OpenGL;

namespace OrbitalModel.Graphics;

public class Vbo
{
    public Vbo(float[] vertices)
    {
        GL.GenBuffers(1, out int id);
        GL.BindBuffer(BufferTarget.ArrayBuffer, id);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
        Id = id;
    }

    public int Id { get; set; }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void Delete()
    {
        GL.DeleteBuffer(Id);
    }
}
