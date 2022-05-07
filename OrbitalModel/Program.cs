using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using ImGuiNET;
using System.Numerics;

using OrbitalModel.Graphics;

namespace OrbitalModel;

public class Program
{
    public static void Main(string[] args)
    {
        var width = 1200;
        var height = 800;
        var updateFrequency = 60.0f;

        var gws = GameWindowSettings.Default;
        var nws = NativeWindowSettings.Default;
        gws.RenderFrequency = 60.0;
        gws.UpdateFrequency = updateFrequency;

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
            Width = width,
            Height = height,
            SimulationFrequency = updateFrequency,
            G = 1,
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

            GL.ClearColor(0.07f, 0.13f, 0.17f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            // Render viewport

            vp.Render();

            // Render gui

            gui.Update(window, (float)args.Time);
            // ImGui.DockSpaceOverViewport();
            // if (ImGui.Begin("viewport"))
            // {
            // 
            // }

            if (ImGui.Begin("options"))
            {
                // Camera
                ImGui.Text("camera");
                ImGui.Separator();
                if (ImGui.Button("reset camera"))
                {
                    vp.ResetCamera();
                }
                if (ImGui.Button("look at origin"))
                {
                    vp.LookAtOrigin();
                }
                ImGui.SliderFloat("field of view (degrees)", ref vp.FovDegrees, 10.0f, 179.0f);
                vp.ApplyFov();
                
                ImGui.LabelText("position", vp.CameraPositionString);
                ImGui.LabelText("target", vp.CameraTargetString);
                ImGui.LabelText("distance to target", $"{vp.DistanceToTargetDisplay, 3}");

                // Simulation
                ImGui.Text("simulation");
                ImGui.Separator();

                if (vp.Paused)
                {
                    if (ImGui.Button("play"))
                    {
                        vp.Paused = false;
                    }
                }
                else
                {
                    if (ImGui.Button("pause"))
                    {
                        vp.Paused = true;
                    }
                }
                ImGui.SameLine();
                ImGui.Text(vp.Paused ? "Paused" : "Running");

                ImGui.SliderFloat("time step significand", ref vp.DtSignificand, 1f, 10f);
                ImGui.SliderInt("time step exponent", ref vp.DtExponent, -6, 6);
                ImGui.SliderInt("steps per frame", ref vp.StepsPerFrame, 1, 10000);
                ImGui.LabelText("time step", $"{vp.RealDtDisplay} s");
                ImGui.LabelText("simulation framerate", $"{vp.SimulationFrequency} fps");
                ImGui.LabelText("time scale", $"{vp.TimeScaleDisplay} s (sim) = 1 s (real)");

                // Data
                ImGui.Text("data");
                ImGui.Separator();
                ImGui.Checkbox("show acceleration vector field", ref vp.ShowAccelerationField);
                ImGui.SameLine();
                var regenerate = ImGui.Button("regenerate");

                ImGui.PushItemWidth(100f);
                ImGui.DragFloat("x min", ref vp.VectorFieldXMin, v_speed: 0.5f);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100f);
                ImGui.DragFloat("y min", ref vp.VectorFieldYMin, v_speed: 0.5f);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100f);
                ImGui.DragFloat("z min", ref vp.VectorFieldZMin, v_speed: 0.5f);
                ImGui.PopItemWidth();

                ImGui.PushItemWidth(100f);
                ImGui.DragFloat("x max", ref vp.VectorFieldXMax, v_speed: 0.5f);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100f);
                ImGui.DragFloat("y max", ref vp.VectorFieldYMax, v_speed: 0.5f);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100f);
                ImGui.DragFloat("z max", ref vp.VectorFieldZMax, v_speed: 0.5f);
                ImGui.PopItemWidth();

                ImGui.DragFloat("spacing", ref vp.VectorFieldSpacing, 0.1f, 10f);

                if (regenerate)
                {
                    vp.RegenerateAccelerationField();
                }
            }

            if (ImGui.Begin("debug"))
            {
                ImGui.LabelText("render framerate", $"{vp.FramerateDisplay} fps");
                ImGui.LabelText("frame time", $"{vp.FrameTimeDisplay} ms");

                ImGui.End();
            }

            gui.Render();

            // End rendering

            window.SwapBuffers();
        };
        window.Resize += args =>
        {
            GL.Viewport(0, 0, args.Width, args.Height);
            gui.WindowResized(args.Width, args.Height);
            vp.Resize(args.Width, args.Height);
        };
        window.TextInput += args =>
        {
            gui.PressChar((char)args.Unicode);
        };
        window.Run();
    }
}

public class Vector3Field
{
    public Vector3Field(float x, float y, float z)
    {
        _value = new Vector3(x, y, z);
    }

    public ref Vector3 Ref => ref _value;
    private Vector3 _value;

    public void BindX(ref float x) => x = _value.X;
    public void BindY(ref float y) => y = _value.Y;
    public void BindZ(ref float z) => z = _value.Z;
}