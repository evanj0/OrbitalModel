using OpenTK.Mathematics;

namespace OrbitalModel;

public class OrbitalBody
{
    public OrbitalBody(double eccentricity, double semimajorAxis, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double pericenterEpoch, double period)
    {
        Eccentricity = eccentricity;
        SemimajorAxis = semimajorAxis;
        Inclination = inclination;
        LongitudeOfAscendingNode = longitudeOfAscendingNode;
        ArgumentOfPeriapsis = argumentOfPeriapsis;
        PericenterEpoch = pericenterEpoch;
        Period = period;
    }


    /// <summary>
    /// e
    /// </summary>
    public double Eccentricity { get; set; }

    /// <summary>
    /// a
    /// </summary>
    public double SemimajorAxis { get; set; }

    /// <summary>
    /// i
    /// </summary>
    public double Inclination { get; set; }

    /// <summary>
    /// Ω
    /// </summary>
    public double LongitudeOfAscendingNode { get; set; }

    /// <summary>
    /// ω
    /// </summary>
    public double ArgumentOfPeriapsis { get; set; }

    public double I => Inclination;
    public double O => LongitudeOfAscendingNode;
    public double W => ArgumentOfPeriapsis;

    /// <summary>
    /// Tp
    /// </summary>
    public double PericenterEpoch { get; set; }

    /// <summary>
    /// T
    /// </summary>
    public double Period { get; set; }

    public (Vector Position, Vector Velocity) InitialConditions(double t)
    {
        var M = MeanAnomaly(t, Period, PericenterEpoch);
        var E = EccentricAnomaly(M, Eccentricity);
        var v = TrueAnomaly(E, Eccentricity);
        var posUnit = 
            new Vector(
                (Math.Cos(O) * Math.Cos(W + v)) - (Math.Sin(O) * Math.Cos(I) * Math.Sin(W + v)), 
                (Math.Sin(O) * Math.Cos(W + v)) + (Math.Cos(O) * Math.Cos(I) * Math.Sin(W + v)),
                Math.Sin(I) * Math.Sin(W + v))
            .Normalized();
        var velUnit = 
            new Vector(
                (-1 * Math.Cos(O) * Math.Sin(W + v)) - (Math.Sin(O) * Math.Cos(I) * Math.Cos(W + v)),
                (-1 * Math.Sin(O) * Math.Sin(W + v)) + (Math.Cos(O) * Math.Cos(I) * Math.Cos(W + v)),
                Math.Sin(I) * Math.Cos(W + v))
            .Normalized();
        var radius = Radius(v, SemimajorAxis, Eccentricity);
        var speed = InstantaneousSpeed(GravitationalParameter, radius, SemimajorAxis);
        return (posUnit * radius, velUnit * speed);
    }

    public static double InstantaneousSpeed(double u, double r, double a)
    {
        return Math.Sqrt(u * ((2.0 / r) - (1.0 / a)));
    }

    public double GravitationalParameter => 4 * Math.PI * Math.PI * SemimajorAxis * SemimajorAxis * SemimajorAxis / (Period * Period);

    private static double MeanAnomaly(double t, double T, double Tp)
    {
        return 2.0 * Math.PI / T * (t - Tp);
    }

    private static double EccentricAnomaly(double meanAnomaly, double eccentricity)
    {
        Func<double, double> f = E => E + (eccentricity * Math.Sin(E)) - meanAnomaly;
        Func<double, double> fPrime = E => 1 + eccentricity * Math.Cos(E);
        var E = NewtonIterations(f, fPrime, meanAnomaly, 100);
        return E;
    }

    private static double TrueAnomaly(double eccentricAnomaly, double e)
    {
        return 2 * Math.Atan(Math.Sqrt((1.0 + e) / (1.0 - e)) * Math.Tan(eccentricAnomaly / 2.0));
    }

    private static double Radius(double trueAnomaly, double a, double e)
    {
        return a * (1.0 - (e * e)) / (1 + (e * Math.Cos(trueAnomaly)));
    }

    private static double SinExpansion(double x, int nMax)
    {
        var sum = 0.0;
        for (var n = 0; n < nMax; n++)
        {
            sum += Math.Pow(-1, n) * Math.Pow(x, (2 * n) + 1) / MathHelper.Factorial((2 * n) + 1);
        }
        return sum;
    }

    private static double NewtonIterations(Func<double, double> f, Func<double, double> fPrime, double x0, int iterations)
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
