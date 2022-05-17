using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalModel.Graphics;

public class OrbitalBody
{
    public float Inclination { get; set; } // i
    public float LongitudeOfAscendingNode { get; set; } // Ω
    public float ArgumentOfPeriapsis { get; set; } // ω
    public float PericenterEpoch { get; set; }
    public float PericenterDistance { get; set; }
    public float PericenterVelocity { get; set; }
}
