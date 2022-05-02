using OpenTK.Mathematics;
using OrbitalModel.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalModel;

public class VectorField
{
    public VectorField(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax, float spacing, Shader shader)
    {
        _vectors = new();

        for (var x = xMin; x <= xMax; x += spacing)
        {
            for (var y = yMin; y <= yMax; y += spacing)
            {
                for (var z = zMin; z <= zMax; z += spacing)
                {
                    Console.WriteLine($"{x},{y},{z}");
                    _vectors.Add(((x, y, z), (0, 0, 0)));
                }
            }
        }

        _arrow = Meshes.CreateArrow()
            .ScaleXY(0.01f)
            .CreateMesh(shader);
    }

    private List<(Vector3, Vector3)> _vectors;

    private Mesh _arrow;

    public void UpdateVectors(Func<Vector3, Vector3, Vector3> mapping)
    {
        for (var i = 0; i < _vectors.Count; i++)
        {
            _vectors[i] = (_vectors[i].Item1, mapping(_vectors[i].Item1, _vectors[i].Item2));
        }
    }

    public void Render(Camera camera)
    {
        foreach ((var pos, var direction) in _vectors)
        {
            var w = direction;
            var u = Vector3.Cross(Vector3.UnitZ, w);
            var v = Vector3.Cross(w, u);
            u.NormalizeFast();
            v.NormalizeFast();
            w.NormalizeFast();
            var translation = Matrix4.CreateTranslation(pos);
            var scale = Matrix4.CreateScale((1, 1, direction.LengthFast * 0.25f + 0.25f));
            var coordTransform = new Matrix4(
                (u.X, v.X, w.X, 0),
                (u.Y, v.Y, w.Y, 0),
                (u.Z, v.Z, w.Z, 0),
                (0,   0,   0,   1));
            coordTransform.Invert();
            _arrow.Render(camera, coordTransform * (translation * scale));
        }
    }
}
