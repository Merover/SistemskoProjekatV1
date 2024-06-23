using System;
using System.IO;
using System.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SysProjekat
{
    public class ConvertClass
    {
        public static string ConvertToGif(string imagePath, int index)
        {
            using var image = Image.Load<Rgba32>(imagePath);

            int width = image.Width;
            int height = image.Height;

            using var gif = new Image<Rgba32>(width, height);

            ManualResetEvent[] resetEvents = new ManualResetEvent[6];

            for (int i = 0; i < 6; i++)
            {
                resetEvents[i] = new ManualResetEvent(false);
                int indexCopy = i;
                ThreadPool.QueueUserWorkItem(state =>
                {
                    EditImage(image.Clone(), indexCopy, gif.Frames);
                    resetEvents[indexCopy].Set();
                });
            }

            WaitHandle.WaitAll(resetEvents);

            string gifpath = $"../../../GifFile{index}.gif";
            gif.Save(gifpath);

            return gifpath;
        }

        public static void EditImage(Image<Rgba32> image, int index, ImageFrameCollection<Rgba32> gifFrames)
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
        }
    }
}

