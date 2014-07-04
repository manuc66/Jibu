using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Jibu;

namespace MandelbrotFractal
{

    // A control that can calculate and
    // display the Mandebrot set.
    // NOTE: Don't run this application with debugging!
    // It is very slow because of a bug related to 
    // managed debugging assistants and the Bitmap-class.
    public partial class MandelbrotPanel : UserControl
    {
        private int width, height;
        private Bitmap bmp;
        private Color[,] image;

        public MandelbrotPanel()
        {
            InitializeComponent();

            // Use double-buffering
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);

        }

        private void InitImage()
        {
            width = this.Width;
            height = this.Height;
            image = new Color[width, height];
            bmp = new Bitmap(width, height);
        }

        private void DrawImage()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bmp.SetPixel(x, y, image[x, y]);
                }
            }

            Refresh();
        }

        // Draws the Mandelbrot set using a single thread.
        // The time spent calculating all points is returned.
        // The image-drawing time is left out, because this can only
        // be done in a single thread.
        public long SingleThreadedDraw(double xMin, double xMax, double yMin, double yMax, int iterations)
        {
            long total, start;
            double mandWidth = xMax - xMin;
            double mandHeight = yMax - yMin;

            start = DateTime.Now.Ticks;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double xCoord = xMin + ((mandWidth / width) * x);
                    double yCoord = yMin + ((mandHeight / height) * y);

                    image[x, y] = MandelbrotMethods.MapToColor(MandelbrotMethods.SolvePoint(xCoord, yCoord,iterations));
                }
            }
            total = DateTime.Now.Ticks - start;
            
            DrawImage();
            
            return total / 10000;
        }

        // Draws the Mandelbrot set using the parallel for loop.
        // The time spent calculating all points is returned.
        // The image-drawing time is left out, because this can only
        // be done in a single thread.
        // Notice how this method resembles the sequential version.
        // The only change is a parallel outer for loop.
        public long MultiThreadedDraw(double xMin, double xMax, double yMin, double yMax, int iterations)
        {
            long total, start;
            double mandWidth = xMax - xMin;
            double mandHeight = yMax - yMin;

            start = DateTime.Now.Ticks;

            Parallel.For(0, width, 1, delegate(int x)
            {
                for (int y = 0; y < height; y++)
                {
                    double xCoord = xMin + ((mandWidth / width) * x);
                    double yCoord = yMin + ((mandHeight / height) * y);

                    image[x, y] = MandelbrotMethods.MapToColor(MandelbrotMethods.SolvePoint(xCoord, yCoord, iterations));
                }
            });
            
            total = DateTime.Now.Ticks - start;

            DrawImage();

            return total / 10000;            
        }
        
        protected override void OnLoad(EventArgs e)
        {
            InitImage();
            base.OnLoad(e);
        }
                
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(bmp, 0, 0);
        }

    }

    // Contains static methods for solving
    // the Mandelbrot set.
    class MandelbrotMethods
    {
        // Solves a single point in the Mandelbrot set
        // and returns the number of iterations spent.
        // The iterations parameter specifies the maximum
        // number of iterations.
        public static int SolvePoint(double x, double y, int iterations)
        {
            double r = 0.0, s = 0.0;
            double next_r, next_s;
            int iteration = 0;

            while ((r * r + s * s) <= 4.0)
            {
                next_r = r * r - s * s + x;
                next_s = 2 * r * s + y;
                r = next_r; s = next_s;
                if (++iteration == iterations)
                    break;
            }

            return iteration;
        }

        // Maps an iteration number to a color.
        // The colors can be tweaked by fiddling 
        // with this method.
        public static Color MapToColor(int val)
        {
            int r = (val * 12) % 256;
            int g = (val * 16) % 256;
            int b = (val * 5) % 256;

            return Color.FromArgb(r, g, b);
        }
    }

}
