using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    public static PathManager instance;
    [System.Serializable]
    public class Path
    {
        public List<Transform> wayPoints;
        public Range roundsActive;
    }

    [System.Serializable]
    public struct Range
    {
        public int start;
        public int end;
    }

    public List<Path> paths;

    private void Start()
    {
        if (instance == null) instance = this;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (Path p in paths)
        {
            for(int i = 0; i < p.wayPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(p.wayPoints[i].position,p.wayPoints[i+1].position);
                Gizmos.DrawWireSphere(p.wayPoints[i].position, 0.1f);
            }

            Gizmos.DrawWireSphere(p.wayPoints[p.wayPoints.Count - 1].position, 0.1f);
        }
    }

    public Path GetClosestActivePath(Vector2 aPos)
    {
        float lClosestDist = Mathf.Infinity;
        Path lClosestPath = new Path();
        foreach (Path p in paths)
        {
            if (!isPathActive(p)) continue;

            float lClosesetPathDist = Mathf.Infinity;
            for (int i = 0; i < p.wayPoints.Count; i++)
            {
                float lDistToWaypoint = Vector2.Distance(aPos, (Vector2)p.wayPoints[i].position);
                if (lDistToWaypoint < lClosesetPathDist)
                {
                    lClosesetPathDist = lDistToWaypoint;
                }
            }
            if(lClosesetPathDist < lClosestDist)
            {
                lClosestDist = lClosesetPathDist;
                lClosestPath = p;
            }
        }
        return lClosestPath;
    }

    public bool isPathActive(Path aPath)
    {
        if(aPath.roundsActive.start <= GameManager.instance.CurrentRound && aPath.roundsActive.end >= GameManager.instance.CurrentRound)
        {
            return true;
        }
        return false;
    }
}
