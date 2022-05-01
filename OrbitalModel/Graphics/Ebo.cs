using OpenTK.Graphics.OpenGL;

namespace OrbitalModel.Graphics;

public class Ebo
{
    public Ebo(int[] indices)
    {
        GL.GenBuffers(1, out int id);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, id);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(float), indices, BufferUsageHint.StaticDraw);
        Id = id;
    }

    public int Id { get; set; }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, Id);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }

    public void Delete()
    {
        GL.DeleteBuffer(Id);
    }
}
