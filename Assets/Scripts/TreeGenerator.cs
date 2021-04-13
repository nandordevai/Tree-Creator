using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TreeGenerator : MonoBehaviour
{
    public class Branch
    {
        public Vector3 start;
        public Vector3 end;
        public Vector3 direction;
        public Branch parent;
        public List<Branch> children = new List<Branch>();
        public List<Vector3> attractors = new List<Vector3>();

        public Branch(Vector3 start, Vector3 end, Vector3 direction, Branch parent)
        {
            this.start = start;
            this.end = end;
            this.direction = direction;
            this.parent = parent;
        }
    }

    int numAttractors = 400;
    float sphereSize = 5f;
    float attractionDistance = .8f;
    float timer = 0f;
    float updateInterval = 1f;
    List<Branch> branches = new List<Branch>();
    float branchLength = 1f;
    int steps = 10;
    int currentStep = 0;
    int initialBranches = 0;
    List<Vector3> attractors = new List<Vector3>();

    void GenerateAttractors()
    {
        var container = GameObject.Find("Attractors");
        for (var i = 0; i < numAttractors; i++)
        {
            float alpha = Random.Range(0, Mathf.PI);
            float theta = Random.Range(0, 2 * Mathf.PI);
            float d = Random.Range(0, sphereSize);
            Vector3 v = new Vector3(
                Mathf.Cos(theta) * Mathf.Sin(alpha),
                Mathf.Sin(theta) * Mathf.Sin(alpha),
                Mathf.Cos(alpha)
            );
            v *= d;
            attractors.Add(v);
            // DEBUG
            GameObject attr = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            attr.transform.position = v;
            attr.GetComponent<MeshRenderer>().material.color = Color.blue;
            attr.transform.localScale = new Vector3(.1f, .1f, .1f);
            attr.transform.parent = container.transform;
        }
    }

    void Grow()
    {
        var parent = branches.Last();
        var activeAttractors = new List<Vector3>();
        foreach (var a in attractors)
        {
            if (Vector3.Distance(a, parent.end) < attractionDistance)
            {
                activeAttractors.Add(a);
            }
        }
        Vector3 v = Vector3.zero;
        if (activeAttractors.Count > 0)
        {
            // The direction of the child of branch can be computed as the normalized sum of the normalized directions between the attraction points and the end of branch.
            foreach (var attr in activeAttractors)
            {
                v += (attr - parent.end);
            }
            v /= activeAttractors.Count;
            v.Normalize();
        }
        else
        {
            v = parent.direction;
        }
        var branch = new Branch(
            parent.end,
            parent.end + v * branchLength,
            v,
            parent
        );
        branches.Add(branch);
        Draw(branch);
        currentStep++;
    }

    void Draw(Branch b)
    {
        GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        node.transform.position = b.start;
        node.transform.localScale = new Vector3(.2f, .2f, .2f);
        if (branches.Count > 1)
        {
            GameObject connector = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            // connector.transform.position = Vector3.Lerp(start, end, .5f);
            connector.transform.position = Vector3.Lerp(
                b.start,
                b.end, //b.start + new Vector3(0, -branchLength / 2, 0),
                .5f
            );
            connector.transform.localScale = new Vector3(.1f, .5f, .1f);
            connector.transform.LookAt(b.end);
            connector.transform.Rotate(90, 0, 0, Space.Self);
        }
    }

    void Start()
    {
        GenerateAttractors();
        var start = new Vector3(0, -sphereSize + 2, 0);
        var firstBranch = new Branch(
            start,
            start + Vector3.up,
            Vector3.up,
            null
        );
        branches.Add(firstBranch);
        Draw(firstBranch);
        for (int i = 0; i < initialBranches; i++)
        {
            Grow();
        }
    }

    void Update()
    {
        if (currentStep >= steps) enabled = false;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            Grow();
        }
    }
}
