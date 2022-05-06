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

        var vp = new SimulationViewport()
        {

        };

        window.Load += () =>
        {
            gui = new ImGuiController(width, height);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            vp.Init();
            vp.AddBody(1, (0, 0, -0.5f), (-0.5f, 0, 0));
            vp.AddBody(1, (0, 0, 0.5f), (0.5f, 0, 0));
            vp.AddBody(0.0001f, (0, 1, 0), (1, 0, 0));
            vp.AddBody(0.0005f, (0, -1, 0), (0.5f, 0, 0));
            vp.AddBody(0.0005f, (0, -0.8f, 0), (-0.5f, 0, 0));
            vp.AddBody(0.0005f, (0, -0.8f, 0.5f), (-0.5f, 0.5f, 0));
        };
        vp.AddToWindow(window);

        window.RenderFrame += args =>
        {            
            GL.Enable(EnableCap.DepthTest);
            gui.Update(window, (float)args.Time);

            GL.ClearColor(0.07f, 0.13f, 0.17f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            vp.Render();

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
                    vp.ResetCamera();
                }
                if (ImGui.Button("Look At Origin"))
                {
                    vp.LookAtOrigin();
                }
                ImGui.SliderFloat("Field of View (degrees)", ref vp.FovDegrees, 10.0f, 179.0f);
                vp.ApplyFov();
                
                ImGui.LabelText("Position", vp.CameraPositionString);
                ImGui.LabelText("Target", vp.CameraTargetString);
                ImGui.LabelText("Distance to Target", $"{Math.Round(vp.DistanceToTargetDisplay, 3)}");

                // Simulation
                ImGui.Text("Simulation");
                ImGui.Separator();
                ImGui.SliderFloat("Time Step Significand", ref vp.DtSignificand, 1f, 10f);
                ImGui.SliderInt("Time Step Exponent", ref vp.DtExponent, -6, 6);
                ImGui.SliderInt("Steps Per Frame", ref vp.StepsPerFrame, 1, 10000);
                ImGui.LabelText("Time Step (dt)", $"{Math.Round(vp.RealDt, 4)} s");
                ImGui.LabelText("Simulation Frequency (frames/s)", $"{simulationFrequency}");
                ImGui.LabelText("Time Scale", $"{Math.Round(realDt * simulationFrequency * simsPerFrame, 3)} s = 1 s");

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
            realDt = dtSignificand;
            switch (dtExponent)
            {
                case -6: realDt *= 1e-6f; break;
                case -5: realDt *= 1e-5f; break;
                case -4: realDt *= 1e-4f; break;
                case -3: realDt *= 1e-3f; break;
                case -2: realDt *= 1e-2f; break;
                case -1: realDt *= 1e-1f; break;
                case  0: realDt *= 1e0f; break;
                case  1: realDt *= 1e1f; break;
                case  3: realDt *= 1e2f; break;
                case  4: realDt *= 1e3f; break;
                case  5: realDt *= 1e4f; break;
                case  6: realDt *= 1e5f; break;
                case  2: realDt *= 1e6f; break;
            }
            for (int i = 0; i < simsPerFrame; i++)
            {
                Model.Step(g, realDt, bodies);
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
