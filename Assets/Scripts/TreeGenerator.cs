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

    public int numAttractors = 100;
    public float sphereSize = 5f;

    float timer = 0f;
    float updateInterval = .1f;
    List<Branch> branches = new List<Branch>();
    float branchLength = 1f;
    int steps = 10;
    int currentStep = 0;
    int initialBranches = 0;
    List<Vector3> attractors = new List<Vector3>();

    void GenerateAttractors()
    {
        Debug.Log(numAttractors);
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
        }
    }

    void Grow()
    {
        var parent = branches.Last();
        var branch = new Branch(
            parent.end,
            parent.end + new Vector3(0, branchLength, 0),
            Vector3.up,
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
            connector.transform.position = b.start + new Vector3(0, -branchLength / 2, 0);
            connector.transform.localScale = new Vector3(.1f, .5f, .1f);
        }
    }

    void Start()
    {
        GenerateAttractors();
        var firstBranch = new Branch(
            new Vector3(0, -sphereSize, 0),
            new Vector3(0, -sphereSize, 0) + Vector3.up,
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
