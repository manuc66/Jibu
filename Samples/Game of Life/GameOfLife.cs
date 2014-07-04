using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GameOfLife
{
    

    public class GameOfLife : Form
    {
        public GameOfLife(Panel panel)
        {
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            Text = "Game of Life";
            AutoSize = true;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.Fixed3D;            
            Controls.Add(panel);

        }
    }


    public class GamePanel : Panel
    {
        private Bitmap bmp;

        public GamePanel(World world, int cellPadding, bool parallel)
        {         
            int cellSize = 2 * cellPadding;
            int width = world.Width * cellSize;
            int height = world.Height * cellSize;
            bmp = new Bitmap(width, height);
            Size = bmp.Size;
            

            Timer timer = new Timer();
            timer.Interval = 1;

            timer.Tick += delegate(Object sender, EventArgs e)
            {
                if (parallel)
                {
                    world.ParallelTransition();
                }
                else
                {
                    world.SequentialTransition();
                }

                world.ImageDraw(Graphics.FromImage(bmp), cellSize);
                Refresh();

                
            };

            timer.Start();
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(bmp, 0, 0);
        }
    }
}