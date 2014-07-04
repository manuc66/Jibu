using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GameOfLife
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // The size of the world
            int columns = 150;
            int rows = 150;

            // Each cell will have width and length = 2 * cellPadding 
            int cellPadding = 2;

            // The number of LifeForms to begin with
            int numLifeForms = 100;

            // Set this to false to run the 
            // game sequentially.
            // If the game is run on a computer
            // with multiple cores, then the
            // speed difference should obvious.
            bool parallel = true;

            LifeForm[] lifeForms = new LifeForm[numLifeForms];
            Random rand = new Random();

            // Generate some life forms that
            // will be placed randomly
            for (int i = 0; i < numLifeForms; i++)
            {
                int x = rand.Next(columns);
                int y = rand.Next(rows);

                if (i < numLifeForms / 3)
                    lifeForms[i] = LifeForm.CreateGlider(x, y);
                else if (i < numLifeForms / 2)
                    lifeForms[i] = LifeForm.CreatePulsar(x, y);
                else
                    lifeForms[i] = LifeForm.CreateLightweightSpaceship(x, y);

            }

            World w = new World(columns, rows);
            w.AddLifeForms(lifeForms);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameOfLife(new GamePanel(w, cellPadding,parallel)));



            // For a text-based version uncomment the loop below.
            /* 
             
            for (int i = 0; i < 10; i++)
            {
                w.ParallelTransition();
                w.ConsoleDraw();
            }
             */

        }
    }
}