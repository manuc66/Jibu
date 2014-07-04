using System;
using System.Collections.Generic;
using System.Text;
using Jibu;

namespace ConvexHull
{
    // This class contains a parallel implementation of the
    // convex hull algorithm.
    //
    // The algorithm is a divide & conquer algorithm.
    // We can easely divide the sequential algorithm in two parts, One
    // finding the upper hull and one finding the lower hull. This will
    // only exploit some of the concurrency in the problem and it's not
    // a scalable solution. 
    //
    // We need to exploit the concurrency in finding the upper hull and
    // the lower hull. 
    // Fortunately we can find the upper hull of a sorted set of points
    // by splitting the points in two, find the upper hull for each part
    // and finally merge the two upper hulls to one upper hull.
    // 
    // The final solution is scalable and gives a much better exploration
    // of the concurrency in the problem.
    public class ParConvexHull
    {
        // The main algorithm is fairly simple. We simply
        // sort the points and find the lower and upper hulls
        // concurrently. Finally we combine them and return
        // the points in the convex hull.
        public List<Point> ConvexHull(List<Point> points)
        {
            int length = points.Count;

            // Sort the points, see the STL class
            // for more information on the 
            // parallel quicksort implementation.
            STL.QuickSort(points, 10);

            // Find Upper and Lower hull
            // The Parallel.Gather is similar to the Parallel.Run method. It runs the submitted
            // tasks concurrently, gather the result from each task and returns array containing
            // all results.
            List<Point>[] LU = Parallel.Gather(
                                    new UpperHull(points, 0, length-1, 10), 
                                    new LowerHull(points, 0, length-1, 10));

            // Combine Upper and Lower hull
            LU[0].RemoveAt(LU[0].Count - 1);
            LU[0].AddRange(LU[1]);
            LU[0].RemoveAt(LU[0].Count - 1);

            return LU[0];
        }
    }


    // UpperHull is an Future that finds and returns the
    // upper hull for a sorted set of points. 
    public class UpperHull : Future<List<Point>>
    {
        private List<Point> points;
        private int start, end, grainSize;

        // UpperHull is initialised with the point list, the 
        // start index and the end index.
        public UpperHull(List<Point> points, int start, int end, int grainSize)
        {
            this.points = points;
            this.start = start;
            this.end = end;
            this.grainSize = grainSize;
        }


        // If the number of points is less than the grainSize, the Run method
        // simply finds the upper hull by going through all points from left to
        // right, as described in the sequential convex hull algorithm.
        // Otherwise we split the points in two and find the upper hulls
        // concurrently. 
        // Finally we merge the two upper hulls and return the upper hull.
        public override List<Point> Run()
        {
            int length = end - start + 1;

            if (length < grainSize)
            {
                List<Point> U = new List<Point>(length);

                int j = start;
                while (j < start + 2 && j <= end)
                {
                    U.Add(points[j]);
                    j++;
                }
                for (int i = start + 2; i <= end; i++)
                {
                    while (U.Count > 1 && STL.Turn(U[U.Count - 2], U[U.Count - 1], points[i]) <= 0)
                    {
                        U.RemoveAt(U.Count - 1);
                    }
                    U.Add(points[i]);
                }
                return U;
            }
            else
            {
                int middle = (length / 2) + start;
                // The Parallel.Gather is similar to the Parallel.Run method. It runs the submitted
                // tasks concurrently, gather the result from each task and returns array containing
                // all results.
                List<Point>[] res = Parallel.Gather(
                                            new UpperHull(points, start, middle, grainSize),
                                            new UpperHull(points, middle + 1, end, grainSize));

                return MergeUpperHulls(res[0], res[1]);
            }
        }

        // MergeUpperHulls merge two upper hulls in O(n) time.
        private List<Point> MergeUpperHulls(List<Point> P1, List<Point> P2)
        {
            int lengthP1 = P1.Count;
            int lengthP2 = P2.Count;
            int currentP1Index = lengthP1 - 1;
            int currentP2Index = 0;
            bool done = false;

            while (!done)
            {
                done = true;
                if (currentP2Index < lengthP2 - 1 && STL.Turn(P1[currentP1Index], P2[currentP2Index], P2[currentP2Index + 1]) < 0)
                {
                    done = false;
                    currentP2Index++;
                    while (currentP2Index < lengthP2 - 1 && !UpperTangent(P1[currentP1Index], P2[currentP2Index], P2[currentP2Index + 1], P2[currentP2Index - 1]))
                    {
                        currentP2Index++;
                    }
                }

                if (currentP1Index > 0 && STL.Turn(P2[currentP2Index], P1[currentP1Index], P1[currentP1Index - 1]) > 0)
                {
                    done = false;
                    currentP1Index--;
                    while (currentP1Index > 0 && !UpperTangent(P2[currentP2Index], P1[currentP1Index], P1[currentP1Index + 1], P1[currentP1Index - 1]))
                    {
                        currentP1Index--;
                    }
                }
            }

            List<Point> U = new List<Point>();
            U.AddRange(P1.GetRange(0, currentP1Index+1));
            U.AddRange(P2.GetRange(currentP2Index, lengthP2-currentP2Index));
            return U;
        }


