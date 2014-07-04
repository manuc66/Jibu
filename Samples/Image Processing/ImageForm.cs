using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace ImageProcessing
{
    class ImageForm : Form
    {
        public ImageForm(Panel panel)
        {
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            Text = "Image Processing";
            AutoSize = true;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.Fixed3D;            
            Controls.Add(panel);

        }
    }

    class ImagePanel : Panel
    {
        private Bitmap bmp;

        public ImagePanel(Bitmap bmp)
        {
            Size = bmp.Size;
            this.bmp = bmp;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(bmp, 0, 0);
        }
    }
}
