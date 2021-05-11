using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3 position;
    public Vector3 direction;
    public Node parent;
    public List<Vector3> attractors = new List<Vector3>();
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
                v += a - position;
            }
            v += RandomVector() * randomGrowth;
            v /= attractors.Count + 1;
            return v.normalized;
        }
    }
}
