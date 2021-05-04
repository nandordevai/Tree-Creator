using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SCTree : MonoBehaviour
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

    List<Attractor> attractors = new List<Attractor>();
    List<Node> nodes = new List<Node>();
    float timer = 0f;
    List<Attractor> activeAttractors = new List<Attractor>();
    Mesh mesh;
    MeshFilter filter;

    public class Attractor
    {
        public Vector3 position;

        public Attractor()
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
            d *= SCTree.size;
            v *= d;
            this.position = v;
        }
    }

    public class Node
    {
        public Vector3 position;
        public Vector3 direction;
        public Node parent;
        public List<Attractor> attractors = new List<Attractor>();
        public bool isTrunk = false;
        public int vertexStart;
        public int maxChildrenDepth = 0;
        public float size;
        public bool isGrowing = true;

        float randomGrowth = .1f;

        public Node(Vector3 position, Vector3 direction, Node parent)
        {
            this.position = position;
            this.direction = direction;
            this.parent = parent;
        }

        public void IncreaseChildrenDepth(int d)
        {
            if (d > maxChildrenDepth)
            {
                maxChildrenDepth = d;
            }
            if (parent != null)
            {
                parent.IncreaseChildrenDepth(d + 1);
            }
        }

        public void SetSize(float initialSize)
        {
            size = (Mathf.Pow(maxChildrenDepth, 1.1f) / 500) + initialSize;
        }

        Vector3 RandomVector()
        {
            float alpha = Random.Range(0, Mathf.PI);
            float theta = Random.Range(0, Mathf.PI * 2f);
            Vector3 v = new Vector3(
                Mathf.Cos(theta) * Mathf.Sin(alpha),
                Mathf.Sin(theta) * Mathf.Sin(alpha),
                Mathf.Cos(alpha)
            );
            return v;
        }

        public Vector3 GetGrowthDirection()
        {
            if (attractors.Count == 0)
            {
                return direction;
            }
            else
            {
                Vector3 v = Vector3.zero;
                foreach (var a in attractors)
                {
                    v += a.position - position;
                }
                v += RandomVector() * randomGrowth;
                v /= attractors.Count + 1;
                return v.normalized;
            }
        }
    }

    void Awake()
    {
        mesh = new Mesh();
    }

    void Associate()
    {
        // Associate each attractor with the single closest node within the pre-defined attraction distance.
        foreach (var n in nodes)
        {
            n.attractors.Clear();
        }
        activeAttractors.Clear();
        foreach (var a in attractors)
        {
            Node node = GetClosestNode(a, attractionDistance);
            if (node != null)
            {
                node.attractors.Add(a);
                activeAttractors.Add(a);
            }
        }
    }

    Node GetClosestNode(Attractor attractor, float maxDistance)
    {
        Node closest = null;
        float minDistance = 0f;
        foreach (var n in nodes)
        {
            float currentDistance = Vector3.Distance(attractor.position, n.position);
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

    List<Node> GetNodesInDistance(Attractor attractor, float distance)
    {
        List<Node> closeNodes = new List<Node>();
        foreach (var n in nodes)
        {
            if (Vector3.Distance(attractor.position, n.position) < distance)
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
            attractors.Add(new Attractor());
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
                vertices[vIdx] = pos - transform.position;
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
        Node rootNode = new Node(new Vector3(0, -size - initialNodeDistance, 0), Vector3.up, null);
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
            Gizmos.DrawSphere(a.position, .05f);
        }

        foreach (var a in activeAttractors)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(a.position, .05f);
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
