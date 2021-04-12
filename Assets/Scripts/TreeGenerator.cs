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

    float timer = 0f;
    float updateInterval = .1f;
    List<Branch> branches = new List<Branch>();
    float branchLength = 1f;
    int steps = 10;
    int currentStep = 5;
    int initialBranches = 5;

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
        var firstBranch = new Branch(Vector3.zero, Vector3.up, Vector3.up, null);
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
            currentStep++;
            Grow();
        }
    }
}
