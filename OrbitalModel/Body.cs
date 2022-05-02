using OpenTK.Mathematics;
using OrbitalModel.Graphics;

namespace OrbitalModel;

public class Body
{
    public double Mass { get; set; }
    public Vector Velocity { get; set; }
    public Vector Position { get; set; }
    private Mesh _mesh;

    public Body(double mass, Vector position, Vector velocity, Mesh mesh)
    {
        Mass = mass;
        Velocity = velocity;
        Position = position;
        _mesh = mesh;
    }

    public void Render(int shader, Camera camera)
    {
        var matrix = Matrix4.CreateScale(0.1f * (float)Mass + 0.075f) * Matrix4.CreateTranslation((float)Position.X, (float)Position.Y, (float)Position.Z);
        _mesh.Render(camera, matrix);
    }
}
