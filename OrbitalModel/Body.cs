using OpenTK.Mathematics;
using OrbitalModel.Graphics;

namespace OrbitalModel;

public class Body
{
    public float Mass { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 Position { get; set; }
    private Mesh _mesh;

    public Body(float mass, Vector3 position, Vector3 velocity, Mesh mesh)
    {
        Mass = mass;
        Velocity = velocity;
        Position = position;
        _mesh = mesh;
    }

    public void Render(Camera camera)
    {
        var matrix = Matrix4.CreateScale(0.1f * (float)Mass + 0.075f) * Matrix4.CreateTranslation(Position);
        _mesh.Render(camera, matrix);
    }
}
