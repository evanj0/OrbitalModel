namespace OrbitalModel;

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

    public void Render()
    {

    }
}
