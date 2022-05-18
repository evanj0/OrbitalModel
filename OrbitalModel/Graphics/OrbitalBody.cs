using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalModel.Graphics;

public class OrbitalBody
{
    public double Inclination { get; set; } // i
    public double LongitudeOfAscendingNode { get; set; } // Ω
    public double ArgumentOfPeriapsis { get; set; } // ω
    public double PericenterEpoch { get; set; }
    public double PericenterDistance { get; set; }
    public double PericenterVelocity { get; set; }
    public double Period { get; set; }

    public double MeanAnomaly(double t)
    {
        return 2.0 * Math.PI / Period * (t - PericenterEpoch);
    }

    public double EccentricAnomaly(double meanAnomaly, double eccentricity)
    {
        Func<double, double> f = E => E + (eccentricity * Math.Sin(E)) - meanAnomaly;
        Func<double, double> fPrime = E => 1 + eccentricity * Math.Cos(E);
        var E = NewtonIterations(f, fPrime, meanAnomaly, 100);
        return E;
    }

    public double TrueAnomaly(double eccentricAnomaly, double e)
    {
        return 2 * Math.Atan(Math.Sqrt((1.0 + e) / (1.0 - e)) * Math.Tan(eccentricAnomaly / 2.0));
    }

    public double Radius(double trueAnomaly)
    {
        return 1;
    }

    public double SinExpansion(double x, int nMax)
    {
        var sum = 0.0;
        for (var n = 0; n < nMax; n++)
        {
            sum += Math.Pow(-1, n) * Math.Pow(x, (2 * n) + 1) / MathHelper.Factorial((2 * n) + 1);
        }
        return sum;
    }

    public double NewtonIterations(Func<double, double> f, Func<double, double> fPrime, double x0, int iterations)
    {
        var xn = x0;
        for (var i = 0; i < iterations; i++)
        {
            var xIntercept = f(xn) / fPrime(xn) + xn;
            xn = xIntercept;
        }
        return xn;
    }

}
