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
    private bool _showTrail;
    private List<Vector> _positions;
    public ref bool ShowTrailRef => ref _showTrail;

    public Body(double mass, Vector position, Vector velocity, Mesh mesh, string name, Color4 color)
    {
        Mass = mass;
        Velocity = velocity;
        Position = position;
        _mesh = new MeshBuilder()
            .SetVertexColor(color)
            .CreateCenteredCube()
            .Scale(0.2f)
            .CreateMesh(mesh.Shader);
        Name = name;
        Color = color;
        _arrowMesh = new MeshBuilder()
            .SetVertexColor(Color4.Yellow)
            .CreateArrow()
            .Scale(0.1f, 0.1f, 1)
            .Translate(0, 0, 1)
            .CreateMesh(mesh.Shader);
        _positions = new List<Vector>();
    }

    public void Render(Camera camera, Matrix4 transform, float size)
    {
        var scale = Matrix4.CreateScale(0.1f * (float)Mass + 0.075f);
        var matrix = Matrix4.CreateScale(size) * Matrix4.CreateTranslation(Position) * transform;
        _mesh.Render(camera, matrix);
        if (_showVelocity)
        {
            var w = Velocity.Normalized();
            var v = Vector.Cross(w, Position).Normalized();
            var u = Vector.Cross(w, v).Normalized();
            var coordTransform = new Matrix4(
                ((float)u.X, (float)v.X, (float)w.X, 0),
                ((float)u.Y, (float)v.Y, (float)w.Y, 0),
                ((float)u.Z, (float)v.Z, (float)w.Z, 0),
                (0, 0, 0, 1));
            coordTransform.Invert();
            _arrowMesh.Render(camera, coordTransform * Matrix4.CreateTranslation(Position * (1 / size)));
        }
        if (_showTrail)
        {
            _positions.Add(Position);
            foreach (var position in _positions)
            {
                _mesh.Render(camera, Matrix4.CreateScale(0.2f) * Matrix4.CreateScale(size) * Matrix4.CreateTranslation(position) * transform);
            }
            if (_positions.Count > 10000)
            {
                _positions.RemoveAt(0);
            }
        }
    }
}
