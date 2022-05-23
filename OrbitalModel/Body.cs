using OpenTK.Mathematics;
using OrbitalModel.Graphics;

namespace OrbitalModel;

public class Body
{
    public double Mass { get; set; }
    public Vector Velocity { get; set; }
    public string Name { get; }
    public Color4 Color { get; }
    public Vector Position { get; set; }
    private bool _showVelocity;
    public ref bool ShowVeloctiyRef => ref _showVelocity;
    public bool ShowVeloctiy { set => _showVelocity = value; }
    private Mesh _mesh;
    private Mesh _arrowMesh;

    public Body(double mass, Vector position, Vector velocity, Mesh mesh, string name, Color4 color)
    {
        Mass = mass;
        Velocity = velocity;
        Position = position;
        _mesh = mesh;
        Name = name;
        Color = color;
        _arrowMesh = new MeshBuilder()
            .SetVertexColor(Color4.Yellow)
            .CreateArrow()
            .Scale(0.1f, 0.1f, 1)
            .CreateMesh(mesh.Shader);
    }

    public void Render(Camera camera, Matrix4 transform, float size)
    {
        var scale = Matrix4.CreateScale(0.1f * (float)Mass + 0.075f);
        var matrix = Matrix4.CreateScale(size) * Matrix4.CreateTranslation(Position) * transform;
        _mesh.Render(camera, matrix);
        if (_showVelocity)
        {
            var u = Velocity.Normalized();
            var v = Vector.Cross(u, Position).Normalized();
            var w = Vector.Cross(v, u).Normalized();
            var coordTransform = new Matrix4(
                ((float)u.X, (float)v.X, (float)w.X, 0),
                ((float)u.Y, (float)v.Y, (float)w.Y, 0),
                ((float)u.Z, (float)v.Z, (float)w.Z, 0),
                (0, 0, 0, 1));
            coordTransform.Invert();
            _arrowMesh.Render(camera, coordTransform * Matrix4.CreateTranslation(Position * (1 / size)));
        }
    }
}
