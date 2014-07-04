using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ConvexHull
{
    public partial class Graphic : Form
    {
        private Pen Mypen;
        private List<ConvexHull.Point> p;
        private List<ConvexHull.Point> solution;
        private ParConvexHull convex;

        public Graphic()
        {
            InitializeComponent();
            Mypen = new Pen(Color.Blue);
            p = new List<ConvexHull.Point>();
            solution = new List<ConvexHull.Point>();
            convex = new ParConvexHull();
        }

        private void points_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                p.Add(new ConvexHull.Point(e.X, e.Y));
                points.Invalidate();
            }
        }

        private void points_Paint(object sender, PaintEventArgs e)
        {
            foreach(ConvexHull.Point point in p)
                e.Graphics.DrawRectangle(Mypen, point.X, point.Y, 2, 2);

            int stop = solution.Count - 1;
            for (int i = 0; i < stop; i++)
                e.Graphics.DrawLine(Mypen, solution[i].X, solution[i].Y, solution[i + 1].X, solution[i + 1].Y);

            if(solution.Count > 0)
                e.Graphics.DrawLine(Mypen, solution[stop].X, solution[stop].Y, solution[0].X, solution[0].Y);
        }


        private void start_Click(object sender, EventArgs e)
        {
            if (p.Count < 1)
                return;

            this.output.Text = "Finding Convex hull ...";
            solution = convex.ConvexHull(p);
            points.Invalidate();
            this.output.Text = "Done";
        }

        private void clear_Click(object sender, EventArgs e)
        {
            p.Clear();
            solution.Clear();
            points.Invalidate();
            this.output.Text = "Use the right mouse button to plot points";
        }

    }
}