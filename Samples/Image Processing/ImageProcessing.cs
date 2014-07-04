using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace ImageProcessing
{
    public enum Filter { EDGE_DETECTION, MEDIAN_FILTER, MEAN_FILTER }

    public class ImageProcessing
    {
        /************************************************
         * Converts a Bitmap to gray-scale. 
         ************************************************/
        public static Bitmap ConvertToGrayScale(Bitmap src)
        {

            Bitmap grayImg = new Bitmap(src.Width, src.Height);

            for (int y = 0; y < grayImg.Height; y++)
            {
                for (int x = 0; x < grayImg.Width; x++)
                {
                    Color color = src.GetPixel(x, y);
                    int intensity = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    grayImg.SetPixel(x, y, Color.FromArgb(intensity, intensity, intensity));
                }
            }

            return grayImg;
        }


        /************************************************
         * Returns the pixels of a Bitmap as an integer
         * array. The image is converted to gray scale.
         ************************************************/
        public static int[] GetPixels(Bitmap src)
        {
            int[] pixels = new int[src.Width * src.Height];

            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    Color color = src.GetPixel(x, y);
                    int gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    pixels[x + y * src.Width] = gray;
                }
            }

            return pixels;
        }

        /************************************************
         * Sets the pixels of a Bitmap provided an array 
         * of gray scale values.
         ************************************************/
        public static void SetPixels(Bitmap dest, int[] pixels)
        {
            for (int y = 0; y < dest.Height; y++)
            {
                for (int x = 0; x < dest.Width; x++)
                {
                    int val = pixels[x + y * dest.Width];
                    dest.SetPixel(x, y, Color.FromArgb(val,val,val));
                }
            }
        }

        /************************************************
         * Detects edges in a gray-scale image, with the
         * pixels supplied as a one-dimensional integer
         * array.
         ************************************************/
        public static void EdgeDetection(int[] srcPixels, int[] resultPixels, int width, int height, int yStart, int yEnd)
        {

            if (yStart == 0)
                yStart = 1;
            if (yEnd == height)
                yEnd = height - 1;

            for (int y = yStart; y < yEnd; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {

                    // Gx = (x2 + 2*x5 + x8) - (x0 + 2*x3 + x6)
                    int Gx = (srcPixels[x + 1 + (width * (y - 1))] + 2 * srcPixels[x + 1 + width * y] + srcPixels[x + 1 + width * (y + 1)]) -
                    (srcPixels[x - 1 + width * (y - 1)] + 2 * srcPixels[x - 1 + width * y] + srcPixels[x - 1 + width * (y + 1)]);

                    // Gy = (x6 + 2*x7 + x8) - (x0 + 2*x1 + x2)		
                    int Gy = (srcPixels[x - 1 + (width * (y + 1))] + 2 * srcPixels[x + width * (y + 1)] + srcPixels[x + 1 + width * (y + 1)]) -
                    (srcPixels[x - 1 + width * (y - 1)] + 2 * srcPixels[x + width * (y - 1)] + srcPixels[x + 1 + width * (y - 1)]);

                    int sum = Math.Abs(Gx) + Math.Abs(Gy);

                    if (sum > 255)
                        sum = 255;

                    resultPixels[x + y * width] = sum;
                }
            }
        }

        /************************************************
         * Applies the median filter to an image, where
         * the pixels are supplied in a one-dimensional 
         * integer array.
         ************************************************/
        public static void MedianFilter(int[] srcPixels, int[] resultPixels, int width, int height, int yStart, int yEnd)
        {
            if (yStart == 0)
                yStart = 1;
            if (yEnd == height)
                yEnd = height - 1;

            int[] neighbors = new int[9];

            for (int y = yStart; y < yEnd; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    neighbors[0] = srcPixels[x - 1 + (y - 1) * width];
                    neighbors[1] = srcPixels[x + (y - 1) * width];
                    neighbors[2] = srcPixels[x + 1 + (y - 1) * width];
                    neighbors[3] = srcPixels[x - 1 + y * width];
                    neighbors[4] = srcPixels[x + y * width];
                    neighbors[5] = srcPixels[x + 1 + y * width];
                    neighbors[6] = srcPixels[x - 1 + (y + 1) * width];
                    neighbors[7] = srcPixels[x + (y + 1) * width];
                    neighbors[8] = srcPixels[x + 1 + (y + 1) * width];

                    // sort the neighbors
                    Array.Sort(neighbors);

                    // 4 is the middle element
                    resultPixels[x + y * width] = neighbors[4];

                }
            }
        }

        /************************************************
         * Applies the mean filter to an image, where
         * the pixels are supplied in a one-dimensional 
         * integer array.
         ************************************************/
        public static void MeanFilter(int[] srcPixels, int[] resultPixels, int width, int height, int yStart, int yEnd)
        {
            if (yStart == 0)
                yStart = 1;
            if (yEnd == height)
                yEnd = height - 1;

            for (int y = yStart; y < yEnd; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int sum = srcPixels[x - 1 + (y - 1) * width] + srcPixels[x + (y - 1) * width] + srcPixels[x + 1 + (y - 1) * width] +
                    srcPixels[x - 1 + y * width] + srcPixels[x + y * width] + srcPixels[x + 1 + y * width] +
                    srcPixels[x - 1 + (y + 1) * width] + srcPixels[x + (y + 1) * width] + srcPixels[x + 1 + (y + 1) * width];

                    resultPixels[x + y * width] = sum / 9;

                }
            }
        }

        /************************************************
         * Convenience method, that applies a filter to an
         * image.
         ************************************************/
        public static void Process(Filter filter, int[] srcPixels, int[] resultPixels, int width, int height, int yStart, int yEnd)
        {
            switch (filter)
            {
                case Filter.EDGE_DETECTION:
                    ImageProcessing.EdgeDetection(srcPixels, resultPixels, width, height, yStart, yEnd);
                    break;
                case Filter.MEDIAN_FILTER:
                    ImageProcessing.MedianFilter(srcPixels, resultPixels, width, height, yStart, yEnd);
                    break;
                case Filter.MEAN_FILTER:
                    ImageProcessing.MeanFilter(srcPixels, resultPixels, width, height, yStart, yEnd);
                    break;
            }
        }

        /************************************************
         * Uses the Pipeline Future to apply an array of 
         * filters to an image. 
         * Parallel.
         ************************************************/
        public static Bitmap ApplyFiltersPipelined(Bitmap image, Filter[] filters, int strips)
        {
            return new Pipeline(image, filters, strips).Result();
        }

        /************************************************
         * Applies an array of filters to an image.
         * Sequential. 
         ************************************************/
        public static Bitmap ApplyFilters(Bitmap image, Filter[] filters)
        {
            int width = image.Width;
            int height = image.Height;

            int[] pixels = GetPixels(image);
            int[] resultPixels = new int[pixels.Length];

            for (int i = 0; i < filters.Length; i++)
            {
                Process(filters[i], pixels, resultPixels, width, height, 0, height);

                // swap the arrays
                int[] temp = pixels;
                pixels = resultPixels;
                resultPixels = temp;
            }

            Bitmap result = new Bitmap(width, height);
            SetPixels(result, pixels);            
            return result;
        }

    }

}
