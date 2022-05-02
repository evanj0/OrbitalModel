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
        var width = 1200;
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
            Position = (1, 1, 1),
            Target = (0, 0, 0),
            ScreenWidth = width,
            ScreenHeight = height,
            NearClip = 0.1f,
            FarClip = 100.0f,
        };

        int program = 0;

        Shader colorShader = null!;
        Shader textureShader = null!;

        Vao vao1 = null!;

        var bodies = new List<Body>()
        {
            new Body(1, (0, 0, 0), (0, 0, 0)),
            new Body(0.0001, (0, 1, 0), (1, 0, 0)),
            new Body(0.0005, (0, -1, 0), (0.5, 0, 0)),
            new Body(0.0005, (0, -0.8, 0), (-0.5, 0, 0)),
            new Body(0.0005, (0, -0.8, 0.5), (-0.5, 0.5, 0)),
        };
        var g = 1.0;
        var dt = 1.0f;
        var dtMultiplier = 0.0001f;
        var simsPerFrame = 1;

        var originXY = new Mesh(
            new float[] 
            {
                0, 0, 0,    0, 0, 0,
                0, -1, 0,   0, 0, 0,
                1, 0, 0,    0, 0, 0,
            },
            new int[]
            {
                0, 1, 2,
            },
            colorShader
            );

        var originScale = Matrix4.CreateScale(0.25f);

        window.Load += () =>
        {
            gui = new ImGuiController(width, height);

            GL.Enable(EnableCap.DepthTest);

            program = CreateShader();
            // GL.UseProgram(program);

            colorShader = new ShaderBuilder()
                .AddVertexFromFile("../../../assets/vertex_shader.glsl")
                .AddFragmentFromFile("../../../assets/fragment_shader.glsl")
                .Compile();

            GL.BindVertexArray(vao1.Id);
            // vao1.Bind();
        };

        double lastFps = 0;
        double lastFrameTime = 0;

        window.RenderFrame += args =>
        {
            GL.Enable(EnableCap.DepthTest);
            gui.Update(window, (float)args.Time);

            GL.ClearColor(0.07f, 0.13f, 0.17f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            originXY.Render(camera, originScale);

            foreach (var body in bodies)
            {
                body.Render(program, camera);
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
                }
                if (ImGui.Button("Look At Origin"))
                {
                    camera.Target = (0, 0, 0);
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
                ImGui.SliderInt("Steps Per Frame", ref simsPerFrame, 1, 1000);
                ImGui.LabelText("Time Step (dt)", $"{dt * dtMultiplier} s");
                ImGui.LabelText("Simulation Frequency (frames/s)", $"{simulationFrequency}");
                ImGui.LabelText("Time Scale", $"{Math.Round(dt * simulationFrequency * simsPerFrame, 3)} s = 1 s");

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

    public static int CreateShader()
    {
        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

        GL.ShaderSource(vertexShader, File.ReadAllText("../../../assets/vertex_shader.glsl"));
        GL.ShaderSource(fragmentShader, File.ReadAllText("../../../assets/fragment_shader.glsl"));

        GL.CompileShader(vertexShader);
        GL.CompileShader(fragmentShader); 

        Console.WriteLine(GL.GetShaderInfoLog(vertexShader));
        Console.WriteLine(GL.GetShaderInfoLog(fragmentShader));

        var shaderProgram = GL.CreateProgram();

        GL.AttachShader(shaderProgram, vertexShader);
        GL.AttachShader(shaderProgram, fragmentShader);

        GL.LinkProgram(shaderProgram);

        GL.DetachShader(shaderProgram, vertexShader);
        GL.DetachShader(shaderProgram, fragmentShader);

        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader);

        Console.WriteLine(GL.GetProgramInfoLog(shaderProgram));

        return shaderProgram;
    }

    public static int CreateVao(out int indexBuffer)
    {
        var vertexBuffer = GL.GenBuffer();
        var colorBuffer = GL.GenBuffer();
        indexBuffer = GL.GenBuffer();
        var vao = GL.GenVertexArray();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, 9 * sizeof(float), new float[] { -0.5f, -0.5f, 0.0f, 0.5f, -0.5f, 0.0f, 0.0f, 0.5f, 0.0f }, BufferUsageHint.StaticCopy);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, colorBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, 9 * sizeof(float), new float[] { 1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f }, BufferUsageHint.StaticCopy);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(1);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
        GL.BufferData(BufferTarget.ElementArrayBuffer, 3 * sizeof(uint), new uint[] { 0, 1, 2 }, BufferUsageHint.StaticCopy);

        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        GL.DeleteBuffer(vertexBuffer);
        GL.DeleteBuffer(colorBuffer);

        return vao;
    }
}
