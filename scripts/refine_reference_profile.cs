using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class RefineReferenceProfile
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

    static char FindClosestPaletteChar(Color c, int gx, int gy)
    {
        // Pure white catchlight inside eye
        if (gx >= 30 && gx <= 34 && gy >= 14 && gy <= 18)
        {
            if (c.R > 215 && c.G > 215 && c.B > 215) return 'W';
        }

        // Contour / Pupil
        if (c.R < 42 && c.G < 30 && c.B < 30)
        {
            // Inside the eye: pupil
            if (gx >= 31 && gx <= 34 && gy >= 15 && gy <= 18) return 'K';
            return '#';
        }

        // Eye area (gy: 14..19, gx: 28..34)
        if (gx >= 28 && gx <= 34 && gy >= 14 && gy <= 19)
        {
            if (c.R > 220 && c.G > 220 && c.B > 200) return 'W';
            if (c.R < 45 && c.G < 35 && c.B < 35) return 'K';
            if (c.R > 225 && c.G > 145 && c.B < 60) return 'Y';
            if (c.R > 180 && c.G > 85 && c.B < 40) return 'R';
            if (c.R > 120 && c.G > 45 && c.B < 30) return 'M';
        }

        // Beak area (gy: 17..22, gx: 34..40)
        if (gx >= 34 && gx <= 40 && gy >= 17 && gy <= 22)
        {
            if (c.R < 45 && c.G < 35 && c.B < 35) return '#';
            if (c.R > 200 && c.G > 120 && c.B < 50) return 'O';
            if (c.R > 130 && c.G > 55 && c.B < 30) return 'M';
        }

        // Paws area (gy >= 41)
        if (gy >= 41)
        {
            if (c.R < 45 && c.G < 35 && c.B < 35) return '#';
            if (c.R > 205 && c.G > 125 && c.B < 50) return 'O';
            if (c.R > 135 && c.G > 55 && c.B < 30) return 'M';
        }

        // Facial disc cream (gy: 12..24, gx: 23..36)
        if (gx >= 23 && gx <= 36 && gy >= 12 && gy <= 24)
        {
            if (c.R > 195 && c.G > 175 && c.B > 140) return 'F';
            if (c.R > 155 && c.G > 135 && c.B > 105) return 'F';
        }

        // Breast beige (gy: 24..41, gx: 24..38)
        if (gx >= 24 && gx <= 38 && gy >= 24 && gy <= 41)
        {
            // Chevrons
            if (c.R < 115 && c.G < 85 && c.B < 60) return 'S';
            if (c.R > 175 && c.G > 145 && c.B > 110) return 'C';
        }

        // Browns for body and wing:
        if (c.R > 135 && c.G > 90 && c.B > 50) return 'L';
        if (c.R > 100 && c.G > 65 && c.B > 35) return 'B';
        if (c.R > 65 && c.G > 40 && c.B > 20) return 'S';
        if (c.R > 35 && c.G > 22 && c.B > 14) return 'D';

        return '#';
    }

    public static void Main()
    {
        string refPath = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\.user_uploaded\media_1792300480076_e208efda.png";
        using (Bitmap src = new Bitmap(refPath))
        {
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

            for (int gy = 0; gy < dstH; gy++)
            {
                for (int gx = 0; gx < dstW; gx++)
                {
                    double srcCenterX = srcMinX + (gx + 0.5) * srcW / dstW;
                    double srcCenterY = srcMinY + (gy + 0.5) * srcH / dstH;

                    int sx = Math.Min(src.Width - 1, Math.Max(0, (int)srcCenterX));
                    int sy = Math.Min(src.Height - 1, Math.Max(0, (int)srcCenterY));

                    Color c = src.GetPixel(sx, sy);
                    int targetX = startX + gx;
                    int targetY = startY + gy;

                    if (c.R > 220 && c.G > 220 && c.B > 220)
                    {
                        grid[targetY, targetX] = '.';
                    }
                    else
                    {
                        char ch = FindClosestPaletteChar(c, targetX, targetY);
                        grid[targetY, targetX] = ch;
                    }
                }
            }

            // Outside flood fill to eliminate background noise
            bool[,] visited = new bool[48, 48];
            Queue<Point> q = new Queue<Point>();
            // Enqueue all borders
            for (int x = 0; x < 48; x++)
            {
                if (grid[0, x] == '.') { q.Enqueue(new Point(x, 0)); visited[0, x] = true; }
                if (grid[47, x] == '.') { q.Enqueue(new Point(x, 47)); visited[47, x] = true; }
            }
            for (int y = 0; y < 48; y++)
            {
                if (grid[y, 0] == '.') { q.Enqueue(new Point(0, y)); visited[y, 0] = true; }
                if (grid[y, 47] == '.') { q.Enqueue(new Point(47, y)); visited[y, 47] = true; }
            }

            int[] dxs = { 1, -1, 0, 0 };
            int[] dys = { 0, 0, 1, -1 };
            while (q.Count > 0)
            {
                Point p = q.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    int nx = p.X + dxs[i];
                    int ny = p.Y + dys[i];
                    if (nx >= 0 && nx < 48 && ny >= 0 && ny < 48 && !visited[ny, nx])
                    {
                        if (grid[ny, nx] == '.')
                        {
                            visited[ny, nx] = true;
                            q.Enqueue(new Point(nx, ny));
                        }
                    }
                }
            }

            // Any '.' that wasn't connected to the outside (except in the eye) becomes contour or fill
            // Ensure eye catchlight 'W' is crisp
            // In the eye (Y: 15, X: 32):
            grid[15, 32] = 'W';
            // Beak hook tip check:
            // Ensure beak (Y: 18..21, X: 36..39) is clean:
            // Y=18: #OOOO#
            // Y=19: #MOOO#
            // Y=20: .#MM#

            // Save clean outputs
            Bitmap bmp = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
            Bitmap bmpFlipped = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
            for (int y = 0; y < 48; y++)
                for (int x = 0; x < 48; x++)
                {
                    char ch = grid[y, x];
                    bmp.SetPixel(x, y, Palette[ch]);
                    bmpFlipped.SetPixel(47 - x, y, Palette[ch]);
                }

            string baseDir = Directory.Exists("assets") ? Directory.GetCurrentDirectory() : AppDomain.CurrentDomain.BaseDirectory;
            string profileDir = Path.Combine(baseDir, @"assets\sprites\walk_profile");
            Directory.CreateDirectory(profileDir);

            string fileRight = Path.Combine(profileDir, "owl_profile_right.png");
            string fileLeft = Path.Combine(profileDir, "owl_profile_left.png");
            string fileFrame0 = Path.Combine(profileDir, "frame_0.png");

            bmp.Save(fileRight, ImageFormat.Png);
            bmpFlipped.Save(fileLeft, ImageFormat.Png);
            bmp.Save(fileFrame0, ImageFormat.Png);

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

            string out8x = @"assets\preview_exact_ref_refined_8x.png";
            string brain8x = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\preview_exact_ref_refined_8x.png";
            p8x.Save(out8x, ImageFormat.Png);
            p8x.Save(brain8x, ImageFormat.Png);
            Console.WriteLine("Saved: " + out8x);

            // Write formatted code for C# array
            List<string> lines = new List<string>();
            for (int y = 0; y < 48; y++)
            {
                char[] row = new char[48];
                for (int x = 0; x < 48; x++) row[x] = grid[y, x];
                lines.Add("\"" + new string(row) + "\", // " + y.ToString("D2"));
            }
            File.WriteAllLines(@"scripts\refined_ref_grid.txt", lines.ToArray());
            Console.WriteLine("Refined grid generated successfully!");
        }
    }
}
