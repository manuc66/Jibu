using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MandelbrotFractal
{
    // A form that displays a MandelbrotPanel and
    // a control panel that allows zooming and other
    // stuff. 
    // NOTE: Don't run this application with debugging!
    // It is very slow because of a bug related to 
    // managed debugging assistants and the Bitmap-class.
    public partial class MandelbrotForm : Form
    {
        private double xMin, xMax, yMin, yMax;
        private int iterations;

        public MandelbrotForm()
        {
            InitializeComponent();
        }

        private void MandelbrotForm_Load(object sender, EventArgs e)
        {
            // we draw the fractal using the default values
            btDrawMt_Click(null, null);
        }

        // Draws the fractal on the panel
        private void DrawFractal()
        {
            long timeSpent;                       

            if (rbMultiThreaded.Checked)
            {
                timeSpent = panFractal.MultiThreadedDraw(xMin, xMax, yMin, yMax, iterations);
            }
            else
            {
                timeSpent = panFractal.SingleThreadedDraw(xMin, xMax, yMin, yMax, iterations);
            }

            labCalcTime.Text = "Calculated in " + timeSpent + " ms.";
        }

        // Triggers when the "Draw fractal" button is clicked
        private void btDrawMt_Click(object sender, EventArgs e)
        {
            // validate user input
            if (!double.TryParse(tbXmin.Text, out xMin) ||
                !double.TryParse(tbXmax.Text, out xMax) ||
                !double.TryParse(tbYmin.Text, out yMin) ||
                !double.TryParse(tbYmax.Text, out yMax) ||
                !int.TryParse(tbIters.Text, out iterations) || iterations < 1)
            {
                MessageBox.Show("You must enter a valid number in all textboxes!");
                return;
            }

            if (!(xMin < xMax && yMin < yMax))
            {
                MessageBox.Show("Coordinates are not valid!");
                return;
            }
            
            // draw fractal
            DrawFractal();
        }

        // Triggers when the fractal is clicked, which
        // results in a zoom.
        private void panFractal_MouseClick(object sender, MouseEventArgs e)
        {
            // validate user input
            double zoomFactor;

            if (!double.TryParse(tbZoomFactor.Text, out zoomFactor))
            {
                MessageBox.Show("Zoom factor is not valid!");
                return;
            }
            
            if (!int.TryParse(tbIters.Text, out iterations) || iterations < 1)
            {
                MessageBox.Show("The number of iterations is not valid!");
                return;
            }

            double mandWidth = xMax - xMin;
            double mandHeight = yMax - yMin;

            // convert clicked point to a point in our coordinate system
            double mandCenterX = xMin + (mandWidth * (e.X / (double)panFractal.Width));
            double mandCenterY = yMin + (mandHeight * (e.Y / (double)panFractal.Height));

            // calculate the new bounds for our coordinate system
            double mandDeltaX = (.5 * mandWidth / zoomFactor);
            double mandDeltaY = (.5 * mandHeight / zoomFactor);
                        
            xMin = mandCenterX - mandDeltaX;
            xMax = mandCenterX + mandDeltaX;
            yMin = mandCenterY - mandDeltaY;
            yMax = mandCenterY + mandDeltaY;

            // draw zoomed fractal
            DrawFractal();

            // set the new bounds on the GUI
            tbXmin.Text = xMin.ToString();
            tbXmax.Text = xMax.ToString();
            tbYmin.Text = yMin.ToString();
            tbYmax.Text = yMax.ToString();
        }




    }
}