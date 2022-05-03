using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;

using ImGuiNET;

using OrbitalModel.Graphics;
using OpenTK.Mathematics;

namespace OrbitalModel;

public class Program
{
    public static void Main(string[] args)
    {
        var width = 1400;
        var height = 900;
        var simulationFrequency = 60.0;
        float fov = 90;

        var gws = GameWindowSettings.Default;
        var nws = NativeWindowSettings.Default;
        gws.RenderFrequency = 60.0;
        gws.UpdateFrequency = simulationFrequency;

        nws.IsEventDriven = false;
        nws.API = ContextAPI.OpenGL;
        nws.APIVersion = Version.Parse("4.1");
        nws.AutoLoadBindings = true;
        nws.WindowState = WindowState.Normal;
        nws.Size = new(width, height);
        nws.Title = "Orbital Model";
        
        var window = new GameWindow(gws, nws);

        ImGuiController gui = null!;

        var camera = new Camera
        {
            Position = (1.5f, 1.5f, 1),
            Target = (0, 0, 0),
            ScreenWidth = width,
            ScreenHeight = height,
            NearClip = 0.1f,
            FarClip = 100.0f,
        };

        Shader colorShader = null!;
        Shader textureShader = null!;


        List<Body> bodies = null!;
        var g = 1.0f;
        var dt = 1.0f;
        var dtMultiplier = 0.0001f;
        var simsPerFrame = 1;

        VectorField accelField = null!;
        bool showAccelField = false;

        Mesh origin = null!;

        var originScale = Matrix4.CreateScale(0.25f);

        window.Load += () =>
        {
            gui = new ImGuiController(width, height);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            colorShader = new ShaderBuilder()
                .AddVertexFromFile("../../../assets/vertex.glsl")
                .AddFragmentFromFile("../../../assets/fragment_color.glsl")
                .Compile();

            origin = Meshes.CreateOrigin()
                .Scale(0.75f)
                .CreateMesh(colorShader);

            bodies = new List<Body>()
            {
                new Body(1, (0, 0, -0.5f), (-0.5f, 0, 0), Meshes.CreateBodyMarker().CreateMesh(colorShader)),
                new Body(1, (0, 0, 0.5f), (0.5f, 0, 0), Meshes.CreateBodyMarker().CreateMesh(colorShader)),
                new Body(0.0001f, (0, 1, 0), (1, 0, 0), Meshes.CreateBodyMarker().CreateMesh(colorShader)),
                new Body(0.0005f, (0, -1, 0), (0.5f, 0, 0), Meshes.CreateBodyMarker().CreateMesh(colorShader)),
                new Body(0.0005f, (0, -0.8f, 0), (-0.5f, 0, 0), Meshes.CreateBodyMarker().CreateMesh(colorShader)),
                new Body(0.0005f, (0, -0.8f, 0.5f), (-0.5f, 0.5f, 0), Meshes.CreateBodyMarker().CreateMesh(colorShader)),
            };

            accelField = new VectorField(-1, 1, -1, 1, -1, 1, 0.25f, colorShader);
        };

        double lastFps = 0;
        double lastFrameTime = 0;
        int trackedBody = 0;
        bool tracking = false;

        window.RenderFrame += args =>
        {            
            GL.Enable(EnableCap.DepthTest);
            gui.Update(window, (float)args.Time);

            GL.ClearColor(0.07f, 0.13f, 0.17f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            if (tracking)
            {
                var pos = bodies[trackedBody].Position;
                camera.Target = ((float)pos.X, (float)pos.Y, (float)pos.Z);
            }

            origin.Render(camera, originScale);

            foreach (var body in bodies)
            {
                body.Render(camera);
            }

            if (showAccelField)
            {
                accelField.Render(camera);
            }

            // ImGui rendering

            ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 600));
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, 0));
            ImGui.SetWindowCollapsed(false);
            if (ImGui.Begin("Options"))
            {
                // Camera
                ImGui.Text("Camera");
                ImGui.Separator();
                if (ImGui.Button("Reset Camera"))
                {
                    camera.Position = (1, 1, 1);
                    camera.Target = (0, 0, 0);
                    tracking = false;
                }
                if (ImGui.Button("Look At Origin"))
                {
                    camera.Target = (0, 0, 0);
                    tracking = false;
                }
                ImGui.SliderFloat("Field of View (degrees)", ref fov, 10.0f, 179.0f);
                camera.FovDegrees = fov;
                
                ImGui.LabelText("Position", $"({Math.Round(camera.Position.X, 3)}, {Math.Round(camera.Position.Y, 3)}, {Math.Round(camera.Position.Z, 3)})");
                ImGui.LabelText("Target", $"({Math.Round(camera.Target.X, 3)}, {Math.Round(camera.Target.Y, 3)}, {Math.Round(camera.Target.Z, 3)})");
                ImGui.LabelText("Distance to Target", $"{Math.Round(camera.Gaze.LengthFast, 3)}");

                // Simulation
                ImGui.Text("Simulation");
                ImGui.Separator();
                ImGui.SliderFloat("Time Step (dt)", ref dt, 1f, 10f);
                ImGui.SliderFloat("Time Step Multiplier", ref dtMultiplier, 0.0001f, 1.0f);
                ImGui.SliderInt("Steps Per Frame", ref simsPerFrame, 1, 10000);
                ImGui.LabelText("Time Step (dt)", $"{dt * dtMultiplier} s");
                ImGui.LabelText("Simulation Frequency (frames/s)", $"{simulationFrequency}");
                ImGui.LabelText("Time Scale", $"{Math.Round(dt * simulationFrequency * simsPerFrame, 3)} s = 1 s");

                // Data
                ImGui.Text("Data");
                ImGui.Separator();
                ImGui.Checkbox("Show acceleration vector field", ref showAccelField);

                // Objects
                ImGui.Text("Objects");
                ImGui.Separator();
                for (var i = 0; i < bodies.Count; i++)
                {
                    ImGui.Text($"{bodies[i].Mass}[{i}]:");
                    if (ImGui.Button("Look At"))
                    {
                        Console.WriteLine(i);
                        trackedBody = i;
                        tracking = true;
                    }
                }

                ImGui.End();
            }

            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 100));
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(500, 0));
            ImGui.SetWindowCollapsed(false);
            if (ImGui.Begin("Debug"))
            {
                ImGui.LabelText("frames/s", $"{lastFps}");
                ImGui.LabelText("Frame Time", $"{lastFrameTime} ms");

                ImGui.End();
            }

            gui.Render();

            // End rendering

            window.SwapBuffers();
        };
        window.UpdateFrame += args =>
        {
            lastFps = Math.Round(1.0 / args.Time, 3);
            lastFrameTime = Math.Round(args.Time * 1000, 3);
            for (int i = 0; i < simsPerFrame; i++)
            {
                Model.Step(g, dt * dtMultiplier, bodies);
            }
            if (showAccelField)
            {
                Model.UpdateForceVectorField(bodies, accelField, g);
            }
        };
        window.AddSmoothCameraOrbit(camera, sensitivity: 0.1f, maxSpeed: 1f, minSpeed: 0.01f, deceleration: 0.75f);
        window.AddSmoothCameraZoom(camera, sensitivity: 0.1f, maxSpeed: 1f, minSpeed: 0.01f, deceleration: 0.75f);
        window.AddCameraPan(camera, sensitivity: 0.001f);
        window.Resize += args =>
        {
            GL.Viewport(0, 0, args.Width, args.Height);
            gui.WindowResized(args.Width, args.Height);
            camera.WindowResized(args.Width, args.Height);
        };
        window.Closing += args =>
        {
        };
        window.Run();
    }
}
