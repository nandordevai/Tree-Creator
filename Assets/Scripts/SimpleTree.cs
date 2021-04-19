using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTree : MonoBehaviour
{
    List<Branch> branches = new List<Branch>();
    Vector3 direction = Vector3.up;
    float branchLength = 5f;
    int steps = 3;

    public class Branch
    {
        Vector3 start;
        Vector3 end;
        Vector3 direction;

        public Branch(Vector3 start, Vector3 end, Vector3 direction)
        {
            this.start = start;
            this.end = end;
            this.direction = direction;
        }

        public void Draw()
        {
            GameObject connector = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            connector.transform.localScale = new Vector3(
                .5f,
                Vector3.Distance(start, end) / 2,
                .5f
            );
            connector.transform.LookAt(end - start);
            connector.transform.Rotate(90, 0, 0, Space.Self);
            GameObject ss = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject se = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ss.transform.position = start;
            se.transform.position = end;
            connector.transform.position = Vector3.Lerp(start, end, .5f);
        }
    }

    void Start()
    {
        Vector3 currentStart = Vector3.zero;
        Vector3 currentEnd;
        for (int i = 0; i < steps; i++)
        {
            currentEnd = currentStart + (direction * branchLength);
            branches.Add(new Branch(
                start: currentStart,
                end: currentEnd,
                direction: Vector3.up
            ));
            currentStart = currentEnd;
        }
        // end: new Vector3(2f, 5f, 0)));
        // branches.Add(new Branch(start: new Vector3(2f, 5f, 0), end: new Vector3(3f, 10f, 2f)));
        // branches.Add(new Branch(start: new Vector3(2f, 5f, 0), end: new Vector3(1f, 8f, 2f)));
        // branches.Add(new Branch(start: new Vector3(1f, 8f, 2f), end: new Vector3(-1f, 12f, 1f)));
        // branches.Add(new Branch(start: new Vector3(-1f, 12f, 1f), end: new Vector3(0, 17f, -1f)));

        foreach (var b in branches)
        {
            b.Draw();
        }
    }
}
