using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OrbitalModel.Graphics;

public class Camera
{
    public Vector3 Position { get; set; } = (1, 1, 1);
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
    public Vector3 Up { get; set; } = (0, 0, 1);
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

    public void TranslateLocal(float x, float y, float z, bool translateTarget = false)
    {
        var localToGlobal = GetViewMatrix().Inverted();

        var localTranslation = new Vector4(x, y, z, 0);
        var globalTranslation = (localTranslation * localToGlobal).Xyz;
        Position += globalTranslation;
        if (translateTarget)
        {
            Target += globalTranslation;
        }
    }

    public void RotateAboutZ(float x, float y, float angle)
    {
        Position -= (x, y, 0);
        Quaternion rotation = Quaternion.FromAxisAngle(Vector3.UnitZ, angle);
        Position = (Position4 * Matrix4.CreateFromQuaternion(rotation)).Xyz;
        Position += (x, y, 0);
    }

    public void RotateAboutPointAndAxis(Vector3 point, Vector3 axis, float angle)
    {
        Position -= point;
        Quaternion rotation = Quaternion.FromAxisAngle(axis, angle);
        Position = (Position4 * Matrix4.CreateFromQuaternion(rotation)).Xyz;
        Position += point;
    }

    public void Orbit(float azimuth, float inclination)
    {
        RotateAboutZ(Target.X, Target.Y, azimuth);
        RotateAboutPointAndAxis(Target, Vector3.Cross(Position - Target, Up), inclination);
    }

    public void WindowResized(int width, int height)
    {
        ScreenWidth = width;
        ScreenHeight = height;
    }
}

public static class GameWindow_Extensions
{
    public static void AddSmoothCameraZoom(this GameWindow window, Camera camera, float sensitivity, float maxSpeed, float minSpeed, float deceleration)
    {
        var velocity = 0f;

        window.MouseWheel += args =>
        {
            velocity += -1f * sensitivity * args.OffsetY * (camera.Position - camera.Target).LengthFast;
        };

        window.UpdateFrame += args =>
        {
            camera.TranslateLocal(0, 0, velocity);

            if (Math.Abs(velocity) < minSpeed)
            {
                velocity = 0f;
            }
            else if (Math.Abs(velocity) > maxSpeed)
            {
                velocity /= Math.Abs(velocity);
                velocity *= maxSpeed;
            }

            velocity *= deceleration;
        };
    }

    public static void AddSmoothCameraOrbit(this GameWindow window, Camera camera, float sensitivity, float maxSpeed, float minSpeed, float deceleration)
    {
        var velocity = Vector2.Zero;

        window.MouseMove += args =>
        {
            if (window.IsMouseButtonDown(MouseButton.Left) && !ImGuiNET.ImGui.GetIO().WantCaptureMouse)
            {
                velocity.X += -1f * sensitivity * MathHelper.DegreesToRadians(args.DeltaX);
                velocity.Y += 1f * sensitivity * MathHelper.DegreesToRadians(args.DeltaY);
            }
        };

        window.UpdateFrame += args =>
        {
            camera.Orbit(velocity.X, velocity.Y);

            if (velocity.LengthFast < minSpeed)
            {
                velocity = Vector2.Zero;
            }
            else if (velocity.LengthFast > maxSpeed)
            {
                velocity.NormalizeFast();
                velocity *= maxSpeed;
            }

            velocity *= deceleration;
        };
    }

    public static void AddCameraPan(this GameWindow window, Camera camera, float sensitivity)
    {
        window.MouseMove += args =>
        {
            if (window.IsMouseButtonDown(MouseButton.Right) && !ImGuiNET.ImGui.GetIO().WantCaptureMouse)
            {
                camera.TranslateLocal(-1f * args.DeltaX * sensitivity, args.DeltaY * sensitivity, 0, true);
            }
        };
    }
}