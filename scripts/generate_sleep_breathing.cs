using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class SleepBreathingGenerator
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

    // Approved Master Grid from User
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
        for (int y = 0; y < 48; y++) res[y] = src[y].ToCharArray();
        return res;
    }

    static char[][] CloneGrid(char[][] src)
    {
        char[][] res = new char[48][];
        for (int y = 0; y < 48; y++) res[y] = (char[])src[y].Clone();
        return res;
    }

    static void ClearZzz(char[][] grid)
    {
        for (int y = 0; y <= 9; y++)
        {
            for (int x = 25; x < 48; x++) grid[y][x] = '.';
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
                if (c != '.') grid[targetY][targetX] = c;
            }
        }
    }

    // Patterns for Zzz particles
    static readonly string[] TinyZ = new string[]
    {
        "####",
        "#WW#",
        ".#W#",
        "####"
    };

    static readonly string[] SmallZ = new string[]
    {
        "######",
        "#WWWW#",
        "..#W#.",
        ".#W#..",
        "#WWWW#",
        "######"
    };

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

    // Inhale chest expansion: on rows 28..35, the beige chest 'C' expands 1px outward into the body
    static void ApplyChestExpansion(char[][] grid)
    {
        // Row 28: replace 1 '#B' on each flank with '#C' or widen chest by 1px
        // Let's widen the chest by 1 px left and right on rows 28..34:
        for (int y = 28; y <= 35; y++)
        {
            // Find first and last 'C' in this row
            int firstC = -1, lastC = -1;
            for (int x = 0; x < 48; x++)
            {
                if (grid[y][x] == 'C')
                {
                    if (firstC == -1) firstC = x;
                    lastC = x;
                }
            }
            if (firstC > 0 && grid[y][firstC - 1] == 'B')
            {
                grid[y][firstC - 1] = 'C';
            }
            if (lastC > 0 && lastC + 1 < 48 && grid[y][lastC + 1] == 'B')
            {
                grid[y][lastC + 1] = 'C';
            }
        }
    }

    // Inhale vertical lift: shifts torso and head (rows 10..42) UP by 1 px (to rows 09..41),
    // stretches ankles at row 42, keeps feet strictly at Y=43..44.
    static char[][] ApplyVerticalLift(char[][] baseGrid)
    {
        char[][] lifted = new char[48][];
        for (int y = 0; y < 48; y++)
        {
            lifted[y] = new char[48];
            for (int x = 0; x < 48; x++) lifted[y][x] = '.';
        }

        // Shift rows 10..42 up to 09..41
        for (int y = 10; y <= 42; y++)
        {
            for (int x = 0; x < 48; x++)
            {
                lifted[y - 1][x] = baseGrid[y][x];
            }
        }

        // Row 42: Ankle stretch connection under lifted body
        // Body bottom is now at row 41. Feet are at row 43 and 44.
        // Row 42 connects feet (X: 16..20 and 27..31) to body:
        for (int x = 0; x < 48; x++) lifted[42][x] = '.';
        lifted[42][17] = '#'; lifted[42][18] = 'O'; lifted[42][19] = '#';
        lifted[42][28] = '#'; lifted[42][29] = 'O'; lifted[42][30] = '#';

        // Row 43 and 44: Feet stay firmly anchored on floor horizon Y=44
        for (int x = 0; x < 48; x++)
        {
            lifted[43][x] = baseGrid[43][x];
            lifted[44][x] = baseGrid[44][x];
        }

        return lifted;
    }

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
        Console.WriteLine("Generating breathing animation for Sleep state...");

        // Clean base body from master grid (no Zzz)
        char[][] cleanBase = CloneGrid(MasterGrid);
        ClearZzz(cleanBase);

        // --- Frame 0: Exhale / Relaxed Rest ---
        // Body is relaxed at neutral height (Y: 10..44).
        // Tiny 'z' just forming above right ear tuft.
        char[][] f0 = CloneGrid(cleanBase);
        DrawPattern(f0, 32, 6, TinyZ);

        // --- Frame 1: Inhale Start (Chest Fluff Expansion) ---
        // Chest expands by 1px on flanks (soft expansion of breathing belly).
        // Small 'z' rises to Y: 4, large 'Z' emerges at top right.
        char[][] f1 = CloneGrid(cleanBase);
        ApplyChestExpansion(f1);
        DrawPattern(f1, 31, 4, SmallZ);
        DrawPattern(f1, 40, 3, TinyZ);

        // --- Frame 2: Deep Inhale Apex (Chest Expanded + Gentle 1px Vertical Lift) ---
        // Chest is expanded, torso and head rise +1px, ear tufts reach row 09, feet anchor at Y=44.
        // Full Zzz in the air: Small z at Y: 2, Large Z at Y: 0..6!
        char[][] f2_body = CloneGrid(cleanBase);
        ApplyChestExpansion(f2_body);
        char[][] f2 = ApplyVerticalLift(f2_body);
        DrawPattern(f2, 30, 2, SmallZ);
        DrawPattern(f2, 39, 0, LargeZ);

        // --- Frame 3: Exhale Transition (Settling back down) ---
        // Body settles back down 1px to neutral height (Y: 10..44).
        // Chest is halfway between expanded and relaxed.
        // Large Z floats off top (Y: -2..4, partially visible at Y: 0..4), small z fades.
        char[][] f3 = CloneGrid(cleanBase);
        ApplyChestExpansion(f3);
        DrawPattern(f3, 31, 3, SmallZ);
        DrawPattern(f3, 40, 1, LargeZ);

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
            string fullPath = Path.Combine(sleepDir, kvp.Key);
            bmp.Save(fullPath, ImageFormat.Png);
            bitmaps.Add(bmp);
            Console.WriteLine("Saved: " + fullPath);
        }

        // Render 8x Preview Strip
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

        Console.WriteLine("[OK] Generated breathing animation frames!");
    }
}
