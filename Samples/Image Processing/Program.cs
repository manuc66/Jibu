using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace ImageProcessing
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // You probably have to change the path to point to the supplied
            // image "river.jpg".
            // Because of a bug in Visual studio this project should not be run
            // inside the debugger. It will be extremely slow.
            Bitmap image = new Bitmap(@"river.jpg");

            // Filters can be added and removed here.
            Filter[] filters = new Filter[] { Filter.MEDIAN_FILTER, Filter.MEAN_FILTER, Filter.EDGE_DETECTION };

            // Do the processing using the pipeline
            // Note that the image will be converted to gray scale.
            Bitmap result = ImageProcessing.ApplyFiltersPipelined(image, filters, 8);

            // ... or use the sequential method
            // Bitmap result = ImageProcessing.ApplyFilters(image, filters);

            // Show the resulting image           
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ImageForm(new ImagePanel(result)));



            // For a text based version uncomment the line below.
            // Remember to change the "Output type" of the project
            // to "Console application"

            // ImageProcessingConsole(image);

        }

        private static void ImageProcessingConsole(Bitmap image)
        {


            // we apply a filter 50 times
            int numFilters = 50;

            // in the pipelined version the image
            // is divided into 30 strips
            int strips = 30;

            Filter[] filters = new Filter[numFilters];

            for (int i = 0; i < numFilters; i++)
            {
                filters[i] = Filter.MEDIAN_FILTER;
            }

            // Let the JIT compilation kick in
            ImageProcessing.ApplyFilters(image, filters);
            ImageProcessing.ApplyFiltersPipelined(image, filters, strips);


            long start = DateTime.Now.Ticks;
            ImageProcessing.ApplyFilters(image, filters);
            long end = DateTime.Now.Ticks;
            long tSeq = (end - start) / 10000;
            Console.WriteLine("Time spent " + tSeq);

            start = System.DateTime.Now.Ticks;
            ImageProcessing.ApplyFiltersPipelined(image, filters, strips);
            end = System.DateTime.Now.Ticks;
            long tPar = (end - start) / 10000;
            Console.WriteLine("Time spent " + tPar);

            Console.WriteLine("Speed up " + (double)tSeq / tPar);
        }
    }
}