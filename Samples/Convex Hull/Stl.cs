using System;
using System.Collections.Generic;
using System.Text;
using Jibu;

namespace ConvexHull
{
    // This class provides some basic methods.
    public static class STL
    {
        public static void QuickSort<T>(List<T> data, int grainSize)
            where T : IComparable
        {
            QuickSort(data, 0, data.Count - 1, grainSize);
        }


        // This is just a standard parallel divide and conquer quicksort algorithm. 
        // No optimizations are implemented.
        // If the number elements is less than grainSize we use the .NET List.Sort algorithm.
        // Otherwise we split the list in two and handle the parts concurrently.
        // Jibu makes it very easy to implement the parallel version of quicksort - it's almost
        // identical to the sequential version. We simply make the recursive calls inside the
        // Paralle.Run method.
        public static void QuickSort<T>(List<T> data, int start, int end, int grainSize)
            where T : IComparable
        {
            int num = end - start + 1;
            if (num < grainSize || num < 2)
                data.Sort(start, num, null);
            else
            {
                int i = Partition(data, start, end);
                // Parallel.Run executes the submitted tasks, in case two delegates,
                // concurrently. Parallel.Run use the Jibu build in scheduler to run
                // the tasks.
                Parallel.Run(
                    new DelegateAsync(delegate { QuickSort(data, start, i - 1, grainSize); }),
                    new DelegateAsync(delegate { QuickSort(data, i + 1, end, grainSize); })
                    );
            }
        }

        // Returns the 2 dimensional cross product
        public static int CrossProduct2D(Point p0, Point p1)
        {
            return p0.X * p1.Y - p0.Y * p1.X;
        }


        // Returns a positive number if we make a right turn, a negative
        // number if we make a left turn and zero if the points are
        // collinear.
        public static int Turn(Point p0, Point p1, Point p2)
        {
            Point p0p1 = new Point(p1.X - p0.X, p1.Y - p0.Y);
            Point p0p2 = new Point(p2.X - p0.X, p2.Y - p0.Y);

            return CrossProduct2D(p0p1, p0p2);
        }


        // --------------------------------- Auxilary methods --------------------------------------------------


        // Partition makes an in-place division of the elements in data.
        // Elements are sorted according to a pivot element and in this case
        // the last element in data is chosen as the pivot.
        // All elements less than the pivot element are placed before the pivot 
        // and elements greater than or equal to the pivot are place after
        // the pivot. Finally the pivot index is returned.
        private static int Partition<T>(List<T> data, int start, int end)
            where T : IComparable
        {
            int i = start;
            int j = end - 1;
            int pivot = end; // pivot is the last element in the array

            while (true)
            {
                while (data[i].CompareTo(data[pivot]) < 0)
                    i++;

                while (data[pivot].CompareTo(data[j]) <= 0)
                {
                    j--;
                    if (j <= start)
                        break;
                }

                if (i >= j)
                    break;

                swap(data, i, j);
            }

            swap(data, i, pivot);
            return i;
        }


        // Swap data
        private static void swap<T>(List<T> data, int i, int j)
            where T : IComparable
        {
            T temp = data[i];
            data[i] = data[j];
            data[j] = temp;
        }
    }
}
