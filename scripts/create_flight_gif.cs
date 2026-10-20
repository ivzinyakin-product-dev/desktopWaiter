using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;

class FlightGifCreator
{
    public static void Main()
    {
        string flyDir = @"assets\sprites\fly";
        // 6-beat ping-pong loop: Up -> MidUp -> MidDown -> Down -> MidDown -> MidUp
        string[] framePaths = new string[]
        {
            Path.Combine(flyDir, "frame_0.png"),
            Path.Combine(flyDir, "frame_1.png"),
            Path.Combine(flyDir, "frame_2.png"),
            Path.Combine(flyDir, "frame_3.png"),
            Path.Combine(flyDir, "frame_2.png"),
            Path.Combine(flyDir, "frame_1.png")
        };

        int scale = 8;
        string outGif = @"assets\preview_flight_animation_8x.gif";
        string brainGif = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\preview_flight_animation_8x.gif";

        GifBitmapEncoder encoder = new GifBitmapEncoder();

        foreach (string fp in framePaths)
        {
            using (Bitmap raw = new Bitmap(fp))
            {
                int w = raw.Width * scale;
                int h = raw.Height * scale;
                using (Bitmap scaled = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(scaled))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                        g.Clear(System.Drawing.Color.Transparent);
                        g.DrawImage(raw, new Rectangle(0, 0, w, h));
                    }

                    IntPtr hBitmap = scaled.GetHbitmap();
                    try
                    {
                        BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                            hBitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                        encoder.Frames.Add(BitmapFrame.Create(bs));
                    }
                    finally
                    {
                        DeleteObject(hBitmap);
                    }
                }
            }
        }

        using (FileStream fs = new FileStream(outGif, FileMode.Create))
        {
            encoder.Save(fs);
        }
        File.Copy(outGif, brainGif, true);
        Console.WriteLine("Saved animated flight GIF: " + outGif);
    }

    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);
}
