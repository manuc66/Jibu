using System;
using System.Collections.Generic;
using System.Text;

namespace GameOfLife
{
    public class Point
    {
        public int x, y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    /**************************************
     * Acts as a LifeForm factory. Can
     * create three different life forms.
     **************************************/	
    public abstract class LifeForm
    {
        protected Point offset;
        protected Point[] points;


        public LifeForm(int x, int y)
        {
            offset = new Point(x, y);
        }

        public Point[] Points
        {
            get
            {
                return points;
            }
        }

        public Point Offset
        {
            get
            {
                return offset;
            }
        }

        public static LifeForm CreatePulsar(int xOffset, int yOffset)
        {
            return new Pulsar(xOffset, yOffset);
        }

        public static LifeForm CreateGlider(int xOffset, int yOffset)
        {
            return new Glider(xOffset, yOffset);
        }

        public static LifeForm CreateLightweightSpaceship(int xOffset, int yOffset)
        {
            return new LightweightSpaceship(xOffset, yOffset);
        }

    }

    class Glider : LifeForm
    {
        public Glider(int x, int y)
            : base(x, y)
        {
            points = new Point[] 
                {
                    new Point(1,0),
				    new Point(1,1), new Point(2,1),
    			    new Point(0,2), new Point(2,2)			
	        	};
        }
    }

    class LightweightSpaceship : LifeForm
    {
        public LightweightSpaceship(int x, int y)
            : base(x, y)
        {
            points = new Point[] 
                {
                    new Point(1,0),new Point(4,0),
				    new Point(0,1), 
				    new Point(0,2), new Point(4,2),
				    new Point(0,3), new Point(1,3),new Point(2,3),new Point(3,3)
			    };
        }
    }

    class Pulsar : LifeForm
    {
        public Pulsar(int x, int y)
            : base(x, y)
        {
            points = new Point[] 
                {
                    new Point(1,0), 
				    new Point(0,1),new Point(1,1), new Point(2,1),
				    new Point(0,2), new Point(2,2),
				    new Point(0,3),new Point(1,3), new Point(2,3),
				    new Point(1,4) 
                };
        }
    }

}
