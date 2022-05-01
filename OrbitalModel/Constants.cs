using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalModel;

public static class Constants
{
    public static readonly double EarthMass = 5.972e24; //5.972 × 10^24 kg
    public static readonly double EarthRadius = 6.3781e6; //6,378.1370 km
    public static readonly double EarthOrbitalRadius = 150.36e9; //150.36 billion m
    public static readonly double EarthOrbitalVelocity = 29780; //29.78 km/s
    public static readonly double SunMass = 1.98847e30; //1.98847×10^30 kg
    public static readonly int OneYear = 31_536_000;
    public static readonly double G = 6.6743e-11;
}