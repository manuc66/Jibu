using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace NBody
{
    class Body
    {
        private const double G = 6.67E-11;
        private const double EPS = 3E4;
        private double x, y, xVelocity, yVelocity, xForce, yForce, mass;
        private Color color;

        public Body(double x, double y, double xVelocity, double yVelocity, double mass, Color color)
        {
            this.x = x;
            this.y = y;
            this.xVelocity = xVelocity;
            this.yVelocity = yVelocity;
            this.mass = mass;
            this.color = color;
        }

        public void Reset()
        {
            xForce = 0;
            yForce = 0;
        }

        /****************************
         * Calculate the effect of 
         * the Body b. This Body won't
         * be moved until the update 
         * method is called.
         ****************************/
        public void CalcForce(Body b)
        {
            double dx = b.x - x;
            double dy = b.y - y;

            double r = Math.Sqrt(dx * dx + dy * dy);

            double F = (G * mass * b.mass) / (r * r + EPS * EPS);

            xForce += F * dx / r;
            yForce += F * dy / r;
        }


        /****************************
         * Moves this body according
         * to the effects of the other
         * bodies.
         ****************************/
        public void Update(double dt)
        {
            xVelocity += dt * xForce / mass;
            yVelocity += dt * yForce / mass;
            x += dt * xVelocity;
            y += dt * yVelocity;
        }

        public void Paint(Graphics g, float radius)
        {
            Brush b = new SolidBrush(color);
            g.FillEllipse(b, (float)x + radius, (float)y + radius, 2 * radius, 2 * radius);
        }
    }
}
