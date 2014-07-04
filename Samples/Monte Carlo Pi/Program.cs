using System;
using System.Collections.Generic;
using System.Text;
using Jibu;
using System.Threading;

namespace MonteCarloPi
{
    static class MonteCarloPi
    {
        /************************************************** 
         * Sequential estimation of Pi using the Monte Carlo 
         * scheme.
         **************************************************/
        public static double Estimate(int iterations, int randomSeed)
        {
            Random rand = new Random(randomSeed);
            double sum = 0;

            for (int i = 0; i < iterations; i++)
            {
                double xVal = rand.NextDouble();
                sum += Math.Sqrt(1 - xVal * xVal);

            }

            return sum / iterations * 4;
        }

        public static double Estimate(int iterations)
        {
            return Estimate(iterations, DateTime.Now.Millisecond);
        }

        /************************************************** 
         * Parallel estimation of Pi using the Monte Carlo 
         * scheme.
         **************************************************/
        public static double ParallelEstimate(int iterations)
        {
            return new ParallelPiCalculator(iterations).Result();
        }

        /************************************************** 
         * A Future that can estimate Pi in parallel using
         * the Monte Carlo scheme.
         **************************************************/
        private class ParallelPiCalculator : Future<Double>
        {
            private int iterations;
            private int procCount;

            /************************************************** 
             * To simplify the code we just do an integer division
             * to split the iteration number. This means that
             * that we can miss up to procCount-1 iterations. 
             **************************************************/
            public ParallelPiCalculator(int iterations)
            {
                this.procCount = Manager.ProcessorCount;
                this.iterations = iterations / procCount;
            }

            public override Double Run()
            {
                // We use the reduce method to distribute work to 'procCount' tasks
                // and combine the results. 
                // The grainSize parameter is 1 => one task for each iteration.
                double sum = Parallel.Reduce<double>(0, procCount, 0.0, 1,
                    delegate(int i) { return MonteCarloPi.Estimate(iterations, i); },
                    delegate(double op1, double op2) { return op1 + op2; });

                return sum / procCount;
            }
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Pi is: 3.14159265358979323846... according to Wikipedia");

            

            // how many 'darts' do we throw at the circle/square 
            int darts = 1000000000;

            // Let the JIT compilation kick in.
            MonteCarloPi.Estimate(10000000);
            MonteCarloPi.ParallelEstimate(10000000);

            Console.WriteLine("Calculating pi using 1 core");
            long start = DateTime.Now.Ticks;
            double seqResult = MonteCarloPi.Estimate(darts,1);
            long t1 = DateTime.Now.Ticks - start;

            Console.WriteLine(seqResult + " calculated in " + t1 / 1E4 + " ms");

            Console.WriteLine("Calculating pi using "+Manager.ProcessorCount+" core(s)");
            start = DateTime.Now.Ticks;
            double parResult = MonteCarloPi.ParallelEstimate(darts);
            long t2 = DateTime.Now.Ticks - start;
            Console.WriteLine(parResult + " calculated in " + t2 / 1E4 + " ms");

            Console.WriteLine("Speed up: " + (double)t1 / t2);

            Console.Read();
        }
    }
}
