using System.Collections.Generic;
using UnityEngine;

public class Octree
{
    BoundingBox boundary;
    int capacity;
    List<Node> points = new List<Node>();
    bool divided = false;
    Octree une = null;
    Octree unw = null;
    Octree use = null;
    Octree usw = null;
    Octree lne = null;
    Octree lnw = null;
    Octree lse = null;
    Octree lsw = null;

    public Octree(BoundingBox boundary, int capacity = 4)
    {
        this.boundary = boundary;
        this.capacity = capacity;
    }

    public bool Insert(Node node)
    {
        if (!boundary.Contains(node.position)) return false;
        if (points.Count < capacity)
        {
            points.Add(node);
        }
        else
        {
            if (!divided)
            {
                Subdivide();
            }
            une.Insert(node);
            unw.Insert(node);
            use.Insert(node);
            usw.Insert(node);
            lne.Insert(node);
            lnw.Insert(node);
            lse.Insert(node);
            lsw.Insert(node);
        }
        return true;
    }

    void Subdivide()
    {
        float w = boundary.width;
        float h = boundary.height / 2;
        float d = boundary.depth;
        une = new Octree(
            new BoundingBox(
                new Vector3(
                    boundary.center.x + w,
                    boundary.center.y + h,
                    boundary.center.z + d
                ),
                w, h, d
            ),
            capacity
        );
        unw = new Octree(
            new BoundingBox(
                new Vector3(
                    boundary.center.x - w,
                    boundary.center.y + h,
                    boundary.center.z + d
                ),
                w, h, d
            ),
            capacity
        );
        use = new Octree(
            new BoundingBox(
                new Vector3(
                    boundary.center.x + w,
                    boundary.center.y + h,
                    boundary.center.z - d
                ),
                w, h, d
            ),
            capacity
        );
        usw = new Octree(
            new BoundingBox(
                new Vector3(
                    boundary.center.x - w,
                    boundary.center.y + h,
                    boundary.center.z - d
                ),
                w, h, d
            ),
            capacity
        );
        lne = new Octree(
            new BoundingBox(
                new Vector3(
                    boundary.center.x + w,
                    boundary.center.y - h,
                    boundary.center.z + d
                ),
                w, h, d
            ),
            capacity
        );
        lnw = new Octree(
            new BoundingBox(
                new Vector3(
                    boundary.center.x - w,
                    boundary.center.y - h,
                    boundary.center.z + d
                ),
                w, h, d
            ),
            capacity
        );
        lse = new Octree(
            new BoundingBox(
                new Vector3(
                    boundary.center.x + w,
                    boundary.center.y - h,
                    boundary.center.z - d
                ),
                w, h, d
            ),
            capacity
        );
        lsw = new Octree(
            new BoundingBox(
                new Vector3(
                    boundary.center.x - w,
                    boundary.center.y - h,
                    boundary.center.z - d
                ),
                w, h, d
            ),
            capacity
        );
        divided = true;
    }

    public void Query(BoundingBox range, ref List<Node> found)
    {
        if (!boundary.Intersects(range))
        {
            return;
        }
        foreach (var n in points)
        {
            if (range.Contains(n.position))
            {
                found.Add(n);
            }
        }
        if (divided)
        {
            une.Query(range, ref found);
            unw.Query(range, ref found);
            use.Query(range, ref found);
            usw.Query(range, ref found);
            lne.Query(range, ref found);
            lnw.Query(range, ref found);
            lse.Query(range, ref found);
            lsw.Query(range, ref found);
        }
    }

    public void Show()
    {
        foreach (var n in points)
        {
            GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.transform.localScale = new Vector3(.05f, .05f, .05f);
            s.transform.position = n.position;
        }
        if (divided)
        {
            une.Show();
            unw.Show();
            use.Show();
            usw.Show();
            lne.Show();
            lnw.Show();
            lse.Show();
            lsw.Show();
        }
    }
}
