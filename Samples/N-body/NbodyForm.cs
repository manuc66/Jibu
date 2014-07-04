using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace NBody
{
    class NBodyForm : Form
    {
        public NBodyForm(Panel panel)
        {
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            Text = "NBody";
            AutoSize = true;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Controls.Add(panel);
        }
    }

    class NbodyPanel : Panel
    {
        private Bitmap bmp;

        public NbodyPanel(
            int width, // width of the panel 
            int height,  // height of the panel
            float xMin, // the smallest visible x-coordinate
            float xMax, // the largest visible x-coordinate
            float yMin, // the smallest visible y-coordinate
            float yMax, // the largest visible y-coordinate
            float dt, // the time step-size
            Body[] bodies, // the bodies to be shown
            bool parallel // should the calculations be done in parallel
                )
        {
            
            this.Size = new Size(width, height);
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;

            float xScale = (float)(width / (xMax - xMin));
            float yScale = (float)(height / (yMax - yMin));

            float xTranslate = (float)Math.Abs(xMin * xScale);
            float yTranslate = (float)Math.Abs(yMin * yScale);

            float planetRadius = ((xMax - xMin) / width) * 10;


            Timer timer = new Timer();
            timer.Interval = 1;
            
            timer.Tick += delegate(Object sender, EventArgs e)
            {
                Bitmap bmpNew = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(bmpNew);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Transform = new System.Drawing.Drawing2D.Matrix(xScale, 0, 0, yScale, xTranslate, yTranslate);
                                

                if (parallel)
                {
                    NBodySolver.ParallelCalcForceAndUpdate(bodies, dt);
                }
                else
                {
                    NBodySolver.SequentialCalcForceAndUpdate(bodies, dt);
                }

                NBodySolver.SequentialDraw(bodies, g, planetRadius);

                this.bmp = bmpNew;
                Refresh();
            };

            timer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if(bmp != null)
                e.Graphics.DrawImageUnscaled(bmp, 0, 0);
        }
    }
}
