using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class GenerateSleepAnimation
{
    static readonly Dictionary<char, Color> Palette = new Dictionary<char, Color>()
    {
        { '.', Color.FromArgb(0, 0, 0, 0) },            // Transparent
        { '#', ColorTranslator.FromHtml("#1A0F0A") },   // Outline
        { 'K', ColorTranslator.FromHtml("#120906") },   // Dark Pupil
        { 'D', ColorTranslator.FromHtml("#361F12") },   // Deep Plumage Shadow
        { 'S', ColorTranslator.FromHtml("#5A3820") },   // Feather Shadow
        { 'B', ColorTranslator.FromHtml("#7D5330") },   // Base Brown Body
        { 'L', ColorTranslator.FromHtml("#966840") },   // Light Brown Highlights
        { 'C', ColorTranslator.FromHtml("#D6BA98") },   // Beige Chest & Belly
        { 'F', ColorTranslator.FromHtml("#EAD8BC") },   // Light Cream Facial Disc
        { 'W', ColorTranslator.FromHtml("#F5ECE0") },   // Glint / Sleep Zzz
        { 'Y', ColorTranslator.FromHtml("#FFA726") },   // Yellow-Amber Upper Iris
        { 'O', ColorTranslator.FromHtml("#F59E0B") },   // Orange Beak & Feet
        { 'R', ColorTranslator.FromHtml("#E65100") },   // Rich Orange Lower Iris
        { 'M', ColorTranslator.FromHtml("#B45309") }    // Beak & Talon Shadow
    };

    // User's Exact Approved Master Grid
    static readonly string[] MasterGrid = new string[]
    {
        "................................................",  // 00
        ".......................................#######..",  // 01
        ".......................................#WWWWW#..",  // 02
        "..............................######......#WW#..",  // 03
        "..............................#WWWW#.....#WW#...",  // 04
        "................................#W#.....#WW#....",  // 05
        "...............................#W#.....#WWWWW#..",  // 06
        "..............................#WWWW#...#######..",  // 07
        "..............................######............",  // 08
        "................................................",  // 09
        ".........###........................###.........",  // 10
        "........#SLL##....................##LLS#........",  // 11
        "........#SSLLL##..############..##LLLSS#........",  // 12
        ".......####SSLLL##SBBBBBBBBBBS##LLLSS####.......",  // 13
        ".......#LBSSSSLLLL##BBBBBBBB##LLLLSSSSBL#.......",  // 14
        "........#SLLSSSSSLB#BBBBBBBB#BLSSSSSLLS#........",  // 15
        ".........#########SBBSBBBBSBBS#########.........",  // 16
        "..........#SLLFFFF##BBSSSSBB##FFFFLLS#..........",  // 17
        ".........#SLLFFFFFFFBBBBBBBBFFFFFFFLLS#.........",  // 18
        "........#SLLFFFFFFFFFLLBBLLFFFFFFFFFLLS#........",  // 19
        ".......#SLLFFF#LLLL#FFFLLFFF#LLLL#FFFLLS#.......",  // 20
        ".......#LLBFFF#BBBB#FFFFFFFF#BBBB#FFFBLL#.......",  // 21
        ".......#LBBFF##SSSS##FOOOOF##SSSS##FFBBL#.......",  // 22
        ".......#LBBFFF######FOOOOOOF######FFFBBL#.......",  // 23
        ".......#SLBBFFFFFFFFFMOOOOMFFFFFFFFFBBLS#.......",  // 24
        ".......#SLBBBFFFFFFFB#MMMM#BFFFFFFFBBBLS#.......",  // 25
        ".......###LBBBFFFFBBCC####CCBBFFFFBBBL###.......",  // 26
        "......#BBB#LBBBBBBCCCCCCCCCCCCBBBBBBL#BBB#......",  // 27
        ".....#BBBBB#BBBCCCCCCCCCCCCCCCCCCBBB#BBBBB#.....",  // 28
        ".....#BBBBBB#BBCSCCLCCCCCCCCLCCSCBB#BBBBBB#.....",  // 29
        "....#SB#BBBB#BCCCSSCCBCCCCBCCSSCCCB#BBBB#BS#....",  // 30
        "....##L#B#BB#BCCCCCCCCBBBBCCCCCCCCB#BB#B#L##....",  // 31
        "....#D#SL#BL#BCCCCCCCCCCCCCCCCCCCCB#LB#LS#D#....",  // 32
        "....#DSS#BL#BCCSCCCSCCCCCCCCSCCCSCCB#LB#SSD#....",  // 33
        "....#DBSBB##BCCCSCCCSCCCCCCSCCCSCCCB##BBSBD#....",  // 34
        "....##BSBL#BBBCCCCCCCCCCCCCCCCCCCCBBB#LBSB##....",  // 35
        ".....#B#L#BBBBCCCCSCCCCCCCCCCSCCCCBBBB#L#B#.....",  // 36
        ".....#L##SLBBBCCCCCSCCCCCCCCSCCCCCBBBLS##L#.....",  // 37
        ".....##D##SLBBBCCCCCCCSSSSCCCCCCCBBBLS##D##.....",  // 38
        ".......#D##SLLBBBCCCCCCCCCCCCCCBBBLLS##D#.......",  // 39
        "........#D##SSLBBBBBBBBBBBBBBBBBBLSS##D#........",  // 40
        ".........##D###SSLLBBBBBBBBBBLLSS###D##.........",  // 41
        "...........####D###SSSSSSSSSS###D####...........",  // 42
        "................#OO##......##OO#................",  // 43
        "...............#MM##MM#..#MM##MM#...............",  // 44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    static char[][] CloneGrid(string[] src)
    {
        char[][] res = new char[48][];
        for (int y = 0; y < 48; y++)
        {
            res[y] = src[y].ToCharArray();
        }
        return res;
    }

    static char[][] CloneGrid(char[][] src)
    {
        char[][] res = new char[48][];
        for (int y = 0; y < 48; y++)
        {
            res[y] = (char[])src[y].Clone();
        }
        return res;
    }

    static void ClearZzz(char[][] grid)
    {
        for (int y = 0; y <= 9; y++)
        {
            for (int x = 25; x < 48; x++)
            {
                grid[y][x] = '.';
            }
        }
    }

    static void DrawPattern(char[][] grid, int startX, int startY, string[] pat)
    {
        for (int y = 0; y < pat.Length; y++)
        {
            int targetY = startY + y;
            if (targetY < 0 || targetY >= 48) continue;
            for (int x = 0; x < pat[y].Length; x++)
            {
                int targetX = startX + x;
                if (targetX < 0 || targetX >= 48) continue;
                char c = pat[y][x];
                if (c != '.')
                {
                    grid[targetY][targetX] = c;
                }
            }
        }
    }

    // Pattern for small 'z' (6x6)
    static readonly string[] SmallZ = new string[]
    {
        "######",
        "#WWWW#",
        "..#W#.",
        ".#W#..",
        "#WWWW#",
        "######"
    };

    // Pattern for tiny 'z' (4x4)
    static readonly string[] TinyZ = new string[]
    {
        "####",
        "#WW#",
        ".#W#",
        "####"
    };

    // Pattern for large 'Z' (7x7)
    static readonly string[] LargeZ = new string[]
    {
        "#######",
        "#WWWWW#",
        "...#WW#",
        "..#WW#.",
        ".#WW#..",
        "#WWWWW#",
        "#######"
    };

    static Bitmap RenderGrid(char[][] grid)
    {
        Bitmap bmp = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
        for (int y = 0; y < 48; y++)
        {
            for (int x = 0; x < 48; x++)
            {
                char ch = grid[y][x];
                bmp.SetPixel(x, y, Palette[ch]);
            }
        }
        return bmp;
    }

    static void Main()
    {
        Console.WriteLine("Generating complete Sleep Animation Set...");

        // 1. Create Clean Body (base)
        char[][] cleanBody = CloneGrid(MasterGrid);
        ClearZzz(cleanBody);

        // 2. Generate 4 Frames:
        // Frame 0: Exhale / Relaxed rest. Tiny 'z' just appearing at right ear.
        char[][] f0 = CloneGrid(cleanBody);
        DrawPattern(f0, 32, 6, TinyZ);

        // Frame 1: Inhale start. Small 'z' rises to mid-height (Y: 4, X: 31). Large Z starts emerging at top right.
        char[][] f1 = CloneGrid(cleanBody);
        DrawPattern(f1, 31, 4, SmallZ);
        // Hint of rising Z at top right
        DrawPattern(f1, 40, 3, TinyZ);

        // Frame 2: Deep Inhale Apex. EXACT Master Grid (Small z at X:30 Y:3, Large Z at X:39 Y:1)
        char[][] f2 = CloneGrid(MasterGrid);

        // Frame 3: Exhale transition. Large Z floats up to Y:0, Small z floats up to Y:2, fading.
        char[][] f3 = CloneGrid(cleanBody);
        DrawPattern(f3, 31, 2, SmallZ);
        // Large Z top part floating off canvas (Y: 0)
        DrawPattern(f3, 40, 0, LargeZ);

        var frames = new Dictionary<string, char[][]>()
        {
            { "frame_0.png", f0 },
            { "frame_1.png", f1 },
            { "frame_2.png", f2 },
            { "frame_3.png", f3 }
        };

        string sleepDir = @"assets\sprites\sleep";
        Directory.CreateDirectory(sleepDir);

        var bitmaps = new List<Bitmap>();

        foreach (var kvp in frames)
        {
            Bitmap bmp = RenderGrid(kvp.Value);
            string p = Path.Combine(sleepDir, kvp.Key);
            bmp.Save(p, ImageFormat.Png);
            bitmaps.Add(bmp);
            Console.WriteLine("Saved: " + p);
        }

        // Also save master frame with and without Zzz
        Bitmap masterWithZzz = RenderGrid(CloneGrid(MasterGrid));
        masterWithZzz.Save(@"assets\sprites\sleep\master_frame.png", ImageFormat.Png);
        masterWithZzz.Save(@"assets\owl_sleep_with_zzz.png", ImageFormat.Png);

        Bitmap masterClean = RenderGrid(cleanBody);
        masterClean.Save(@"assets\owl_sleep_clean.png", ImageFormat.Png);

        // 8x Preview Strip (4 frames side-by-side, 384x384 each -> 1536x384)
        int scale = 8;
        int frameW = 48 * scale;
        int frameH = 48 * scale;
        using (Bitmap strip = new Bitmap(frameW * 4, frameH, PixelFormat.Format32bppArgb))
        using (Graphics g = Graphics.FromImage(strip))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

            for (int i = 0; i < 4; i++)
            {
                g.DrawImage(bitmaps[i], i * frameW, 0, frameW, frameH);
            }

            string stripPath = @"assets\preview_sleep_strip_8x.png";
            strip.Save(stripPath, ImageFormat.Png);
            string brainStrip = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\preview_sleep_strip_8x.png";
            strip.Save(brainStrip, ImageFormat.Png);
            Console.WriteLine("Saved 8x strip to: " + stripPath);
        }

        Console.WriteLine("[OK] Sleep Animation Set successfully generated!");
    }
}
