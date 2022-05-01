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

        var gws = GameWindowSettings.Default;
        var nws = NativeWindowSettings.Default;
        gws.RenderFrequency = 60.0;
        gws.UpdateFrequency = 60.0;

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

        Vao vao1 = null!;

        var bodies = new List<Body>()
        {
            new Body(1, (0, 0, 0), (0, 0, 0)),
            new Body(0.01, (0, 1, 0), (1, 0, 0)),
        };

        window.Load += () =>
        {
            gui = new ImGuiController(width, height);

            var vertices = new float[]
            {
                -0.5f, -0.5f * (float)Math.Sqrt(3) / 3, 0.3f,       0.8f, 0.3f, 0.02f,
                0.5f, -0.5f * (float)Math.Sqrt(3) / 3, 0.0f,        0.8f, 0.3f, 0.02f,
                0.0f, 0.5f * (float)Math.Sqrt(3) * 2 / 3, 0.0f,     1.0f, 0.6f, 0.32f,

                -0.5f / 2, 0.5f * (float)Math.Sqrt(3) / 6, 0.0f,    0.9f, 0.45f, 0.17f,
                0.5f / 2, 0.5f * (float)Math.Sqrt(3) / 6, 0.0f,     0.9f, 0.45f, 0.17f,
                0.0f, -0.5f * (float)Math.Sqrt(3) / 3, 0.0f,        0.8f, 0.3f, 0.02f,
            };

            var indices = new int[]
            {
                0, 3, 5,
                3, 2, 4,
                5, 4, 1,
            };

            vao1 = new Vao();
            vao1.Bind();
            var vbo1 = new Vbo(vertices);
            var ebo1 = new Ebo(indices);

            vao1.LinkAttrib(vbo1, 0, 3, VertexAttribPointerType.Float, 6 * sizeof(float), 0);
            vao1.LinkAttrib(vbo1, 1, 3, VertexAttribPointerType.Float, 6 * sizeof(float), 3 * sizeof(float));
            vao1.Unbind();
            vbo1.Unbind();
            ebo1.Unbind();

            GL.Enable(EnableCap.DepthTest);

            program = CreateShader();
            // GL.UseProgram(program);

            GL.BindVertexArray(vao1.Id);
            // vao1.Bind();
        };

        double lastFps = 0;
        double lastFrameTime = 0;

        window.RenderFrame += args =>
        {
            gui.Update(window, (float)args.Time);

            GL.ClearColor(0.07f, 0.13f, 0.17f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.UseProgram(program);
            var model = Matrix4.Identity;
            // matrices are transposed in OpenTK!
            var matrix = camera.GetMatrix() * model;
            var cameraUniform = GL.GetUniformLocation(program, "camera");
            GL.UniformMatrix4(cameraUniform, false, ref matrix);

            vao1.Bind();
            GL.DrawElements(PrimitiveType.Triangles, 9, DrawElementsType.UnsignedInt, 0);

            // ImGui rendering

            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 600));
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(25, 25));
            ImGui.SetWindowCollapsed(false);
            if (ImGui.Begin("Options"))
            {
                ImGui.Text("Camera");
                ImGui.Separator();
                if (ImGui.Button("Reset Camera"))
                {
                    camera.Position = (1, 1, 1);
                    camera.Target = (0, 0, 0);
                }
                ImGui.LabelText($"({Math.Round(camera.Position.X, 3)}, {Math.Round(camera.Position.Y, 3)}, {Math.Round(camera.Position.Z, 3)})", "Position");
                ImGui.LabelText($"({Math.Round(camera.Target.X, 3)}, {Math.Round(camera.Target.Y, 3)}, {Math.Round(camera.Target.Z, 3)})", "Target");
                ImGui.LabelText($"{Math.Round(camera.Gaze.LengthFast, 3)}", "Distance to Target");

                ImGui.End();
            }

            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 100));
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(400 + 25 + 25, 25));
            ImGui.SetWindowCollapsed(false);
            if (ImGui.Begin("Debug"))
            {
                ImGui.LabelText($"{lastFps}", "FPS");
                ImGui.LabelText($"{lastFrameTime} ms", "Frame Time");

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

            Model.Step(1, 0.01, bodies);
        };
        window.AddSmoothCameraOrbit(camera, sensitivity: 0.1f, maxSpeed: 1f, minSpeed: 0.01f, deceleration: 0.75f);
        window.AddSmoothCameraZoom(camera, sensitivity: 0.1f, maxSpeed: 1f, minSpeed: 0.01f, deceleration: 0.75f);
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
