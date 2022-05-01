using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalModel;

public static class Model
{
    public static Vector Acceleration(double g, Vector p, IEnumerable<Body> bodies)
    {
        var sum = Vector.Zero;
        foreach (var body in bodies)
        {
            var x = body.Position - p;
            var r = x.Mag;
            if (r == 0) continue;
            var a = x.Norm * ((g * body.Mass) / (r * r));
            sum += a;
        }
        return sum;
    }

    public static void Step(double g, double dt, IEnumerable<Body> bodies)
    {
        foreach (var body in bodies)
        {
            var a = Acceleration(g, body.Position, bodies);
            body.Velocity += a * dt;
        }
        foreach (var body in bodies)
        {
            body.Position += body.Velocity * dt;
        }
    }
}

public class Body
{
    public double Mass { get; set; }
    public Vector Velocity { get; set; }
    public Vector Position { get; set; }

    public Body(double mass, Vector position, Vector velocity)
    {
        Mass = mass;
        Velocity = velocity;
        Position = position;
    }
}

public struct Vector
{
    public double X;
    public double Y;
    public double Z;

    public static Vector Zero => new Vector(0, 0, 0);

    public Vector(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public double Dot(Vector v) => X * v.X + Y * v.Y + Z * v.Z;

    public double Mag => Math.Sqrt(X * X + Y * Y + Z * Z);

    public Vector Norm => this / Mag;

    public static Vector operator +(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector operator -(Vector a, Vector b) => new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector operator *(Vector v, double s) => new Vector(v.X * s, v.Y * s, v.Z * s);

    public static Vector operator *(double s, Vector v) => v * s;

    public static Vector operator /(Vector v, double s) => new Vector(v.X / s, v.Y / s, v.Z / s);

    public static implicit operator Vector((double, double, double) x) => new Vector(x.Item1, x.Item2, x.Item3);

    public override string ToString() => $"〈{X}, {Y}, {Z}〉";
}
