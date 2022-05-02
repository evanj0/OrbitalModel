using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalModel.Graphics;

public class Mesh
{
    public float[] Vertices { get; set; }
    public int[] Indices { get; set; }
    public Shader Shader { get; set; }

    private Vao _vao;

    public Mesh(float[] vertices, int[] indices, Shader shader)
    {
        Vertices = vertices;
        Indices = indices;
        Shader = shader;

        _vao = new Vao();
        _vao.Bind();
        var vbo1 = new Vbo(vertices);
        var ebo1 = new Ebo(indices);

        // [x, y, z, r, g, b, a, u, v]
        _vao.LinkAttrib(vbo1, 0, 3, VertexAttribPointerType.Float, 9 * sizeof(float), 0);                   // position
        _vao.LinkAttrib(vbo1, 1, 4, VertexAttribPointerType.Float, 9 * sizeof(float), 3 * sizeof(float));   // color (r, g, b, a)
        _vao.LinkAttrib(vbo1, 2, 2, VertexAttribPointerType.Float, 9 * sizeof(float), 7 * sizeof(float));   // texture coords
        _vao.Unbind();
        vbo1.Unbind();
        ebo1.Unbind();

        _vao.Unbind();
    }

    public void Render(Camera camera, Matrix4 transform)
    {
        Shader.Activate();
        var matrix = transform * camera.GetMatrix();
        var cameraUniform = Shader.GetUniformLocation("camera");
        GL.UniformMatrix4(cameraUniform, false, ref matrix);
        _vao.Bind();
        GL.DrawElements(PrimitiveType.Triangles, Vertices.Length, DrawElementsType.UnsignedInt, 0);
        _vao.Unbind();
    }
}

public class Vertex
{
    public Vector3 Position { get; set; }
    public Color4 Color { get; set; }
    public Vector2 TextureCoordinates { get; set; }

    public Vertex(Vector3 position, Color4 color, Vector2 textureCoordinates)
    {
        Position = position;
        Color = color;
        TextureCoordinates = textureCoordinates;
    }

    public void WriteDataTo(float[] array, int index)
    {
        array[index + 0] = Position.X;
        array[index + 1] = Position.Y;
        array[index + 2] = Position.Z;
        array[index + 3] = Color.R;
        array[index + 4] = Color.G;
        array[index + 5] = Color.B;
        array[index + 6] = Color.A;
        array[index + 7] = TextureCoordinates.X;
        array[index + 8] = TextureCoordinates.Y;
    }
}

public class Face
{ 
    public Vertex Vertex0 { get; set; }
    public Vertex Vertex1 { get; set; }
    public Vertex Vertex2 { get; set; }

    public Face(Vertex vertex0, Vertex vertex1, Vertex vertex2)
    {
        Vertex0 = vertex0;
        Vertex1 = vertex1;
        Vertex2 = vertex2;
    }
}

public class MeshBuilder
{
    private List<Face> _faces;
    private List<Vertex> _vertices;
    private Color4 _color;

    public MeshBuilder()
    {
        _faces = new();
        _vertices = new();
    }

    public MeshBuilder SetVertexColor(Color4 color)
    {
        _color = color;
        return this;
    }

    public MeshBuilder AddVertex(Vector3 position, Vector2 textureCoordinates)
    {
        _vertices.Add(new Vertex(position, _color, textureCoordinates));
        if (_vertices.Count >= 3)
        {
            _faces.Add(new Face(_vertices[0], _vertices[1], _vertices[2]));
            _vertices.Clear();
        }
        return this;
    }

    public MeshBuilder AddFace(Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector2 tex1, Vector2 tex2, Vector2 tex3)
    {
        _faces.Add(new Face(new Vertex(pos1, _color, tex1), new Vertex(pos2, _color, tex2), new Vertex(pos3, _color, tex3)));
        return this;
    }

    public MeshBuilder AddFace(Vector3 pos1, Vector3 pos2, Vector3 pos3)
    {
        var uv = new Vector2(0, 0);
        return AddFace(pos1, pos2, pos3, uv, uv, uv);
    }

    public MeshBuilder Scale(float scale)
    {
        foreach (var face in _faces)
        {
            face.Vertex0.Position *= scale;
            face.Vertex1.Position *= scale;
            face.Vertex2.Position *= scale;
        }
        return this;
    }

    public Mesh CreateMesh(Shader shader)
    {
        var vertices = new float[_faces.Count * 3 * 9];
        var indices = new int[_faces.Count * 3];
        for (var i = 0; i < indices.Length; i++)
        {
            indices[i] = i;
        }
        for (var i = 0; i < _faces.Count; i++)
        {
            _faces[i].Vertex0.WriteDataTo(vertices, i * 3 * 9);
            _faces[i].Vertex1.WriteDataTo(vertices, i * 3 * 9 + 9);
            _faces[i].Vertex2.WriteDataTo(vertices, i * 3 * 9 + 18);
        }
        return new Mesh(vertices, indices, shader);
    }
}
