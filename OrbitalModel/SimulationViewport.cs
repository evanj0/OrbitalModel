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
        _centerOfMass = null!;
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
    public float VectorFieldXMax =  1f;
    public float VectorFieldYMin = -1f;
    public float VectorFieldYMax =  1f;
    public float VectorFieldZMin = -1f;
    public float VectorFieldZMax =  1f;
    public float VectorFieldSpacing = 0.25f;
    private VectorField _accelerationField;
    public bool ShowAccelerationField = false;
    private Mesh _origin;
    private double _lastUpdateFps = 0.0;
    private double _lastUpdateTime = 0.0;
    private double _lastRenderFps = 0.0;
    private double _lastRenderTime = 0.0;
    public bool ShowCenterOfMass = false;
    private Mesh _centerOfMass;
    private Matrix4 _centerOfMassTransform = Matrix4.Identity;
    public int PhysicalScaleOrder = 0;

    public bool Paused = false;

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
        _origin = Meshes.CreateOrigin()
            .Scale(0.25f)
            .CreateMesh(_shader);
        _centerOfMass = new MeshBuilder().CreateCom().CreateMesh(_shader);
    }

    public void OnUpdate(FrameEventArgs args)
    {
        _lastUpdateFps = 1.0 / args.Time;
        _lastUpdateTime = args.Time * 1000;
        _realDt = DtSignificand;
        if (Paused)
        {
            _realDt = 0;
        }
        switch (DtExponent)
        {
            case -6: _realDt *= 1e-6f; break;
            case -5: _realDt *= 1e-5f; break;
            case -4: _realDt *= 1e-4f; break;
            case -3: _realDt *= 1e-3f; break;
            case -2: _realDt *= 1e-2f; break;
            case -1: _realDt *= 1e-1f; break;
            case  0: _realDt *= 1e+0f; break;
            case  1: _realDt *= 1e+1f; break;
            case  2: _realDt *= 1e+2f; break;
            case  3: _realDt *= 1e+3f; break;
            case  4: _realDt *= 1e+4f; break;
            case  5: _realDt *= 1e+5f; break;
            case  6: _realDt *= 1e+6f; break;
        }
        for (int i = 0; i < StepsPerFrame; i++)
        {
            Model.Step(G, _realDt, Bodies);
        }
        if (ShowAccelerationField)
        {
            Model.UpdateForceVectorField(Bodies, _accelerationField, G);
        }
        if (ShowCenterOfMass)
        {
            _centerOfMassTransform = Matrix4.CreateTranslation(Bodies.CenterOfMass());
        }
    }

    public void AddToWindow(GameWindow window)
    {
        window.UpdateFrame += OnUpdate;
        window.RenderFrame += args =>
        {
            _lastRenderFps = 1.0 / args.Time;
            _lastRenderTime = args.Time * 1000;
        };
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
        if (ShowAccelerationField)
        {
            _accelerationField.Render(_camera);
        }
        if (ShowCenterOfMass)
        {
            _centerOfMass.Render(_camera, _centerOfMassTransform);
        }
    }

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

    public void Resize(int width, int height)
    {
        _camera.Resize(width, height);
    }

    public void RegenerateAccelerationField()
    {
        _accelerationField = new VectorField(
            VectorFieldXMin, VectorFieldXMax, 
            VectorFieldYMin, VectorFieldYMax, 
            VectorFieldZMin, VectorFieldZMax, VectorFieldSpacing, _shader);
    }

    private static float Round(float value) => (float)Math.Round(value, 3);
    private static float Round(double value) => (float)Math.Round(value, 3);

    public string CameraPositionString => $"({Round(_camera.Position.X)}, {Round(_camera.Position.Y)}, {Round(_camera.Position.Z)})";

    public string CameraTargetString => $"({Round(_camera.Target.X)}, {Round(_camera.Target.Y)}, {Round(_camera.Target.Z)})";

    public float DistanceToTargetDisplay => Round(_camera.Gaze.LengthFast);

    public float RealDtDisplay => Round(_realDt);

    public float TimeScaleDisplay => Round(_realDt * SimulationFrequency * StepsPerFrame);

    public float UpdateFramerateDisplay => Round(_lastUpdateFps);

    public float UpdateFrameTimeDisplay => Round(_lastUpdateTime);

    public float RenderFramerateDisplay => Round(_lastRenderFps);

    public float RenderFrameTimeDisplay => Round(_lastRenderTime);

    public int AccelerationFieldNumVectors
    {
        get
        {
            var x = (int)((float)(VectorFieldXMax - VectorFieldXMin) / VectorFieldSpacing);
            var y = (int)((float)(VectorFieldYMax - VectorFieldYMin) / VectorFieldSpacing);
            var z = (int)((float)(VectorFieldZMax - VectorFieldZMin) / VectorFieldSpacing);
            return x * y * z;
        }
    }
}
