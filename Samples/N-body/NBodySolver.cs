using System;
using System.Collections.Generic;
using System.Text;
using Jibu;
using System.Drawing;

namespace NBody
{
    class NBodySolver
    {
        /************************ Parallel methods  ****************************/

        public static void ParallelCalcForceAndUpdate(Body[] bodies, double dt)
        {
            Parallel.For(0, bodies.Length, 100, delegate(int i)
            {
                bodies[i].Reset();
                for (int j = 0; j < bodies.Length; j++)
                    if (i != j)
                        bodies[i].CalcForce(bodies[j]);
            });

            Parallel.For(0, bodies.Length, 100, delegate(int i)
            {
                bodies[i].Update(dt);

            });

        }


        /************************ Sequential methods  ****************************/

        public static void SequentialCalcForceAndUpdate(Body[] bodies, double dt)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                bodies[i].Reset();
                for (int j = 0; j < bodies.Length; j++)
                    if (i != j)
                        bodies[i].CalcForce(bodies[j]);
            }

            for (int i = 0; i < bodies.Length; i++)
            {
                bodies[i].Update(dt);
            }
        }

        public static void SequentialDraw(Body[] bodies, Graphics g, float planetRadius)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                bodies[i].Paint(g, planetRadius);
            }
        }

        /************************ Generate random bodies  ****************************/

        private static double RandomDouble(Random rand, double limit)
        {
            return (rand.NextDouble() * 2 * limit) - limit;
        }

        public static Body[] GenerateRandomBodies(int N)
        {
            Random rand = new Random();
            Body[] bodies = new Body[N];

            // Generate some random bodies:		
            for (int i = 0; i < N - 1; i++)
            {
                bodies[i] = new Body(RandomDouble(rand, 2.5E11),
                                    RandomDouble(rand, 2.5E11),
                                    RandomDouble(rand, 4e4),
                                    RandomDouble(rand, 4e4),
                                    RandomDouble(rand, 1E25), Color.Green);
            }

            // And one in the middle, that is much heavier than the rest
            bodies[N - 1] = new Body(0, 0, 0, 0, 1.989e30, Color.Orange);

            return bodies;
        }
    }
}
