using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class BuildExactReferenceGrid
{
    static readonly Dictionary<char, Color> Palette = new Dictionary<char, Color>
    {
        { '.', Color.FromArgb(0, 0, 0, 0) },             // Transparent
        { '#', Color.FromArgb(255, 26, 15, 10) },         // #1A0F0A Тёмный контур
        { 'K', Color.FromArgb(255, 18, 9, 6) },           // #120906 Зрачок
        { 'D', Color.FromArgb(255, 54, 31, 18) },         // #361F12 Глубокая тень
        { 'S', Color.FromArgb(255, 90, 56, 32) },         // #5A3820 Тень перьев
        { 'B', Color.FromArgb(255, 125, 83, 48) },        // #7D5330 Базовый коричневый
        { 'L', Color.FromArgb(255, 150, 104, 64) },       // #966840 Светло-коричневый блик
        { 'C', Color.FromArgb(255, 214, 186, 152) },      // #D6BA98 Бежевая грудка
        { 'F', Color.FromArgb(255, 234, 216, 188) },      // #EAD8BC Кремовый лицевой диск
        { 'W', Color.FromArgb(255, 245, 236, 224) },      // #F5ECE0 Блик в зрачке
        { 'Y', Color.FromArgb(255, 255, 167, 38) },       // #FFA726 Жёлтая радужка
        { 'O', Color.FromArgb(255, 245, 158, 11) },       // #F59E0B Оранжевый клюв и лапки
        { 'R', Color.FromArgb(255, 230, 81, 0) },         // #E65100 Насыщенно-оранжевая радужка
        { 'M', Color.FromArgb(255, 180, 83, 9) }          // #B45309 Тень подклювья и лапок
    };

    static char MapPixelToChar(Color c)
    {
        if (c.R > 230 && c.G > 230 && c.B > 230) return '.'; // Background

        // Contour / Pupil
        if (c.R < 35 && c.G < 25 && c.B < 25) return '#';

        // Eye White catchlight
        if (c.R > 235 && c.G > 225 && c.B > 210 && c.R - c.B < 30) return 'W';

        // Iris Yellow
        if (c.R > 230 && c.G > 140 && c.B < 60) return 'Y';
        // Iris / Beak Orange
        if (c.R > 200 && c.G > 100 && c.B < 40) return 'O';
        // Dark orange / shadow of beak
        if (c.R > 140 && c.G > 50 && c.B < 25) return 'M';

        // Cream facial disc
        if (c.R > 210 && c.G > 195 && c.B > 165 && (c.R - c.B) < 60) return 'F';
        // Breast beige
        if (c.R > 180 && c.G > 155 && c.B > 125) return 'C';

        // Browns:
        // Light brown highlight
        if (c.R > 135 && c.G > 90 && c.B > 50) return 'L';
        // Base warm brown
        if (c.R > 100 && c.G > 65 && c.B > 35) return 'B';
        // Shadow brown
        if (c.R > 65 && c.G > 40 && c.B > 20) return 'S';
        // Deep shadow
        return 'D';
    }

    public static void Main()
    {
        string refPath = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\.user_uploaded\media_1792300480076_e208efda.png";
        using (Bitmap src = new Bitmap(refPath))
        {
            // Bounding box in source: X=[106..875], Y=[63..982]
            // We want the owl placed with feet on Y=44, centered horizontally.
            // Width: 35 px, Height: 42 px (from Y=3 to Y=44)
            int srcMinX = 106, srcMaxX = 875;
            int srcMinY = 63, srcMaxY = 982;
            int srcW = srcMaxX - srcMinX + 1;
            int srcH = srcMaxY - srcMinY + 1;

            int dstW = 35;
            int dstH = 42;
            int startX = (48 - dstW) / 2; // 6
            int startY = 44 - dstH + 1;    // 3

            char[,] grid = new char[48, 48];
            for (int y = 0; y < 48; y++)
                for (int x = 0; x < 48; x++)
                    grid[y, x] = '.';

            // Sample using nearest neighbor on center of each block
            for (int gy = 0; gy < dstH; gy++)
            {
                for (int gx = 0; gx < dstW; gx++)
                {
                    double srcCenterX = srcMinX + (gx + 0.5) * srcW / dstW;
                    double srcCenterY = srcMinY + (gy + 0.5) * srcH / dstH;

                    int sx = Math.Min(src.Width - 1, Math.Max(0, (int)srcCenterX));
                    int sy = Math.Min(src.Height - 1, Math.Max(0, (int)srcCenterY));

                    Color c = src.GetPixel(sx, sy);
                    char ch = MapPixelToChar(c);

                    int targetX = startX + gx;
                    int targetY = startY + gy;
                    if (targetX >= 0 && targetX < 48 && targetY >= 0 && targetY < 48)
                    {
                        grid[targetY, targetX] = ch;
                    }
                }
            }

            // Create bitmap from grid
            Bitmap bmp = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
            for (int y = 0; y < 48; y++)
            {
                for (int x = 0; x < 48; x++)
                {
                    char ch = grid[y, x];
                    bmp.SetPixel(x, y, Palette[ch]);
                }
            }

            // 8x preview
            Bitmap p8x = new Bitmap(384, 384, PixelFormat.Format32bppArgb);
            for (int y = 0; y < 48; y++)
                for (int x = 0; x < 48; x++)
                {
                    Color col = bmp.GetPixel(x, y);
                    for (int dy = 0; dy < 8; dy++)
                        for (int dx = 0; dx < 8; dx++)
                            p8x.SetPixel(x * 8 + dx, y * 8 + dy, col);
                }

            string out8x = @"assets\preview_exact_ref_8x.png";
            string brain8x = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\preview_exact_ref_8x.png";
            p8x.Save(out8x, ImageFormat.Png);
            p8x.Save(brain8x, ImageFormat.Png);
            Console.WriteLine("Saved: " + out8x);

            // Output grid text to file
            List<string> lines = new List<string>();
            for (int y = 0; y < 48; y++)
            {
                char[] row = new char[48];
                for (int x = 0; x < 48; x++) row[x] = grid[y, x];
                lines.Add("\"" + new string(row) + "\", // " + y.ToString("D2"));
            }
            File.WriteAllLines(@"scripts\exact_ref_grid.txt", lines.ToArray());
            Console.WriteLine("Saved grid text to scripts\\exact_ref_grid.txt");
        }
    }
}
