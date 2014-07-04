using System;
using System.Collections.Generic;
using System.Text;

namespace ConvexHull
{
    // Point class
    public class Point : IComparable
    {
        private int x, y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // Points are compared according to their 
        // x-coordinate. The y-coordinate is used
        // if x-coordinates are identical.
        public int CompareTo(Object obj)
        {
            if (obj is Point)
            {
                Point p = (Point)obj;
                if (this.x < p.x)
                    return -1;

                if (this.x == p.x)
                {
                    if (this.y > p.y)
                        return -1;

                    return 0;
                }

                return 1;
            }
            throw new Exception("obj is not a Point");
        }

        public int X
        {
            get { return x; }
        }

        public int Y
        {
            get { return y; }
        }

    }
}
