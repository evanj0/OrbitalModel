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
    private Dictionary<object, Vertex> _vertices;
    private List<object> _keys;
    private Color4 _color;
    private Func<object, object> _keyTransformer;

    public MeshBuilder()
    {
        _keys = new();
        _vertices = new();
        _keyTransformer = obj => obj;
    }

    public MeshBuilder SetVertexColor(Color4 color)
    {
        _color = color;
        return this;
    }

    public MeshBuilder SetKeyTransformer(Func<object, object> f)
    {
        _keyTransformer = f;
        return this;
    }

    public MeshBuilder ResetKeyTransformer()
    {
        _keyTransformer = obj => obj;
        return this;
    }

    public MeshBuilder AddVertex(Vertex vertex, object key)
    {
        if (_vertices.ContainsKey(key)) throw new ArgumentException($"Vertex {key} already exists.");
        _vertices.Add(_keyTransformer(key), vertex);
        return this;
    }

    public MeshBuilder AddVertex(Vector3 position, Vector2 textureCoordinates, object key)
    {
        return AddVertex(new Vertex(position, _color, textureCoordinates), key);
    }

    public MeshBuilder AddVertex(Vector3 position, object key)
    {
        return AddVertex(position, (0, 0), key);
    }

    public MeshBuilder AddVertex(float x, float y, float z, object key)
    {
        return AddVertex((x, y, z), key);
    }

    public MeshBuilder AddTri(object key1, object key2, object key3)
    {
        if (!_vertices.ContainsKey(_keyTransformer(key1))) throw new ArgumentException($"Vertex {_keyTransformer(key1)} not found.");
        if (!_vertices.ContainsKey(_keyTransformer(key2))) throw new ArgumentException($"Vertex {_keyTransformer(key2)} not found.");
        if (!_vertices.ContainsKey(_keyTransformer(key3))) throw new ArgumentException($"Vertex {_keyTransformer(key3)} not found.");
        _keys.Add(_keyTransformer(key1));
        _keys.Add(_keyTransformer(key2));
        _keys.Add(_keyTransformer(key3));
        return this;
    }

    public MeshBuilder AddQuad(object key1, object key2, object key3, object key4)
    {
        return AddTri(key1, key2, key3).AddTri(key1, key3, key4);
    }

    public MeshBuilder JoinWith(MeshBuilder mesh)
    {
        foreach ((var key, var vertex) in mesh._vertices)
        {
            AddVertex(vertex, key);
        }
        foreach (var key in mesh._keys)
        {
            _keys.Add(key);
        }
        return this;
    }

    public MeshBuilder Scale(float scale)
    {
        foreach (var vertex in _vertices.Values)
        {
            vertex.Position *= scale;
        }
        return this;
    }

    public MeshBuilder Scale(Vector3 scale)
    {
        foreach (var vertex in _vertices.Values)
        {
            vertex.Position *= scale;
        }
        return this;
    }

    public MeshBuilder Scale(float x, float y, float z)
    {
        return Scale((x, y, z));
    }

    public MeshBuilder ScaleXY(float scale)
    {
        foreach (var vertex in _vertices.Values)
        {
            vertex.Position = (vertex.Position.X * scale, vertex.Position.Y * scale, vertex.Position.Z);
        }
        return this;
    }
    
    public MeshBuilder Translate(Vector3 translation)
    {
        foreach (var vertex in _vertices.Values)
        {
            vertex.Position += translation;
        }
        return this;
    }

    public MeshBuilder Translate(float x, float y, float z)
    {
        return Translate((x, y, z));
    }

    public Mesh CreateMesh(Shader shader)
    {
        var vertexBuffer = new float[_vertices.Count * 9];
        var indexBuffer = new int[_keys.Count];
        var addedVertices = new Dictionary<object, (int Index, Vertex Vertex)>();
        var index = 0;
        for (var i = 0; i < _keys.Count; i++)
        {
            var key = _keys[i];
            if (!addedVertices.ContainsKey(key))
            {
                addedVertices.Add(key, (index, _vertices[key]));
                index++;
            }
            indexBuffer[i] = addedVertices[key].Index;
        }
        foreach ((var i, var vertex) in addedVertices.Values)
        {
            vertex.WriteDataTo(vertexBuffer, i * 9);
        }
        return new Mesh(vertexBuffer, indexBuffer, shader);
    }
}
