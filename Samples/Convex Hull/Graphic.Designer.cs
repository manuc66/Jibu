namespace ConvexHull
{
    partial class Graphic
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
            this.points = new System.Windows.Forms.Panel();
            this.start = new System.Windows.Forms.Button();
            this.output = new System.Windows.Forms.TextBox();
            this.clear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // points
            // 
            this.points.BackColor = System.Drawing.Color.LightGray;
            this.points.Location = new System.Drawing.Point(12, 12);
            this.points.Name = "points";
            this.points.Size = new System.Drawing.Size(526, 343);
            this.points.TabIndex = 0;
            this.points.MouseClick += new System.Windows.Forms.MouseEventHandler(this.points_MouseClick);
            this.points.Paint += new System.Windows.Forms.PaintEventHandler(this.points_Paint);
            // 
            // start
            // 
            this.start.Location = new System.Drawing.Point(12, 378);
            this.start.Name = "start";
            this.start.Size = new System.Drawing.Size(87, 23);
            this.start.TabIndex = 1;
            this.start.Text = "Convex Hull";
            this.start.UseVisualStyleBackColor = true;
            this.start.Click += new System.EventHandler(this.start_Click);
            // 
            // output
            // 
            this.output.Location = new System.Drawing.Point(197, 381);
            this.output.Name = "output";
            this.output.ReadOnly = true;
            this.output.Size = new System.Drawing.Size(341, 20);
            this.output.TabIndex = 2;
            this.output.Text = "Use the right mouse button to plot points";
            // 
            // clear
            // 
            this.clear.Location = new System.Drawing.Point(105, 378);
            this.clear.Name = "clear";
            this.clear.Size = new System.Drawing.Size(86, 23);
            this.clear.TabIndex = 3;
            this.clear.Text = "Clear";
            this.clear.UseVisualStyleBackColor = true;
            this.clear.Click += new System.EventHandler(this.clear_Click);
            // 
            // Graphic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 420);
            this.Controls.Add(this.clear);
            this.Controls.Add(this.output);
            this.Controls.Add(this.start);
            this.Controls.Add(this.points);
            this.Name = "Graphic";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel points;
        private System.Windows.Forms.Button start;
        private System.Windows.Forms.TextBox output;
        private System.Windows.Forms.Button clear;
    }
}

