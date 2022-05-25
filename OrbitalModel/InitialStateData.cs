using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;
using System.Reflection;

namespace OrbitalModel;

#nullable disable

public class InitialStateData
{
    [JsonPropertyName("scale")]
    public double Scale { get; set; }

    [JsonPropertyName("initialTime")]
    public double InitialTime { get; set; }

    [JsonPropertyName("timeStep")]
    public double TimeStep { get; set; }

    [JsonPropertyName("vectorFieldSize")]
    public double VectorFieldSize { get; set; }

    [JsonPropertyName("bodies")]
    public List<BodyData> Bodies { get; set; }

    [JsonPropertyName("orbitalBodies")]
    public List<OrbitalBodyData> OrbitalBodies { get; set; }
}

public class BodyData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    [JsonPropertyName("showTrail")]
    public bool ShowTrail { get; set; }

    [JsonPropertyName("mass")]
    public double Mass { get; set; }

    [JsonPropertyName("position")]
    public double[] Position { get; set; }

    [JsonPropertyName("velocity")]
    public double[] Velocity { get; set; }

    [JsonIgnore]
    public double Position_X => Position[0];
    [JsonIgnore]
    public double Position_Y => Position[1];
    [JsonIgnore]
    public double Position_Z => Position[2];
    [JsonIgnore]
    public Vector Position_Vector => new Vector(Position_X, Position_Y, Position_Z);

    [JsonIgnore]
    public double Velocity_X => Velocity[0];
    [JsonIgnore]
    public double Velocity_Y => Velocity[1];
    [JsonIgnore]
    public double Velocity_Z => Velocity[2];
    [JsonIgnore]
    public Vector Velocity_Vector => new Vector(Velocity_X, Velocity_Y, Velocity_Z);

    [JsonIgnore]
    public Color4 Color_Color4 => 
        Colors
        .Where(x => Color.ToLower().Replace(" ", "") == x.Key.ToLower())
        .Select(x => x.Color)
        .FirstOrDefault(Color4.HotPink);

    public static readonly List<(string Key, Color4 Color)> Colors = 
        typeof(Color4)
        .GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Select(x => (x.Name, (Color4)x.GetValue(null)))
        .ToList();
}

public class OrbitalBodyData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; }

    [JsonPropertyName("showTrail")]
    public bool ShowTrail { get; set; }

    [JsonPropertyName("reference")]
    public string Reference { get; set; }

    [JsonPropertyName("mass")]
    public double Mass { get; set; }

    [JsonPropertyName("e")]
    public double E { get; set; }

    [JsonPropertyName("a")]
    public double A { get; set; }

    [JsonPropertyName("i")]
    public double I { get; set; }

    [JsonPropertyName("Ω")]
    public double O { get; set; }

    [JsonPropertyName("ω")]
    public double W { get; set; }

    [JsonPropertyName("Tp")]
    public double Tp { get; set; }

    [JsonPropertyName("T")]
    public double T { get; set; }

    [JsonIgnore]
    public Color4 Color_Color4 =>
        BodyData.Colors
        .Where(x => Color.ToLower().Replace(" ", "") == x.Key.ToLower())
        .Select(x => x.Color)
        .FirstOrDefault(Color4.HotPink);
}