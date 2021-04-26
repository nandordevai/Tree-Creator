using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VertexTest : MonoBehaviour
{
    Mesh mesh;
    MeshFilter filter;

    void Awake()
    {
        mesh = new Mesh();
    }

    void BuildMesh()
    {
        int radialSubdivisions = 6;
        int nodeNum = 3;
        Vector3[] vertices = new Vector3[radialSubdivisions * nodeNum];
        int[] triangles = new int[radialSubdivisions * 6 * (nodeNum - 1)];
        int vertexId = 0;
        Vector3 pos;
        for (int i = 0; i < nodeNum; i++)
        {
            vertexId = radialSubdivisions * i;
            for (int j = 0; j < radialSubdivisions; j++)
            {
                float alpha = j * Mathf.PI * 2 / radialSubdivisions;
                pos = new Vector3(
                    .1f * Mathf.Cos(alpha),
                    i,
                    .1f * Mathf.Sin(alpha)
                );
                vertices[vertexId + j] = pos - transform.position;
            }
        }
        int t = 0;
        for (int i = 0; i < nodeNum - 1; i++)
        {
            int nodeStart = radialSubdivisions * i;
            for (int j = 0; j < radialSubdivisions; j++)
            {
                triangles[t] = nodeStart + j;
                triangles[t + 1] = nodeStart + j + radialSubdivisions;
                triangles[t + 2] = nodeStart + (j + 1) % radialSubdivisions;
                triangles[t + 3] = nodeStart + (j + 1) % radialSubdivisions;
                triangles[t + 4] = nodeStart + j + radialSubdivisions;
                triangles[t + 5] = nodeStart + (j + 1) % radialSubdivisions + radialSubdivisions;
                t += 6;
            }
        }
        // Debug.Log(String.Join(",", triangles));
        filter = GetComponent<MeshFilter>();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        filter.mesh = mesh;
    }

    void Start()
    {
        BuildMesh();
    }

    void OnDrawGizmos()
    {
        // Gizmos.color = Color.white;
        // foreach (var v in GetComponent<MeshFilter>().sharedMesh.vertices)
        //     Gizmos.DrawSphere(transform.TransformPoint(v), .02f);
    }
}
