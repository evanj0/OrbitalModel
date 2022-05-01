using OpenTK.Mathematics;

namespace OrbitalModel.Graphics;

public class Camera
{
    public Vector3 Position { get; set; } = (0, 0, 1);
    public Vector4 Position4
    {
        get => (Position.X, Position.Y, Position.Z, 0);
    }
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

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, Target, Up);
    }

    public Matrix4 GetMatrix()
    {
        var view = GetViewMatrix();
        var projection = Matrix4.CreatePerspectiveFieldOfView(FovRadians, (float)ScreenWidth / ScreenHeight, NearClip, FarClip);
        return view * projection;
    }

    public void TranslateLocal(float x, float y, float z)
    {
        var localToGlobal = GetViewMatrix().Inverted();

        var localTranslation = new Vector4(x, y, z, 0);
        var globalTranslation = (localTranslation * localToGlobal).Xyz;
        Position += globalTranslation;
        Target += globalTranslation;
    }

    public void RotateAboutZ(float x, float y, float angle)
    {
        Position -= (x, y, 0);
        Quaternion rotation = Quaternion.FromAxisAngle((1, 0, 0), angle);
        Position = (Position4 * Matrix4.CreateFromQuaternion(rotation)).Xyz;
        Position += (x, y, 0);
    }
}
