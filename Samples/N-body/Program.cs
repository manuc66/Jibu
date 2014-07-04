using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace NBody
{
    static class Program
    {

        [STAThread]
        static void Main()
        {
            //  We simulate some of the planets in our solar system		 
            Body[] bodies = new Body[]{ 
				new Body(1.496e11, 0, 0, 2.980e04, 5.974e24,Color.Blue),	// Earth
				new Body(2.279e11, 0, 0, 2.410e04, 6.419e23,Color.Red),		// Mars
				new Body(5.790e10, 0, 0, 4.790e04, 3.302e23, Color.Gray),	// Mercury				
				new Body(1.082e11, 0, 0, 3.500e04, 4.869e24, Color.Yellow),	// Venus
				new Body(0, 0, 0, 0, 1.989e30, Color.Orange)				// Sun
				};

            // Comment out the section below to generate
            // some random bodies instead of the solar system.
            /*
            int N = 1000;
            Body[] bodies = NBodySolver.GenerateRandomBodies(N);	*/

            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new NBodyForm(new NbodyPanel(800, 800, -2.5e11f, 2.5e11f, -2.5e11f, 2.5e11f, 100000, bodies, true)));


            // Comment out the line below to run a console based version
            // of the N-body problem. Remember to change the "Output type"
            // of the project to "Console application"

            // NBodyConsole();
        }

        private static void NBodyConsole()
        {
            int N = 10000;
            int steps = 1;
            double dt = 100000;

            Body[] bodies = NBodySolver.GenerateRandomBodies(N);


            // To make a fair comparison, we call
            // each method once, to let the JIT kick in
            NBodySolver.ParallelCalcForceAndUpdate(bodies, dt);
            NBodySolver.SequentialCalcForceAndUpdate(bodies, dt);


            Console.WriteLine("Starting parallel N-body solver...");
            long start = DateTime.Now.Ticks / 10000;
            for (int i = 0; i < steps; i++)
            {
                NBodySolver.ParallelCalcForceAndUpdate(bodies, dt);
            }
            long end = DateTime.Now.Ticks / 10000;
            long tPar = end - start;
            Console.WriteLine("Time spent: " + tPar);


            Console.WriteLine("Starting sequential N-body solver...");
            start = DateTime.Now.Ticks / 10000;
            for (int i = 0; i < steps; i++)
            {
                NBodySolver.SequentialCalcForceAndUpdate(bodies, dt);
            }
            end = DateTime.Now.Ticks / 10000;
            long tSeq = end - start;

            Console.WriteLine("Time spent: " + tSeq);
            Console.WriteLine("Speed up: " + ((double)tSeq / tPar));
        }

    }
}