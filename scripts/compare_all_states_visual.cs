using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

class CompareAllStates
{
    static void Main()
    {
        string[] states = new string[] { "idle", "walk", "sleep", "fly" };
        int scale = 8;
        int frameW = 48 * scale;
        int frameH = 48 * scale;
        int totalW = frameW * states.Length;
        int totalH = frameH + 40; // extra space for labels

        using (Bitmap comp = new Bitmap(totalW, totalH, PixelFormat.Format32bppArgb))
        using (Graphics g = Graphics.FromImage(comp))
        {
            g.Clear(Color.FromArgb(24, 30, 41)); // dark background
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

            for (int i = 0; i < states.Length; i++)
            {
                string st = states[i];
                string f = Path.Combine(@"assets\sprites", st, "frame_0.png");
                using (Bitmap bmp = new Bitmap(f))
                {
                    g.DrawImage(bmp, i * frameW, 30, frameW, frameH);
                }

                // Draw state label
                using (Font font = new Font("Segoe UI", 12, FontStyle.Bold))
                using (SolidBrush b = new SolidBrush(Color.Gold))
                {
                    g.DrawString(st.ToUpper(), font, b, i * frameW + 20, 5);
                }
            }

            // Draw alignment guide lines across all 4 frames
            using (Pen redPen = new Pen(Color.FromArgb(140, 255, 60, 60), 1))
            using (Pen bluePen = new Pen(Color.FromArgb(140, 60, 180, 255), 1))
            using (Pen greenPen = new Pen(Color.FromArgb(140, 60, 255, 100), 1))
            using (Pen floorPen = new Pen(Color.FromArgb(220, 255, 215, 0), 2))
            {
                // Guide: Head top (Y = 10)
                int headY = 30 + 10 * scale;
                g.DrawLine(redPen, 0, headY, totalW, headY);

                // Guide: Eye level (Y = 18)
                int eyeY = 30 + 18 * scale;
                g.DrawLine(bluePen, 0, eyeY, totalW, eyeY);

                // Guide: Beak level (Y = 22)
                int beakY = 30 + 22 * scale;
                g.DrawLine(greenPen, 0, beakY, totalW, beakY);

                // Guide: Baseline Floor (Y = 44)
                int floorY = 30 + 44 * scale;
                g.DrawLine(floorPen, 0, floorY, totalW, floorY);
            }

            string outPath = @"assets\preview_scale_comparison_all_states.png";
            comp.Save(outPath, ImageFormat.Png);
            string brainPath = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\preview_scale_comparison_all_states.png";
            comp.Save(brainPath, ImageFormat.Png);
            Console.WriteLine("Saved: " + outPath);
        }
    }
}
