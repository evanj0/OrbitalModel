using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrbitalModel.Graphics;
using OpenTK.Mathematics;

namespace OrbitalModel;

public static class Meshes
{
    public static MeshBuilder CreateOrigin()
    {
        var red = Color4.OrangeRed;
        var green = Color4.GreenYellow;
        var blue = Color4.SkyBlue;
        var a = (0, 0, 0);
        var b = (-1, 0, 0);
        var c = (0, 1, 0);
        var d = (0, 0, 1);
        return new MeshBuilder()
            // xy plane
            .SetVertexColor(blue)
            .AddFace(a, b, c)
            // yz plane
            .SetVertexColor(red)
            .AddFace(a, c, d)
            // xz plane
            .SetVertexColor(green)
            .AddFace(a, b, d);
            
    }

    public static MeshBuilder CreateBodyMarker()
    {
        var bytes = new byte[] { 0, 0, 0, };
        new Random().NextBytes(bytes);
        var colorR = bytes[0] / 255.0f;
        var colorG = bytes[1] / 255.0f;
        var colorB = bytes[2] / 255.0f;

        var a = new Vector3(0, 0, 0);
        var b = new Vector3(1, 0, 1);
        var c = new Vector3(-1, 0, 1);
        var d = new Vector3(0, 1, 1);
        var e = new Vector3(0, -1, 1);

        return new MeshBuilder()
            .SetVertexColor(new Color4(colorR, colorG, colorB, 1))
            // sides
            .AddFace(a, b, e)
            .AddFace(a, b, d)
            .AddFace(a, c, e)
            .AddFace(a, c, d)
            // top faces
            .AddFace(b, d, c)
            .AddFace(b, e, c);
    }
}

