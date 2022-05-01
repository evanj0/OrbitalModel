using OpenTK.Mathematics;

namespace OrbitalModel.Graphics;

public class Camera
{
    public Vector3 Position { get; set; } = (0, 0, 1);
    public Vector3 Target { get; set; } = (0, 0, 0);
    public Vector3 Gaze
    {
        get => Target - Position;
        set => Target = Position + value;
    }
    public Vector3 Up { get; set; } = (0, 1, 0);
    public float FovRadians { get; set; } = MathHelper.DegreesToRadians(90.0f);
    public int ScreenWidth { get; set; } = 640;
    public int ScreenHeight { get; set; } = 480;
    public float NearClip { get; set; } = 0.1f;
    public float FarClip { get; set; } = 100.0f;

    public Matrix4 GetMatrix()
    {
        var view = Matrix4.LookAt(Position, Target, Up);
        var projection = Matrix4.CreatePerspectiveFieldOfView(FovRadians, (float)ScreenWidth / ScreenHeight, NearClip, FarClip);
        return projection * view;
    }
}
