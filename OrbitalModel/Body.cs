using OpenTK.Mathematics;
using OrbitalModel.Graphics;

namespace OrbitalModel;

public class Body
{
    public double Mass { get; set; }
    public Vector Velocity { get; set; }
    public Vector Position { get; set; }
    private Renderer _renderer;

    public Body(double mass, Vector position, Vector velocity)
    {
        Mass = mass;
        Velocity = velocity;
        Position = position;

        var vertices = new float[]
        {
            0, 0, 0,    1, 0, 0,

            1, 0, 1,    1, 0, 0,
            -1, 0, 1,   1, 0, 0,

            0, 1, 1,    1, 0, 0,
            0, -1, 1,   1, 0, 0,
        };

        var indices = new int[]
        {
            0, 1, 4,
            0, 1, 3,
            0, 2, 4,
            0, 2, 3,
        };

        _renderer = new Renderer(vertices, indices);
    }

    public void Render(int shader, Camera camera)
    {
        var matrix = Matrix4.CreateScale(0.1f) * Matrix4.CreateTranslation((float)Position.X, (float)Position.Y, (float)Position.Z);
        _renderer.Render(shader, camera, matrix);
    }
}
