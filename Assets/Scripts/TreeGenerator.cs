﻿using System.Collections;
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
    List<Branch> extremities = new List<Branch>();
    float branchLength = .2f;
    int steps = 20;
    int currentStep = 0;
    int initialBranches = 0;
    List<Vector3> attractors = new List<Vector3>();
    List<int> activeAttractors = new List<int>();
    float randomGrowth = .1f;
    float killDistance = .4f;

    void GenerateAttractors()
    {
        var container = GameObject.Find("Attractors");
        for (var i = 0; i < numAttractors; i++)
        {
            Vector3 v = RandomVector();
            float d = Random.Range(0, 1f);
            d = Mathf.Pow(Mathf.Sin(d * Mathf.PI / 2f), 0.8f);
            d *= sphereSize;
            v *= d;
            attractors.Add(v);
        }
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

    void AssociateAttractorsWithBranches()
    {
        // Associate each attractor with the single closest node within the pre-defined attraction distance.
        activeAttractors.Clear();
        for (int i = 0; i < attractors.Count; i++)
        {
            Branch closest = null;
            foreach (var b in branches)
            {
                float d = Vector3.Distance(attractors[i], b.end);
                if (d < attractionDistance)
                {
                    if (closest == null || Vector3.Distance(attractors[i], closest.end) > d)
                    {
                        closest = b;
                    }
                }
            }
        }
    }

    void GrowTree()
    {
        activeAttractors.Clear();
        for (int i = extremities.Count - 1; i >= 0; i--)
        {
            extremities[i].attractors.Clear();
            for (int j = 0; j < attractors.Count; j++)
            {
                if (Vector3.Distance(attractors[j], extremities[i].end) < attractionDistance)
                {
                    activeAttractors.Add(j);
                    extremities[i].attractors.Add(attractors[j]);
                }
            }
            if (extremities[i].attractors.Count == 0)
            {
                extremities.RemoveAt(i);
            }
            else
            {
                Grow(extremities[i]);
            }
        }
    }

    void Grow(Branch parent)
    {
        Vector3 v = new Vector3();
        // The direction of the child of branch can be computed as the normalized sum of the normalized directions between the attraction points and the end of branch.
        foreach (var a in parent.attractors)
        {
            v += (a - parent.end);
        }
        v += RandomVector() * randomGrowth;
        v /= activeAttractors.Count + 1;
        v.Normalize();
        CreateBranch(
            start: parent.end,
            end: parent.end + v * branchLength,
            direction: v,
            parent
        );
        currentStep++;
    }

    void CreateBranch(Vector3 start, Vector3 end, Vector3 direction, Branch parent)
    {
        var branch = new Branch(start, end, direction, parent);
        branches.Add(branch);
        extremities.Add(branch);
    }

    void Start()
    {
        GenerateAttractors();
        var start = new Vector3(0, -sphereSize + 2, 0);
        CreateBranch(
            start,
            start + Vector3.up * branchLength,
            Vector3.up,
            null
        );
        // for (int i = 0; i < initialBranches; i++)
        // {
        //     Grow(branches.Last());
        // }
    }

    void Update()
    {
        if (currentStep >= steps) enabled = false;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            GrowTree();
        }
    }

    void OnDrawGizmos()
    {
        if (attractors == null)
        {
            return;
        }
        for (int i = 0; i < attractors.Count; i++)
        {
            if (activeAttractors.Contains(i))
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawSphere(attractors[i], 0.1f);
        }

        if (branches.Count > 0)
        {
            Gizmos.color = new Color(.3f, .3f, .3f, .3f);
            Gizmos.DrawSphere(branches.Last().end, attractionDistance);
            Gizmos.color = new Color(.5f, .5f, .5f, .5f);
            Gizmos.DrawSphere(branches.Last().end, killDistance);
        }

        foreach (Branch b in branches)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(b.start, b.end);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(b.end, 0.05f);
            Gizmos.DrawSphere(b.start, 0.05f);
        }
    }
}
