using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LineDrawComponent : MonoBehaviour
{
    private static Material _lineMaterial;

    public Vector3[] Vertices
    {
        get;
        set;
    } = new Vector3[0];

    public List<KeyValuePair<int, int>> Edges
    {
        get;
        set;
    } = new List<KeyValuePair<int, int>>();


    private static void CreateLineMaterial()
    {
        if (!_lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            _lineMaterial = new Material(shader);
            _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            _lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    // Will be called after all regular rendering is done
    public void OnRenderObject()
    {
        CreateLineMaterial();
        
        // Apply the line material
        _lineMaterial.SetPass(0);

        GL.PushMatrix();
        // Set transformation matrix for drawing to
        // match our transform
        GL.MultMatrix(transform.localToWorldMatrix);

        // Draw lines
        GL.Begin(GL.LINES);
        
        foreach (var edge in Edges)
        {
            var source = Vertices[edge.Key];
            var target = Vertices[edge.Value];
            
            GL.Color(new Color(1, 1, 1, 0.8F));
            GL.Vertex3(source.x,source.y, source.z);
            GL.Vertex3(target.x,target.y, target.z);
        }
        
        GL.End();
        GL.PopMatrix();
    }
}
