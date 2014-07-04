using System;
using System.Collections.Generic;
using System.Text;

namespace ConvexHull
{
    // This class contains a sequential implementation of the
    // convex hull algorithm.
    // The idea behind the algorithm is to sort the points
    // in increasing order according to their x-coordinate.
    // Then we traverse the points from left to right by always
    // testing the last three points - if the make a right turn we 
    // keep them, add a new point and repeat the process. Otherwise 
    // we delete the middle point and repeat the process with the last
    // three points.
    // Once we are done we'll have the upper hull. 
    // Then we repeat the hole process again, but this time we traverse
    // the points from right to left, thus finding the lower hull.
    // Finally we combine the upper and lower hull.
    public class SeqConvexHull
    {

        // Find the convex hull
        public List<Point> ConvexHull(List<Point> points)
        {
            int length = points.Count;
            if (length < 3)
                return points;

            // Sort the points
            points.Sort();

            // Find upper hull
            List<Point> U = new List<Point>(length);

            U.Insert(0, points[0]);
            U.Insert(1, points[1]);
            for (int i = 2; i < length; i++)
            {
                while (U.Count > 1 && STL.Turn(U[U.Count-2], U[U.Count-1], points[i]) <= 0)
                {
                    U.RemoveAt(U.Count-1);
                }
                U.Add(points[i]);
            }

            // Find lower hull
            List<Point> L = new List<Point>(length);

            L.Insert(0, points[length-1]);
            L.Insert(1, points[length-2]);
            for (int j = length - 3; j >= 0; j--)
            {
                while (L.Count > 1 && STL.Turn(L[L.Count - 2], L[L.Count - 1], points[j]) <= 0)
                {
                    L.RemoveAt(L.Count - 1);
                }
                L.Add(points[j]);
            }

            // Combine upper hull and lower hull
            U.RemoveAt(U.Count - 1);
            U.AddRange(L);
            U.RemoveAt(U.Count - 1);

            return U;
        }
    }
}
