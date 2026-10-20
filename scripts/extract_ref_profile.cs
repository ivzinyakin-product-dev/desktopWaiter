using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class ExtractRefProfile
{
    static readonly Dictionary<char, Color> Palette = new Dictionary<char, Color>
    {
        { '.', Color.FromArgb(0, 0, 0, 0) },             // Transparent
        { '#', Color.FromArgb(255, 26, 15, 10) },         // #1A0F0A
        { 'K', Color.FromArgb(255, 18, 9, 6) },           // #120906
        { 'D', Color.FromArgb(255, 54, 31, 18) },         // #361F12
        { 'S', Color.FromArgb(255, 90, 56, 32) },         // #5A3820
        { 'B', Color.FromArgb(255, 125, 83, 48) },        // #7D5330
        { 'L', Color.FromArgb(255, 150, 104, 64) },       // #966840
        { 'C', Color.FromArgb(255, 214, 186, 152) },      // #D6BA98
        { 'F', Color.FromArgb(255, 234, 216, 188) },      // #EAD8BC
        { 'W', Color.FromArgb(255, 245, 236, 224) },      // #F5ECE0
        { 'Y', Color.FromArgb(255, 255, 167, 38) },       // #FFA726
        { 'O', Color.FromArgb(255, 245, 158, 11) },       // #F59E0B
        { 'R', Color.FromArgb(255, 230, 81, 0) },         // #E65100
        { 'M', Color.FromArgb(255, 180, 83, 9) }          // #B45309
    };

    static char FindClosestPaletteChar(Color c)
    {
        if (c.R > 235 && c.G > 235 && c.B > 235) return '.';

        char bestChar = '#';
        double bestDist = double.MaxValue;

        foreach (var kvp in Palette)
        {
            if (kvp.Key == '.') continue;
            Color pal = kvp.Value;
            double dr = c.R - pal.R;
            double dg = c.G - pal.G;
            double db = c.B - pal.B;
            double dist = dr * dr * 0.299 + dg * dg * 0.587 + db * db * 0.114;
            if (dist < bestDist)
            {
                bestDist = dist;
                bestChar = kvp.Key;
            }
        }
        return bestChar;
    }

    public static void Main()
    {
        string refPath = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\.user_uploaded\media_1792300480076_e208efda.png";
        using (Bitmap src = new Bitmap(refPath))
        {
            int minX = src.Width, maxX = 0, minY = src.Height, maxY = 0;
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    Color c = src.GetPixel(x, y);
                    if (c.R < 240 || c.G < 240 || c.B < 240)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }
            Console.WriteLine(string.Format("Bounding Box: X=[{0}..{1}], Y=[{2}..{3}]", minX, maxX, minY, maxY));

            int srcW = maxX - minX + 1;
            int srcH = maxY - minY + 1;

            int targetH = 41;
            int targetW = (int)Math.Round((double)srcW / srcH * targetH);
            Console.WriteLine(string.Format("Target owl size in 48x48: {0}x{1} px", targetW, targetH));

            int startX = (48 - targetW) / 2;
            int startY = 45 - targetH;

            Bitmap rawBmp = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(rawBmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.Clear(Color.Transparent);
                g.DrawImage(src, new Rectangle(startX, startY, targetW, targetH), new Rectangle(minX, minY, srcW, srcH), GraphicsUnit.Pixel);
            }

            Bitmap quantized = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
            string[] grid = new string[48];

            for (int y = 0; y < 48; y++)
            {
                char[] rowChars = new char[48];
                for (int x = 0; x < 48; x++)
                {
                    Color c = rawBmp.GetPixel(x, y);
                    if (c.A < 128)
                    {
                        rowChars[x] = '.';
                        quantized.SetPixel(x, y, Color.Transparent);
                    }
                    else
                    {
                        char ch = FindClosestPaletteChar(c);
                        rowChars[x] = ch;
                        quantized.SetPixel(x, y, Palette[ch]);
                    }
                }
                grid[y] = new string(rowChars);
            }

            Bitmap preview8x = new Bitmap(384, 384, PixelFormat.Format32bppArgb);
            for (int y = 0; y < 48; y++)
                for (int x = 0; x < 48; x++)
                {
                    Color col = quantized.GetPixel(x, y);
                    for (int dy = 0; dy < 8; dy++)
                        for (int dx = 0; dx < 8; dx++)
                            preview8x.SetPixel(x * 8 + dx, y * 8 + dy, col);
                }

            string outPath = @"assets\test_ref_quantized_8x.png";
            preview8x.Save(outPath, ImageFormat.Png);
            preview8x.Save(@"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\test_ref_quantized_8x.png", ImageFormat.Png);
            Console.WriteLine("Saved test quantized: " + outPath);
        }
    }
}
