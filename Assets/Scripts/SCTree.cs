using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public partial class SCTree : MonoBehaviour
{
    public int numAttractors = 400;
    public float branchLength = .2f;
    public static float size = 5f;
    public float attractionDistance = .8f;
    public float updateInterval = 1f;
    public float pruneDistance = .4f;
    public int radialSubdivisions = 5;
    public float branchDiameter = .2f;
    public float initialNodeDistance = 2f;
    public GameObject growthArea;
    public float growthAreaSize = 2f;

    List<Vector3> attractors = new List<Vector3>();
    List<Node> nodes = new List<Node>();
    float timer = 0f;
    List<Vector3> activeAttractors = new List<Vector3>();
    Mesh mesh;
    MeshFilter filter;
    Octree nodeOctree;
    Octree attractorOctree;

    void Awake()
    {
        mesh = new Mesh();
        nodeOctree = new Octree(new BoundingBox(transform.position, size));
        attractorOctree = new Octree(new BoundingBox(transform.position, size));
    }

    void Associate()
    {
        // Associate each attractor with the single closest node within the pre-defined attraction distance.
        foreach (var n in nodes)
        {
            n.attractors.Clear();
        }
        activeAttractors.Clear();
        List<Node> growthAttractors = new List<Node>();
        attractorOctree.Query(
            new BoundingBox(growthArea.transform.position, growthAreaSize),
            ref growthAttractors
        );
        foreach (var a in growthAttractors)
        {
            Node node = GetClosestNode(a.position, attractionDistance);
            if (node != null)
            {
                node.attractors.Add(a.position);
                activeAttractors.Add(a.position);
            }
        }
    }

    Node GetClosestNode(Vector3 attractor, float maxDistance)
    {
        Node closest = null;
        float minDistance = 0f;
        List<Node> qNodes = new List<Node>();
        nodeOctree.Query(new BoundingBox(attractor, maxDistance), ref qNodes);
        foreach (var n in qNodes)
        {
            float currentDistance = Vector3.Distance(attractor, n.position);
            if (currentDistance > maxDistance) continue;
            if (closest == null
            || currentDistance < minDistance)
            {
                closest = n;
                minDistance = currentDistance;
            }
        }
        return closest;
    }

    List<Node> GetNodesInDistance(Vector3 attractor, float distance)
    {
        List<Node> closeNodes = new List<Node>();
        List<Node> qNodes = new List<Node>();
        nodeOctree.Query(new BoundingBox(attractor, distance), ref qNodes);
        foreach (var n in qNodes)
        {
            if (Vector3.Distance(attractor, n.position) < distance)
            {
                closeNodes.Add(n);
            }
        }
        return closeNodes;
    }

    // TODO: set trunk to false when not needed anymore
    void Grow()
    {
        List<Node> newNodes = new List<Node>();
        foreach (var node in nodes)
        {
            node.isGrowing = false;
            if (node.attractors.Count == 0 && !node.isTrunk)
            {
                continue;
            }
            Vector3 direction = node.GetGrowthDirection();
            Node newNode = new Node(
                node.position + (direction * branchLength),
                direction,
                node
            );
            newNodes.Add(newNode);
            nodeOctree.Insert(newNode);
            node.IncreaseChildrenDepth(1);
            if (node.isTrunk)
            {
                node.isTrunk = false;
                newNode.isTrunk = true;
            }
        }
        nodes.AddRange(newNodes);
    }

    void GenerateAttractors()
    {
        for (var i = 0; i < numAttractors; i++)
        {
            float alpha = Random.Range(0, Mathf.PI);
            float theta = Random.Range(0, Mathf.PI * 2f);
            Vector3 v = new Vector3(
                Mathf.Cos(theta) * Mathf.Sin(alpha),
                Mathf.Sin(theta) * Mathf.Sin(alpha),
                Mathf.Cos(alpha)
            );
            float d = Random.Range(0, 1f);
            d = Mathf.Pow(Mathf.Sin(d * Mathf.PI / 2f), 0.8f);
            d *= size;
            v *= d;
            v += transform.position;
            attractors.Add(v);
            attractorOctree.Insert(new Node(v, Vector3.zero, null));
        }
    }

    void Prune()
    {
        for (int i = attractors.Count - 1; i >= 0; i--)
        {
            var closeNodes = GetNodesInDistance(attractors[i], pruneDistance);
            if (closeNodes.Count > 0)
            {
                attractors.RemoveAt(i);
            }
        }
    }

    void BuildMesh()
    {
        mesh.Clear();
        mesh.vertices = CalculateVertices();
        mesh.triangles = CalculateTriangles();
        mesh.RecalculateNormals();
        filter = GetComponent<MeshFilter>();
        filter.mesh = mesh;
    }

    Vector3[] CalculateVertices()
    {
        Vector3[] vertices = new Vector3[nodes.Count * radialSubdivisions];
        Quaternion q;
        Vector3 pos;
        int vIdx = 0;
        foreach (var node in nodes)
        {
            node.vertexStart = vIdx;
            node.SetSize(branchDiameter);
            q = Quaternion.FromToRotation(Vector3.up, node.direction);
            for (int i = 0; i < radialSubdivisions; i++)
            {
                float alpha = i * Mathf.PI * 2 / radialSubdivisions;
                pos = new Vector3(
                    node.size * Mathf.Cos(alpha),
                    0,
                    node.size * Mathf.Sin(alpha)
                );
                pos = q * pos;
                if (node.parent != null && node.isGrowing)
                {
                    pos += Vector3.Lerp(
                        node.parent.position,
                        node.position,
                        timer / updateInterval
                    );
                }
                else
                {
                    pos += node.position;
                }
                vertices[vIdx] = pos - transform.localPosition;
                vIdx++;
            }
        }
        return vertices;
    }

    int WrapVertexIndex(int start, int increment)
    {
        if (increment >= radialSubdivisions)
        {
            increment -= radialSubdivisions;
        }
        return start + increment;
    }

    int[] CalculateTriangles()
    {
        int[] triangles = new int[(nodes.Count - 1) * 6 * radialSubdivisions];
        int t = 0;
        foreach (var node in nodes)
        {
            if (node.parent == null) continue;

            for (int j = 0; j < radialSubdivisions; j++)
            {
                triangles[t] = node.vertexStart + j;
                triangles[t + 1] = node.parent.vertexStart + j;
                triangles[t + 2] = WrapVertexIndex(node.vertexStart, j + 1);

                triangles[t + 3] = WrapVertexIndex(node.vertexStart, j + 1);
                triangles[t + 4] = node.parent.vertexStart + j;
                triangles[t + 5] = WrapVertexIndex(node.parent.vertexStart, j + 1);
                t += 6;
            }
        }
        return triangles;
    }

    void Start()
    {
        GenerateAttractors();
        Node rootNode = new Node(
            new Vector3(0, -size - initialNodeDistance, 0) + transform.localPosition,
            Vector3.up,
            null
        );
        rootNode.isTrunk = true;
        nodes.Add(rootNode);
    }

    void Update()
    {
        if (attractors.Count == 0) enabled = false;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            Associate();
            Grow();
            Prune();
            timer = 0f;
        }
        BuildMesh();
    }

    void OnDrawGizmos()
    {
        foreach (var a in attractors)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(a, .05f);
        }

        foreach (var a in activeAttractors)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(a, .05f);
        }

        foreach (var n in nodes)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(n.position, .05f);
            if (n.parent != null)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(n.position, n.parent.position);
            }
        }
    }
}