        // Returns true if the tangent p0p1 is the upper tangent.
        // p0p1 is the tangent, p2 and p3 are the neighbors of p1.
        // p2 is the next point in the upper hull and p2 is the previous point.
        private bool UpperTangent(Point p0, Point p1, Point p2, Point p3)
        {
            return !(STL.Turn(p0, p1, p2) < 0 && STL.Turn(p0, p1, p3) > 0);
        }
    }

    // LowerHull is an Future that finds and returns the
    // lower hull for a sorted set of points. 
    public class LowerHull : Future<List<Point>>
    {
        private List<Point> points;
        private int start, end, grainSize;

        // UpperHull is initialised with the point list, the 
        // start index and the end index.
        public LowerHull(List<Point> points, int start, int end, int grainSize)
        {
            this.points = points;
            this.start = start;
            this.end = end;
            this.grainSize = grainSize;
        }


        // If the number of points is less than the grainSize, the Run method
        // simply finds the lower hull by going through all points from right to
        // left, as described in the sequential convex hull algorithm.
        // Otherwise we split the points in two and find the lower hulls
        // concurrently. 
        // Finally we merge the two lower hulls and return the lower hull.
        public override List<Point> Run()
        {
            int length = end - start + 1;

            if (length < grainSize)
            {
                List<Point> L = new List<Point>(length);

                int j = end;
                while (j > end-2 && j >= start)
                {
                    L.Add(points[j]);
                    j--;
                }
                for (int i = end - 2; i >= start; i--)
                {
                    while (L.Count > 1 && STL.Turn(L[L.Count - 2], L[L.Count - 1], points[i]) <= 0)
                    {
                        L.RemoveAt(L.Count - 1);
                    }
                    L.Add(points[i]);
                }
                return L;
            }
            else
            {
                int middle = (length / 2) + start;
                List<Point>[] res = Parallel.Gather(
                                            new LowerHull(points, start, middle, grainSize),
                                            new LowerHull(points, middle + 1, end, grainSize));

                return MergeLowerHulls(res[0], res[1]);
            }
        }

        // MergeLowerHulls merge two lower hulls in O(n) time.
        private List<Point> MergeLowerHulls(List<Point> P1, List<Point> P2)
        {
            int lengthP1 = P1.Count;
            int lengthP2 = P2.Count;
            int currentP1Index = 0;
            int currentP2Index = lengthP2 - 1;
            bool done = false;

            while (!done)
            {
                done = true;
                if (currentP2Index > 0 && STL.Turn(P1[currentP1Index], P2[currentP2Index], P2[currentP2Index - 1]) > 0)
                {
                    done = false;
                    currentP2Index--;
                    while (currentP2Index > 0 && !LowerTangent(P1[currentP1Index], P2[currentP2Index], P2[currentP2Index - 1], P2[currentP2Index + 1]))
                    {
                        currentP2Index--;
                    }
                }

                if (currentP1Index < lengthP1 - 1 && STL.Turn(P2[currentP2Index], P1[currentP1Index], P1[currentP1Index + 1]) < 0)
                {
                    done = false;
                    currentP1Index++;
                    while (currentP1Index < lengthP1 - 1 && !LowerTangent(P2[currentP2Index], P1[currentP1Index], P1[currentP1Index + 1], P1[currentP1Index - 1]))
                    {
                        currentP1Index++;
                    }
                }
            }

            List<Point> L = new List<Point>();
            L.AddRange(P2.GetRange(0, currentP2Index + 1));
            L.AddRange(P1.GetRange(currentP1Index, lengthP1-currentP1Index));
            return L;
        }


        // Returns true if the tangent p0p1 is the lower tangent.
        // p0p1 is the tangent, p2 and p3 are the neighbors of p1.
        // p2 is the next point in the lower hull and p2 is the previous point.
        private bool LowerTangent(Point p0, Point p1, Point p2, Point p3)
        {
            return !(STL.Turn(p0, p1, p2) < 0 && STL.Turn(p0, p1, p3) > 0);
        }
    }
}
