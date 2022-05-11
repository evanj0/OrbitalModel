using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;

namespace OrbitalModel;

#nullable disable

[JsonSerializable(typeof(InitialStateData))]
public class InitialStateData
{
    [JsonPropertyName("scale")]
    public float Scale

    [JsonPropertyName("bodies")]
    public List<BodyData> Bodies { get; set; }

}

[JsonSerializable(typeof(BodyData))]
public class BodyData
{
    [JsonPropertyName("mass")]
    public float Mass { get; set; }

    [JsonPropertyName("position")]
    public VectorData Position { get; set; }

    [JsonPropertyName("velocity")]
    public VectorData Velocity { get; set; }
}

[JsonSerializable(typeof(VectorData))]
public class VectorData
{
    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    [JsonPropertyName("z")]
    public float Z { get; set; }

    public Vector3 ToVector3() => new Vector3(X, Y, Z);
}
