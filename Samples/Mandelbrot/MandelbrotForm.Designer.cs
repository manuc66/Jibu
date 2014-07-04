namespace MandelbrotFractal
{
    partial class MandelbrotForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btDrawMt = new System.Windows.Forms.Button();
            this.panFractal = new MandelbrotFractal.MandelbrotPanel();
            this.rbMultiThreaded = new System.Windows.Forms.RadioButton();
            this.rbSingleThreaded = new System.Windows.Forms.RadioButton();
            this.tbZoomFactor = new System.Windows.Forms.TextBox();
            this.gbControlPanel = new System.Windows.Forms.GroupBox();
            this.labZoomFactor = new System.Windows.Forms.Label();
            this.labZoomExplanation = new System.Windows.Forms.Label();
            this.labXmax = new System.Windows.Forms.Label();
            this.labYmin = new System.Windows.Forms.Label();
            this.labYmax = new System.Windows.Forms.Label();
            this.labXmin = new System.Windows.Forms.Label();
            this.tbXmin = new System.Windows.Forms.TextBox();
            this.tbXmax = new System.Windows.Forms.TextBox();
            this.tbYmin = new System.Windows.Forms.TextBox();
            this.tbYmax = new System.Windows.Forms.TextBox();
            this.labIters = new System.Windows.Forms.Label();
            this.tbIters = new System.Windows.Forms.TextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.labCalcTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.gbControlPanel.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // btDrawMt
            // 
            this.btDrawMt.Location = new System.Drawing.Point(6, 197);
            this.btDrawMt.Name = "btDrawMt";
            this.btDrawMt.Size = new System.Drawing.Size(137, 23);
            this.btDrawMt.TabIndex = 1;
            this.btDrawMt.Text = "Draw Fractal";
            this.btDrawMt.UseVisualStyleBackColor = true;
            this.btDrawMt.Click += new System.EventHandler(this.btDrawMt_Click);
            // 
            // panFractal
            // 
            this.panFractal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panFractal.Location = new System.Drawing.Point(14, 12);
            this.panFractal.Name = "panFractal";
            this.panFractal.Size = new System.Drawing.Size(700, 700);
            this.panFractal.TabIndex = 0;
            this.panFractal.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panFractal_MouseClick);
            // 
            // rbMultiThreaded
            // 
            this.rbMultiThreaded.AutoSize = true;
            this.rbMultiThreaded.Checked = true;
            this.rbMultiThreaded.Location = new System.Drawing.Point(9, 151);
            this.rbMultiThreaded.Name = "rbMultiThreaded";
            this.rbMultiThreaded.Size = new System.Drawing.Size(92, 17);
            this.rbMultiThreaded.TabIndex = 4;
            this.rbMultiThreaded.TabStop = true;
            this.rbMultiThreaded.Text = "Multi threaded";
            this.rbMultiThreaded.UseVisualStyleBackColor = true;
            // 
            // rbSingleThreaded
            // 
            this.rbSingleThreaded.AutoSize = true;
            this.rbSingleThreaded.Location = new System.Drawing.Point(9, 174);
            this.rbSingleThreaded.Name = "rbSingleThreaded";
            this.rbSingleThreaded.Size = new System.Drawing.Size(99, 17);
            this.rbSingleThreaded.TabIndex = 5;
            this.rbSingleThreaded.Text = "Single threaded";
            this.rbSingleThreaded.UseVisualStyleBackColor = true;
            // 
            // tbZoomFactor
            // 
            this.tbZoomFactor.Location = new System.Drawing.Point(86, 248);
            this.tbZoomFactor.Name = "tbZoomFactor";
            this.tbZoomFactor.Size = new System.Drawing.Size(57, 20);
            this.tbZoomFactor.TabIndex = 6;
            this.tbZoomFactor.Text = "2";
            this.tbZoomFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // gbControlPanel
            // 
            this.gbControlPanel.Controls.Add(this.tbIters);
            this.gbControlPanel.Controls.Add(this.labIters);
            this.gbControlPanel.Controls.Add(this.tbYmax);
            this.gbControlPanel.Controls.Add(this.tbYmin);
            this.gbControlPanel.Controls.Add(this.tbXmax);
            this.gbControlPanel.Controls.Add(this.tbXmin);
            this.gbControlPanel.Controls.Add(this.labXmin);
            this.gbControlPanel.Controls.Add(this.labYmax);
            this.gbControlPanel.Controls.Add(this.labYmin);
            this.gbControlPanel.Controls.Add(this.labXmax);
            this.gbControlPanel.Controls.Add(this.labZoomExplanation);
            this.gbControlPanel.Controls.Add(this.labZoomFactor);
            this.gbControlPanel.Controls.Add(this.rbMultiThreaded);
            this.gbControlPanel.Controls.Add(this.tbZoomFactor);
            this.gbControlPanel.Controls.Add(this.btDrawMt);
            this.gbControlPanel.Controls.Add(this.rbSingleThreaded);
            this.gbControlPanel.Location = new System.Drawing.Point(720, 11);
            this.gbControlPanel.Name = "gbControlPanel";
            this.gbControlPanel.Size = new System.Drawing.Size(149, 701);
            this.gbControlPanel.TabIndex = 7;
            this.gbControlPanel.TabStop = false;
            this.gbControlPanel.Text = "Control Panel";
            // 
            // labZoomFactor
            // 
            this.labZoomFactor.AutoSize = true;
            this.labZoomFactor.Location = new System.Drawing.Point(6, 251);
            this.labZoomFactor.Name = "labZoomFactor";
            this.labZoomFactor.Size = new System.Drawing.Size(67, 13);
            this.labZoomFactor.TabIndex = 7;
            this.labZoomFactor.Text = "Zoom factor:";
            // 
            // labZoomExplanation
            // 
            this.labZoomExplanation.AutoSize = true;
            this.labZoomExplanation.Location = new System.Drawing.Point(11, 276);
            this.labZoomExplanation.Name = "labZoomExplanation";
            this.labZoomExplanation.Size = new System.Drawing.Size(126, 13);
            this.labZoomExplanation.TabIndex = 8;
            this.labZoomExplanation.Text = "[Click the fractal to zoom]";
            // 
            // labXmax
            // 
            this.labXmax.AutoSize = true;
            this.labXmax.Location = new System.Drawing.Point(6, 41);
            this.labXmax.Name = "labXmax";
            this.labXmax.Size = new System.Drawing.Size(39, 13);
            this.labXmax.TabIndex = 9;
            this.labXmax.Text = "X max:";
            // 
            // labYmin
            // 
            this.labYmin.AutoSize = true;
            this.labYmin.Location = new System.Drawing.Point(6, 67);
            this.labYmin.Name = "labYmin";
            this.labYmin.Size = new System.Drawing.Size(36, 13);
            this.labYmin.TabIndex = 10;
            this.labYmin.Text = "Y min:";
            // 
            // labYmax
            // 
            this.labYmax.AutoSize = true;
            this.labYmax.Location = new System.Drawing.Point(6, 93);
            this.labYmax.Name = "labYmax";
            this.labYmax.Size = new System.Drawing.Size(39, 13);
            this.labYmax.TabIndex = 11;
            this.labYmax.Text = "Y max:";
            // 
            // labXmin
            // 
            this.labXmin.AutoSize = true;
            this.labXmin.Location = new System.Drawing.Point(6, 16);
            this.labXmin.Name = "labXmin";
            this.labXmin.Size = new System.Drawing.Size(36, 13);
            this.labXmin.TabIndex = 12;
            this.labXmin.Text = "X min:";
            // 
            // tbXmin
            // 
            this.tbXmin.Location = new System.Drawing.Point(86, 13);
            this.tbXmin.Name = "tbXmin";
            this.tbXmin.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.tbXmin.Size = new System.Drawing.Size(57, 20);
            this.tbXmin.TabIndex = 13;
            this.tbXmin.Text = "-2";
            this.tbXmin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // tbXmax
            // 
            this.tbXmax.Location = new System.Drawing.Point(86, 38);
            this.tbXmax.Name = "tbXmax";
            this.tbXmax.Size = new System.Drawing.Size(57, 20);
            this.tbXmax.TabIndex = 14;
            this.tbXmax.Text = "2";
            this.tbXmax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // tbYmin
            // 
            this.tbYmin.Location = new System.Drawing.Point(86, 64);
            this.tbYmin.Name = "tbYmin";
            this.tbYmin.Size = new System.Drawing.Size(57, 20);
            this.tbYmin.TabIndex = 15;
            this.tbYmin.Text = "-2";
            this.tbYmin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // tbYmax
            // 
            this.tbYmax.Location = new System.Drawing.Point(86, 90);
            this.tbYmax.Name = "tbYmax";
            this.tbYmax.Size = new System.Drawing.Size(57, 20);
            this.tbYmax.TabIndex = 16;
            this.tbYmax.Text = "2";
            this.tbYmax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labIters
            // 
            this.labIters.AutoSize = true;
            this.labIters.Location = new System.Drawing.Point(6, 119);
            this.labIters.Name = "labIters";
            this.labIters.Size = new System.Drawing.Size(53, 13);
            this.labIters.TabIndex = 17;
            this.labIters.Text = "Iterations:";
            // 
            // tbIters
            // 
            this.tbIters.Location = new System.Drawing.Point(86, 116);
            this.tbIters.Name = "tbIters";
            this.tbIters.Size = new System.Drawing.Size(57, 20);
            this.tbIters.TabIndex = 18;
            this.tbIters.Text = "5000";
            this.tbIters.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labCalcTime});
            this.statusStrip.Location = new System.Drawing.Point(0, 716);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(881, 22);
            this.statusStrip.TabIndex = 8;
            this.statusStrip.Text = "statusStrip1";
            // 
            // labCalcTime
            // 
            this.labCalcTime.Name = "labCalcTime";
            this.labCalcTime.Size = new System.Drawing.Size(0, 17);
            // 
            // MandelbrotForm
            // 
            this.AcceptButton = this.btDrawMt;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(881, 738);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.gbControlPanel);
            this.Controls.Add(this.panFractal);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MandelbrotForm";
            this.Text = "Mandelbrot";
            this.Load += new System.EventHandler(this.MandelbrotForm_Load);
            this.gbControlPanel.ResumeLayout(false);
            this.gbControlPanel.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MandelbrotPanel panFractal;
        private System.Windows.Forms.Button btDrawMt;
        private System.Windows.Forms.RadioButton rbMultiThreaded;
        private System.Windows.Forms.RadioButton rbSingleThreaded;
        private System.Windows.Forms.TextBox tbZoomFactor;
        private System.Windows.Forms.GroupBox gbControlPanel;
        private System.Windows.Forms.TextBox tbYmax;
        private System.Windows.Forms.TextBox tbYmin;
        private System.Windows.Forms.TextBox tbXmax;
        private System.Windows.Forms.TextBox tbXmin;
        private System.Windows.Forms.Label labXmin;
        private System.Windows.Forms.Label labYmax;
        private System.Windows.Forms.Label labYmin;
        private System.Windows.Forms.Label labXmax;
        private System.Windows.Forms.Label labZoomExplanation;
        private System.Windows.Forms.Label labZoomFactor;
        private System.Windows.Forms.TextBox tbIters;
        private System.Windows.Forms.Label labIters;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel labCalcTime;

    }
}

