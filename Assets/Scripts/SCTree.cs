﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCTree : MonoBehaviour
{
    public int numAttractors = 400;
    public float branchLength = .2f;
    public static float size = 5f;
    public float attractionDistance = .8f;
    public float updateInterval = 1f;

    List<Attractor> attractors = new List<Attractor>();
    List<Node> nodes = new List<Node>();
    List<Node> activeNodes = new List<Node>();
    float timer = 0f;
    List<Attractor> activeAttractors = new List<Attractor>();

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

        float randomGrowth = .1f;

        public Node(Vector3 position, Vector3 direction, Node parent)
        {
            this.position = position;
            this.direction = direction;
            this.parent = parent;
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

    void Associate()
    {
        foreach (var n in nodes)
        {
            n.attractors.Clear();
        }
        activeAttractors.Clear();
        // Associate each attractor with the single closest node within the pre-defined attraction distance.
        foreach (var a in attractors)
        {
            Node node = GetClosestNode(a, GetNodesInAttractionZone(a));
            if (node != null)
            {
                node.attractors.Add(a);
                activeAttractors.Add(a);
            }
        }
    }

    Node GetClosestNode(Attractor attractor, List<Node> nodes)
    {
        Node closest = null;
        float distance = 0f;
        foreach (var n in nodes)
        {
            if ((closest == null)
            || (Vector3.Distance(attractor.position, n.position) < distance))
            {
                closest = n;
                distance = Vector3.Distance(attractor.position, n.position);
            }
        }
        return closest;
    }

    List<Node> GetNodesInAttractionZone(Attractor a)
    {
        List<Node> nodes = new List<Node>();
        foreach (var n in nodes)
        {
            if (Vector3.Distance(a.position, n.position) < attractionDistance)
            {
                nodes.Add(n);
            }
        }
        return nodes;
    }

    void Grow()
    {
        for (int i = activeNodes.Count - 1; i >= 0; i--)
        {
            Node node = activeNodes[i];
            Vector3 direction = node.GetGrowthDirection();
            Node newNode = new Node(
                node.position + (direction * branchLength),
                direction,
                node
            );
            activeNodes.RemoveAt(i);
            activeNodes.Add(newNode);
            nodes.Add(node);
        }
    }

    void GenerateAttractors()
    {
        for (var i = 0; i < numAttractors; i++)
        {
            attractors.Add(new Attractor());
        }
    }

    void Start()
    {
        GenerateAttractors();
        Node rootNode = new Node(new Vector3(0, -size + 2f, 0), Vector3.up, null);
        nodes.Add(rootNode);
        activeNodes.Add(rootNode);
    }

    void Update()
    {
        if (attractors.Count == 0) enabled = false;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            Associate();
            Grow();
            timer = 0f;
        }
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
