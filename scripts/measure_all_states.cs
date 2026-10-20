using System;
using System.IO;
using System.Drawing;

class MeasureAllStates
{
    static void Main()
    {
        string[] states = new string[] { "idle", "walk", "sleep", "fly" };
        foreach (string st in states)
        {
            string dir = Path.Combine(@"assets\sprites", st);
            string[] files = Directory.GetFiles(dir, "frame_0.png");
            if (files.Length == 0) files = Directory.GetFiles(dir, "*.png");
            if (files.Length == 0) continue;

            string f = files[0];
            using (Bitmap bmp = new Bitmap(f))
            {
                int minX = 999, maxX = -1, minY = 999, maxY = -1;
                int eyeCount = 0;
                int beakCount = 0;
                int bodyCount = 0;

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color c = bmp.GetPixel(x, y);
                        if (c.A > 0)
                        {
                            if (x < minX) minX = x;
                            if (x > maxX) maxX = x;
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                            bodyCount++;
                        }
                    }
                }

                int w = maxX - minX + 1;
                int h = maxY - minY + 1;
                Console.WriteLine("State: {0,-8} | Bounding Box: X=[{1,2}..{2,2}] (W={3,2}), Y=[{4,2}..{5,2}] (H={6,2}) | Pixels: {7}",
                    st, minX, maxX, w, minY, maxY, h, bodyCount);
            }
        }
    }
}
