using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


public class KDTREETEST : MonoBehaviour
{
    public bool useRandomPoints = true;
    public Vector3[] nonRandomPoints;
    public int points = 1000;
    public float SIZE = 1000f;
    public int searchTimes = 1;

    public int stage = 3;
    KDTree<CustomVector2> tree;
    private List<CustomVector2> pointsList = new List<CustomVector2>();
    private List<Vector3> interpolatedPoints = new List<Vector3>();
    public class CustomVector2 : IKDTreeItem
    {
        public CustomVector2(Vector2f pos, float elevation)
        {
            Position = pos;
            this.elevation = elevation;
        }
        public float elevation;
        public Vector2f Position { get; set; }

        public float this[int dimension] => Position[dimension];
    }

    public void Go(int first, int last)
    {
        var stages = new List<Tuple<string, Action>>();

        void TimeIt(string name, Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            UnityEngine.Debug.Log($"{name}: {stopwatch.ElapsedMilliseconds} ms");
        }

        stages.Add(new Tuple<string, Action>("Place points...",
            () =>
            {
                pointsList.Clear();
                interpolatedPoints.Clear();
                if (useRandomPoints)
                {
                    for (int i = 0; i < points; i++)
                    {
                        Vector2f pos = new Vector2f(UnityEngine.Random.Range(-SIZE, SIZE), UnityEngine.Random.Range(-SIZE, SIZE));
                        var point = new CustomVector2(pos, UnityEngine.Random.Range(0, 5));
                        pointsList.Add(point);
                    }
                }
                else
                {
                    foreach (var point in nonRandomPoints)
                    {
                        var p = new CustomVector2(new Vector2f(point.x, point.z), point.y);
                        pointsList.Add(p);
                    }
                }
            }));


        stages.Add(new Tuple<string, Action>("Build Tree...",
            () =>
            {
                tree = new KDTree<CustomVector2>();
                tree.Insert(pointsList);
            }));

        stages.Add(new Tuple<string, Action>("Search Tree...",
            () =>
            {
                int treversed = 0;

                for (int i = 0; i < searchTimes; i++)
                {
                    Vector2 pos = new Vector2(UnityEngine.Random.Range(-SIZE, SIZE), UnityEngine.Random.Range(-SIZE, SIZE));
                    tree.FindNearestNeighbors(new Vector2f(pos.x, pos.y), 5, ref treversed);
                    //interpolatedPoints.Add(new Vector3(np.Position.x, np.elevation, np.Position.y));
                }
            }));

        for (int i = first; i < last; i++)
        {
            TimeIt(stages[i].Item1, stages[i].Item2);
        }
    }

    void OnValidate()
    {
        if(stage < 0)
        {
            stage = 0;
        }
        else if(stage > 4)
        {
            stage = 4;
        }
    }
    
    void OnDrawGizmos()
    {
        if(pointsList.Count > 0)
        {
            foreach(var point in pointsList)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(new Vector3(point.Position.x, point.elevation, point.Position.y), 1f);
            }
        }
        /* if(interpolatedPoints.Count > 0)
        {
            foreach(var point in interpolatedPoints)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(new Vector3(point.x, point.y, point.z), 1f);
            }
        } */
    }
}
