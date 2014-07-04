using System;
using System.Collections.Generic;
using System.Text;
using Jibu;
using System.Drawing;

namespace ImageProcessing
{

    public class Pipeline : Future<Bitmap>
    {
        private Filter[] filters;
        private Bitmap image;
        private int strips;

        public Pipeline(Bitmap image, Filter[] filters, int strips)
        {
            this.filters = filters;
            this.image = image;
            this.strips = strips;
        }

        public override Bitmap Run()
        {
            int width = image.Width;
            int height = image.Height;

            int stripHeight = height / strips;
            int lastStripHeight = stripHeight + height % strips;

            Async[] stages = new PipelineStage[filters.Length];

            Address next = Address;

            for (int i = filters.Length - 1; i >= 0; i--)
            {
                Async stage = new PipelineStage(filters[i], strips, next).Start();
                stages[i] = stage;
                next = stage.Address;
            }

            Address first = stages[0].Address;

            int[] pixels = ImageProcessing.GetPixels(image);
            int[] resultPixels = new int[width * height];

            for (int i = 0; i < strips - 1; i++)
            {
                int yStart = i * stripHeight;
                int yEnd = yStart + stripHeight;
                Send(new PipelineJob(pixels, resultPixels, width, height, yStart, yEnd), first);
            }

            int yStartLast = (strips - 1) * stripHeight;
            int yEndLast = yStartLast + lastStripHeight;
            Send(new PipelineJob(pixels, resultPixels, width, height, yStartLast, yEndLast), first);

            PipelineJob job  =null;

            for (int i = 0; i < strips; i++)
            {
                job = Receive<PipelineJob>();
            }

            Bitmap result = new Bitmap(width, height);
            ImageProcessing.SetPixels(result, job.srcImage);
            return result;

        }

        /********************************************
         * This class describes the area to be worked
         * upon in a single pipeline stage.
         ********************************************/
        private class PipelineJob
        {
            public int[] srcImage;
            public int[] resultImage;
            public int yStart, yEnd, width, height;

            public PipelineJob(int[] image, int[] resultImage, int width, int height, int yStart, int yEnd)
            {
                this.srcImage = image;
                this.resultImage = resultImage;
                this.width = width;
                this.height = height;
                this.yStart = yStart;
                this.yEnd = yEnd;
            }

            /******************************
             * Creates a copy of this job, 
             * but swaps the image arrays
             ******************************/
            public PipelineJob Copy()
            {
                return new PipelineJob(resultImage, srcImage, width, height, yStart, yEnd);
            }
        }

        /********************************************
         * A single stage in the pipeline.
         ********************************************/
        private class PipelineStage : Async
        {
            private int numChunks;
            private Filter filter;
            private Address nextStage;

            public PipelineStage(Filter filter, int numChunks, Address nextStage)
            {
                this.numChunks = numChunks;
                this.filter = filter;
                this.nextStage = nextStage;
            }


            public override void Run()
            {
                // get first job
                PipelineJob job = Receive<PipelineJob>();

                PipelineJob nextJob = null;

                int receivedChunks = 1;


                while (receivedChunks <= numChunks)
                {
                    if (receivedChunks < numChunks)
                    {
                        // process all but last line
                        ImageProcessing.Process(filter, job.srcImage, job.resultImage, job.width, job.height, job.yStart, job.yEnd - 1);

                        // receive next job				
                        nextJob = Receive<PipelineJob>();

                        // process last line
                        ImageProcessing.Process(filter, job.srcImage, job.resultImage, job.width, job.height, job.yEnd - 1, job.yEnd);
                    }
                    else // this is the last iteration
                    {
                        // process entire chunk
                        ImageProcessing.Process(filter, job.srcImage, job.resultImage, job.width, job.height, job.yStart, job.yEnd);
                    }

                    receivedChunks++;
                    // send result for this iteration to next stage
                    Send(job.Copy(), nextStage);
                    job = nextJob;
                }
            }
        }


    }
}
