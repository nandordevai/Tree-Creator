using UnityEngine;

public class BoundingBox
{
    public Vector3 center;
    public float width;
    public float height;
    public float depth;

    public BoundingBox(Vector3 center, float width, float height, float depth)
    {
        this.center = center;
        this.width = width;
        this.height = height;
        this.depth = depth;
    }

    public BoundingBox(Vector3 center, float size)
    {
        this.center = center;
        this.width = size;
        this.height = size;
        this.depth = size;
    }

    public bool Contains(Vector3 point)
    {
        return point.x >= center.x - this.width &&
            point.x <= center.x + this.width &&
            point.y >= center.y - this.height &&
            point.y <= center.y + this.height &&
            point.z >= center.z - this.depth &&
            point.z <= center.z + this.depth;
    }

    public bool Intersects(BoundingBox other)
    {
        float xDistance = Mathf.Abs(this.center.x - other.center.x) - this.width - other.width;
        float yDistance = Mathf.Abs(this.center.y - other.center.y) - this.height - other.height;
        float zDistance = Mathf.Abs(this.center.z - other.center.z) - this.depth - other.depth;
        return xDistance < 0 && yDistance < 0 && zDistance < 0;
    }
}
