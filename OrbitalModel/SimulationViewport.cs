using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OrbitalModel.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalModel;

public class SimulationViewport
{
    public SimulationViewport()
    {
        _camera = new Camera
        {
            Position = DefaultCameraPosition,
            Target = DefaultCameraTarget,
            ScreenWidth = Width,
            ScreenHeight = Height,
            NearClip = DefaultCameraNearClip,
            FarClip = DefaultCameraFarClip,
        };

        _shader = null!;
        _accelerationField = null!;
        _origin = null!;
    }

    /// <summary>
    /// Must be called before using.
    /// </summary>
    public void Init()
    {
        _shader = new ShaderBuilder()
            .AddVertexFromFile("../../../assets/vertex.glsl")
            .AddFragmentFromFile("../../../assets/fragment_color.glsl")
            .Compile();
        _accelerationField = new VectorField(
            VectorFieldXMin, VectorFieldXMax, 
            VectorFieldYMin, VectorFieldYMax, 
            VectorFieldZMin, VectorFieldZMax, VectorFieldSpacing, _shader);
        _origin = Meshes.CreateOrigin().Scale(0.5f).CreateMesh(_shader);
    }

    public void OnUpdate(FrameEventArgs args)
    {
        _lastFps = Math.Round(1.0 / args.Time, 3);
        _lastFrameTime = Math.Round(args.Time * 1000, 3);
        _realDt = DtSignificand;
        switch (DtExponent)
        {
            case -6: _realDt *= 1e-6f; break;
            case -5: _realDt *= 1e-5f; break;
            case -4: _realDt *= 1e-4f; break;
            case -3: _realDt *= 1e-3f; break;
            case -2: _realDt *= 1e-2f; break;
            case -1: _realDt *= 1e-1f; break;
            case  0: _realDt *= 1e0f;  break;
            case  1: _realDt *= 1e1f;  break;
            case  3: _realDt *= 1e2f;  break;
            case  4: _realDt *= 1e3f;  break;
            case  5: _realDt *= 1e4f;  break;
            case  6: _realDt *= 1e5f;  break;
            case  2: _realDt *= 1e6f;  break;
        }
        for (int i = 0; i < StepsPerFrame; i++)
        {
            Model.Step(G, _realDt, Bodies);
        }
        if (_showAccelerationField)
        {
            Model.UpdateForceVectorField(Bodies, _accelerationField, G);
        }
    }

    public void AddToWindow(GameWindow window)
    {
        window.UpdateFrame += OnUpdate;
        window.AddSmoothCameraOrbit(_camera, sensitivity: 0.1f, maxSpeed: 1f, minSpeed: 0.01f, deceleration: 0.75f);
        window.AddSmoothCameraZoom(_camera, sensitivity: 0.1f, maxSpeed: 1f, minSpeed: 0.01f, deceleration: 0.75f);
        window.AddCameraPan(_camera, sensitivity: 0.001f);
    }

    public void Render()
    {
        _origin.Render(_camera, Matrix4.Identity);
        foreach (var body in Bodies)
        {
            body.Render(_camera);
        }
        if (_showAccelerationField)
        {
            _accelerationField.Render(_camera);
        }
    }

    public int Width = 1400;
    public int Height = 900;
    public float SimulationFrequency = 60.0f;
    public float FovDegrees = 90.0f;
    public Vector3 DefaultCameraPosition = (1.5f, 1.5f, 1);
    public Vector3 DefaultCameraTarget = (0, 0, 0);
    public float DefaultCameraNearClip = 0.1f;
    public float DefaultCameraFarClip = 100.0f;
    private Camera _camera;
    private Shader _shader;
    public List<Body> Bodies { get; set; } = new List<Body>();
    public float G = 1.0f;
    public float DtSignificand = 1.0f;
    public int DtExponent = -4;
    private float _realDt = 0.0f;
    public int StepsPerFrame = 1;
    public float VectorFieldXMin = -1f;
    public float VectorFieldXMax = -1f;
    public float VectorFieldYMin = -1f;
    public float VectorFieldYMax = -1f;
    public float VectorFieldZMin = -1f;
    public float VectorFieldZMax = -1f;
    public float VectorFieldSpacing = 0.25f;
    private VectorField _accelerationField;
    private bool _showAccelerationField = false;
    private Mesh _origin;
    private double _lastFps = 0.0;
    private double _lastFrameTime = 0.0;

    public void AddBody(float mass, Vector3 position, Vector3 acceleration)
    {
        Bodies.Add(new Body(mass, position, acceleration, Meshes.CreateBodyMarker().CreateMesh(_shader)));
    }

    public void ResetCamera()
    {
        _camera.Position = DefaultCameraPosition;
        _camera.Target = DefaultCameraTarget;
    }

    public void LookAtOrigin()
    {
        _camera.Target = (0, 0, 0);
    }

    public void ApplyFov()
    {
        _camera.FovDegrees = FovDegrees;
    }

    public string CameraPositionString => $"({Math.Round(_camera.Position.X, 3)}, {Math.Round(_camera.Position.Y, 3)}, {Math.Round(_camera.Position.Z, 3)})";

    public string CameraTargetString => $"({Math.Round(_camera.Target.X, 3)}, {Math.Round(_camera.Target.Y, 3)}, {Math.Round(_camera.Target.Z, 3)})";

    public float DistanceToTargetDisplay => (float)Math.Round(_camera.Gaze.LengthFast, 3);

    public float RealDtDisplay => _realDt;
}
