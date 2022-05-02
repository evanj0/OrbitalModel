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
        var bytes = new byte[] { 0, 0, 0, };
        new Random().NextBytes(bytes);
        var r = bytes[0] / 255.0f;
        var g = bytes[1] / 255.0f;
        var b = bytes[2] / 255.0f;

        var vertices = new float[]
        {
            0, 0, 0,    r, g, b,

            1, 0, 1,    r, g, b,
            -1, 0, 1,   r, g, b,

            0, 1, 1,    r, g, b,
            0, -1, 1,   r, g, b,
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
        var matrix = Matrix4.CreateScale(0.1f * (float)Mass + 0.1f) * Matrix4.CreateTranslation((float)Position.X, (float)Position.Y, (float)Position.Z);
        _renderer.Render(shader, camera, matrix);
    }
}
