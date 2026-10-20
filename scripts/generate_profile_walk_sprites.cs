using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class ProfileWalkGenerator
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

    static char[,] LoadMasterGrid()
    {
        string[] lines = File.ReadAllLines(@"scripts\refined_ref_grid.txt");
        char[,] grid = new char[48, 48];
        for (int y = 0; y < 48; y++)
        {
            string line = lines[y];
            int start = line.IndexOf('"') + 1;
            string row = line.Substring(start, 48);
            for (int x = 0; x < 48; x++)
            {
                grid[y, x] = row[x];
            }
        }
        return grid;
    }

    static char[,] CloneGrid(char[,] src)
    {
        char[,] dst = new char[48, 48];
        for (int y = 0; y < 48; y++)
            for (int x = 0; x < 48; x++)
                dst[y, x] = src[y, x];
        return dst;
    }

    static Bitmap GridToBitmap(char[,] grid, bool flip = false)
    {
        Bitmap bmp = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
        for (int y = 0; y < 48; y++)
        {
            for (int x = 0; x < 48; x++)
            {
                int srcX = flip ? (47 - x) : x;
                char ch = grid[y, srcX];
                Color col = Palette.ContainsKey(ch) ? Palette[ch] : Color.Transparent;
                bmp.SetPixel(x, y, col);
            }
        }
        return bmp;
    }

    static Bitmap ScaleNN(Bitmap src, int scale)
    {
        int sw = src.Width, sh = src.Height;
        Bitmap dst = new Bitmap(sw * scale, sh * scale, PixelFormat.Format32bppArgb);
        for (int y = 0; y < sh; y++)
            for (int x = 0; x < sw; x++)
            {
                Color col = src.GetPixel(x, y);
                for (int dy = 0; dy < scale; dy++)
                    for (int dx = 0; dx < scale; dx++)
                        dst.SetPixel(x * scale + dx, y * scale + dy, col);
            }
        return dst;
    }

    public static void Main()
    {
        Console.WriteLine("==================================================");
        Console.WriteLine(" GENERATING 4-FRAME PROFILE WALK CYCLE (48x48)   ");
        Console.WriteLine("==================================================");

        char[,] master = LoadMasterGrid();

        // Separate layers from master:
        // Body (Y: 3..40)
        // Back Paw (Left paw in view): Y in 41..45, X in 18..27
        // Front Paw (Right paw in view): Y in 41..45, X in 28..37

        char[][,] frames = new char[4][,];

        // -------------------------------------------------------------
        // FRAME 0: Contact A (Neutral master stance)
        // -------------------------------------------------------------
        frames[0] = CloneGrid(master);

        // -------------------------------------------------------------
        // FRAME 1: Passing A (Front paw lifts, Back paw holds floor, Body dips 1px)
        // -------------------------------------------------------------
        frames[1] = new char[48, 48];
        for (int y = 0; y < 48; y++)
            for (int x = 0; x < 48; x++)
                frames[1][y, x] = '.';

        // Body dips by 1 px: shift rows 3..40 down to 4..41
        for (int y = 3; y <= 40; y++)
        {
            for (int x = 0; x < 48; x++)
            {
                char ch = master[y, x];
                if (ch != '.')
                {
                    // Tail micro-bob on the left: keep tip up
                    if (x <= 13 && y >= 36)
                    {
                        frames[1][y, x] = ch; // tail stays slightly higher
                    }
                    else
                    {
                        frames[1][y + 1, x] = ch;
                    }
                }
            }
        }

        // Back paw (X: 18..27): stays planted firmly on floor Y=44
        for (int y = 41; y <= 45; y++)
        {
            for (int x = 18; x <= 27; x++)
            {
                char ch = master[y, x];
                if (ch != '.') frames[1][y, x] = ch;
            }
        }

        // Front paw (X: 28..37): LIFTS up by 2 px!
        for (int y = 41; y <= 45; y++)
        {
            for (int x = 28; x <= 37; x++)
            {
                char ch = master[y, x];
                if (ch != '.')
                {
                    int ny = y - 2;
                    if (ny >= 0 && ny < 48) frames[1][ny, x + 1] = ch;
                }
            }
        }

        // -------------------------------------------------------------
        // FRAME 2: Contact B (Opposite stride stance)
        // Front paw plants forward on ground Y=44, Back paw trails behind
        // -------------------------------------------------------------
        frames[2] = new char[48, 48];
        for (int y = 0; y < 48; y++)
            for (int x = 0; x < 48; x++)
                frames[2][y, x] = '.';

        // Body at neutral height (Y=0)
        for (int y = 3; y <= 40; y++)
            for (int x = 0; x < 48; x++)
                frames[2][y, x] = master[y, x];

        // Back paw: shifted 1 px left (trailing) on ground Y=44
        for (int y = 41; y <= 45; y++)
        {
            for (int x = 18; x <= 27; x++)
            {
                char ch = master[y, x];
                if (ch != '.')
                {
                    int nx = x - 1;
                    if (nx >= 0) frames[2][y, nx] = ch;
                }
            }
        }

        // Front paw: planted forward (shifted 1 px right) on ground Y=44
        for (int y = 41; y <= 45; y++)
        {
            for (int x = 28; x <= 37; x++)
            {
                char ch = master[y, x];
                if (ch != '.')
                {
                    int nx = x + 1;
                    if (nx < 48) frames[2][y, nx] = ch;
                }
            }
        }

        // -------------------------------------------------------------
        // FRAME 3: Passing B (Back paw lifts, Front paw holds floor, Body dips 1px)
        // -------------------------------------------------------------
        frames[3] = new char[48, 48];
        for (int y = 0; y < 48; y++)
            for (int x = 0; x < 48; x++)
                frames[3][y, x] = '.';

        // Body dips by 1 px
        for (int y = 3; y <= 40; y++)
        {
            for (int x = 0; x < 48; x++)
            {
                char ch = master[y, x];
                if (ch != '.')
                {
                    frames[3][y + 1, x] = ch;
                }
            }
        }

        // Front paw (X: 28..37): stays planted firmly on floor Y=44
        for (int y = 41; y <= 45; y++)
        {
            for (int x = 28; x <= 37; x++)
            {
                char ch = master[y, x];
                if (ch != '.') frames[3][y, x] = ch;
            }
        }

        // Back paw (X: 18..27): LIFTS up by 2 px, swinging forward!
        for (int y = 41; y <= 45; y++)
        {
            for (int x = 18; x <= 27; x++)
            {
                char ch = master[y, x];
                if (ch != '.')
                {
                    int ny = y - 2;
                    int nx = x + 1;
                    if (ny >= 0 && ny < 48 && nx < 48) frames[3][ny, nx] = ch;
                }
            }
        }

        // Save all 4 frames to assets/sprites/walk_profile/
        string baseDir = Directory.Exists("assets") ? Directory.GetCurrentDirectory() : AppDomain.CurrentDomain.BaseDirectory;
        string walkProfDir = Path.Combine(baseDir, @"assets\sprites\walk_profile");
        Directory.CreateDirectory(walkProfDir);

        Bitmap[] bitmaps = new Bitmap[4];
        for (int i = 0; i < 4; i++)
        {
            bitmaps[i] = GridToBitmap(frames[i], false);
            string file = Path.Combine(walkProfDir, string.Format("frame_{0}.png", i));
            bitmaps[i].Save(file, ImageFormat.Png);
            Console.WriteLine(string.Format("[OK] Saved: frame_{0}.png (48x48 px)", i));
        }

        // Create 8x horizontal preview strip
        int frameW = 48 * 8;
        int frameH = 48 * 8;
        int spacing = 16;
        int stripW = frameW * 4 + spacing * 3;
        Bitmap strip = new Bitmap(stripW, frameH, PixelFormat.Format32bppArgb);

        using (Graphics g = Graphics.FromImage(strip))
        {
            g.Clear(Color.Transparent);
            for (int i = 0; i < 4; i++)
            {
                using (Bitmap frame8x = ScaleNN(bitmaps[i], 8))
                {
                    g.DrawImage(frame8x, i * (frameW + spacing), 0);
                }
            }
        }

        string stripPath = Path.Combine(baseDir, @"assets\preview_profile_walk_strip_8x.png");
        string brainStripPath = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\preview_profile_walk_strip_8x.png";
        strip.Save(stripPath, ImageFormat.Png);
        strip.Save(brainStripPath, ImageFormat.Png);
        Console.WriteLine("[OK] Saved 8x strip: " + stripPath);

        Console.WriteLine("\n[COMPLETE] 4-frame profile walk cycle generated successfully!");
    }
}
