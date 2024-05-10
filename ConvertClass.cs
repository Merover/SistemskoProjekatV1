using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SysProjekat
{
    public class ConvertClass
    {
        public static void ConvertToGif(string imagePath, int index)
        {
            using var image = Image.Load<Rgba32>(imagePath);

            int width = image.Width;
            int height = image.Height;

            using var gif = new Image<Rgba32>(width, height);

            int finishedThreads = 0;
            object lockObject = new object();

            for (int i = 0; i < 6; i++)
            {
                int indexCopy = i;
                Thread thread = new Thread(() => EditImage(image.Clone(), indexCopy, gif.Frames, ref finishedThreads, lockObject));
                thread.Start();
            }

            while (true)
            {
                lock (lockObject)
                {
                    if (finishedThreads >= 6)
                        break;
                }
            }

            // !!!
            lock (lockObject)
            {
             //   gif.Save($"../../../GifFile{index}.gif");
            }
        }


        public static void EditImage(Image<Rgba32> image, int index, ImageFrameCollection<Rgba32> gifFrames, ref int finishedThreads, object lockObject)
        {
            byte red = 0, green = 0, blue = 0;

            switch (index)
            {
                case 0:
                    red = 0;
                    green = 50;
                    blue = 0;
                    break;
                case 1:
                    red = 50;
                    green = 0;
                    blue = 0;
                    break;
                case 2:
                    red = 0;
                    green = 0;
                    blue = 50;
                    break;
                case 3:
                    red = 50;
                    green = 50;
                    blue = 0;
                    break;
                case 4:
                    red = 0;
                    green = 50;
                    blue = 50;
                    break;
                case 5:
                    red = 50;
                    green = 0;
                    blue = 50;
                    break;
            }

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Rgba32 pixel = image[x, y];
                    if (red != 0)
                        pixel.R = red;
                    if (green != 0)
                        pixel.G = green;
                    if (blue != 0)
                        pixel.B = blue;
                    image[x, y] = pixel;
                }
            }

            lock (gifFrames)
            {
                gifFrames.AddFrame(image.Frames.RootFrame);
            }

            lock (lockObject)
            {
                finishedThreads++;
            }
        }
    }
}