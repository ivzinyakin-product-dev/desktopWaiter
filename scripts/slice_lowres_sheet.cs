using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

// Utility to slice a master grid spritesheet into individual frame PNGs
// Usage: slice_lowres_sheet.exe <input_image_path> <state_name> <frame_width> <frame_height> [start_index]
class LowResSheetSlicer
{
    static void Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Usage: slice_lowres_sheet.exe <input_image> <state_name> <frame_w> <frame_h> [startIndex]");
            Console.WriteLine("Example: slice_lowres_sheet.exe assets/sheets/owl_walk.png walk 48 48 0");
            return;
        }

        string inputPath = args[0];
        string stateName = args[1];
        int frameW = int.Parse(args[2]);
        int frameH = int.Parse(args[3]);
        int startIndex = (args.Length > 4) ? int.Parse(args[4]) : 0;

        if (!File.Exists(inputPath))
        {
            Console.WriteLine("Error: File not found: " + inputPath);
            return;
        }

        string baseDir = Directory.Exists("assets") ? Directory.GetCurrentDirectory() : AppDomain.CurrentDomain.BaseDirectory;
        string outDir = Path.Combine(baseDir, "assets", "sprites", stateName);
        Directory.CreateDirectory(outDir);

        using (Bitmap src = new Bitmap(inputPath))
        {
            int cols = src.Width / frameW;
            int rows = src.Height / frameH;
            int frameIdx = startIndex;

            Console.WriteLine("Source: {0} ({1}x{2})", inputPath, src.Width, src.Height);
            Console.WriteLine("Slicing into {0}x{1} cells ({2} cols x {3} rows)...", frameW, frameH, cols, rows);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Rectangle cropRect = new Rectangle(c * frameW, r * frameH, frameW, frameH);
                    using (Bitmap frameBmp = new Bitmap(frameW, frameH, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics g = Graphics.FromImage(frameBmp))
                        {
                            g.Clear(Color.Transparent);
                            g.DrawImage(src, new Rectangle(0, 0, frameW, frameH), cropRect, GraphicsUnit.Pixel);
                        }

                        string outName = string.Format("frame_{0}.png", frameIdx);
                        string outPath = Path.Combine(outDir, outName);
                        frameBmp.Save(outPath, ImageFormat.Png);
                        Console.WriteLine("Saved: {0} ({1}x{2})", outPath, frameW, frameH);
                        frameIdx++;
                    }
                }
            }
            Console.WriteLine("Done! Sliced {0} frames into {1}", frameIdx - startIndex, outDir);
        }
    }
}
