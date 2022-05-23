using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalModel;

public static class Model
{
    public static Vector Acceleration(float g, Vector p, IEnumerable<Body> bodies)
    {
        var sum = Vector.Zero;
        foreach (var body in bodies)
        {
            var x = body.Position - p;
            var r = x.Length;
            if (r == 0) continue;
            var a = x.Normalized() * ((g * body.Mass) / (r * r));
            sum += a;
        }
        return sum;
    }

    public static Vector Acceleration(this IEnumerable<Body> bodies, float g, Vector p)
    {
        return Acceleration(g, p, bodies);
    }

    public static void Step(float g, float dt, IEnumerable<Body> bodies)
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

    public static void UpdateForceVectorField(IEnumerable<Body> bodies, VectorField vectorField, float g)
    {
        vectorField.UpdateVectors((pos, vec) => bodies.Acceleration(g, pos));
    }

    public static Vector CenterOfMass(this IEnumerable<Body> bodies)
    {
        var mx = 0.0;
        var my = 0.0;
        var mz = 0.0;
        var mass = 0.0;
        foreach (var body in bodies)
        {
            mx += body.Mass * body.Position.X;
            my += body.Mass * body.Position.Y;
            mz += body.Mass * body.Position.Z;
            mass += body.Mass;
        }
        var com = new Vector(mx, my, mz) / mass;
        return com;
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

    public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

    public Vector Normalized() => this / Length;

    public static Vector Cross(Vector a, Vector b) => new Vector(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X);

    public static Vector operator +(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector operator -(Vector a, Vector b) => new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector operator *(Vector v, double s) => new Vector(v.X * s, v.Y * s, v.Z * s);

    public static Vector operator *(double s, Vector v) => v * s;

    public static Vector operator /(Vector v, double s) => new Vector(v.X / s, v.Y / s, v.Z / s);

    public static implicit operator Vector((double, double, double) x) => new Vector(x.Item1, x.Item2, x.Item3);

    public static implicit operator Vector3(Vector v) => new Vector3((float)v.X, (float)v.Y, (float)v.Z);

    public static implicit operator Vector(Vector3 v) => new Vector(v.X, v.Y, v.Z);

    public override string ToString() => $"〈{X}, {Y}, {Z}〉";
}
