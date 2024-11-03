using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Vector3;

public class ClosestPair3D
{
    private float FindClosestPairBruteForce(List<Vector3> points)
    {
        float minDist = float.MaxValue;
        int n = points.Count;

        // Compare all pairs of points
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                float dist = Distance(points[i], points[j]);
                if (dist < minDist)
                    minDist = dist;
            }
        }
        return minDist;
    }

    private float FindClosestPairDivideAndConquer(List<Vector3> points)
    {
        // Sort the points by X-axis initially
        List<Vector3> sortedByX = new(points);
        sortedByX.Sort((p1, p2) => p1.x.CompareTo(p2.x));

        // Helper function to sort by Y-axis during recursion
        List<Vector3> sortedByY = new(points);
        sortedByY.Sort((p1, p2) => p1.y.CompareTo(p2.y));

        return DivideAndConquer(sortedByX, sortedByY, 0, points.Count - 1);
    }

    private float DivideAndConquer(List<Vector3> sortedByX, List<Vector3> sortedByY, int left, int right)
    {
        // If there are less than or equal to 3 points, use brute force
        if (right - left <= 3)
        {
            return FindClosestPairBruteForce(sortedByX.GetRange(left, right - left + 1));
        }

        // Divide into two halves
        int mid = (left + right) / 2;
        Vector3 midPoint = sortedByX[mid];

        // Create lists of points on the left and right sides, sorted by Y-axis
        List<Vector3> leftByY = new();
        List<Vector3> rightByY = new();

        foreach (var p in sortedByY)
        {
            if (p.x <= midPoint.x) leftByY.Add(p);
            else rightByY.Add(p);
        }

        // Recursive calls for both halves
        float distLeft = DivideAndConquer(sortedByX, leftByY, left, mid);
        float distRight = DivideAndConquer(sortedByX, rightByY, mid + 1, right);

        // The smallest distance found so far
        float minDist = Mathf.Min(distLeft, distRight);

        // Check the strip around the dividing line
        List<Vector3> strip = new();
        foreach (var p in sortedByY)
        {
            if (Mathf.Abs(p.x - midPoint.x) < minDist)
                strip.Add(p);
        }

        // Find the closest points in the strip (brute-force check within the strip)
        return Mathf.Min(minDist, FindClosestInStrip(strip, minDist));
    }

    private float FindClosestInStrip(List<Vector3> strip, float minDist)
    {
        float result = minDist;

        // Compare each point with the next few points in the strip
        for (int i = 0; i < strip.Count; i++)
        {
            for (int j = i + 1; j < strip.Count && (strip[j].y - strip[i].y) < result; j++)
            {
                float dist = Distance(strip[i], strip[j]);
                if (dist < result)
                    result = dist;
            }
        }

        return result;
    }

    public float FindClosestPair(List<Vector3> points)
    {
        if (points.Count <= 40) return FindClosestPairBruteForce(points);
        else return FindClosestPairDivideAndConquer(points);
    }
}
