using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class ProfileOwlGenerator
{
    static readonly Dictionary<char, Color> Palette = new Dictionary<char, Color>
    {
        { '.', Color.FromArgb(0, 0, 0, 0) },             // Прозрачный
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

    static readonly string[] OWL_PROFILE_GRID_48X48 = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        ".................###............................",  // 03
        ".................#SL##..........................",  // 04
        ".................##SLL##........................",  // 05
        "...................#SLLL##......................",  // 06
        ".................####SSLLL#######...............",  // 07
        "..................#SS##SLLLBBBBBB##.............",  // 08
        "..................##LLSSSSBBBBBBBBBS#...........",  // 09
        ".................#S##SLLSSSSBBBBBBBBLS#.........",  // 10
        "................#SSSS############BBBLLS#........",  // 11
        "...............#SSSSSBBBBBLBFFFFFF##BLL#........",  // 12
        "...............#SSSBBBBBBBLBFFF####FF#LS#.......",  // 13
        "...............#SSBBBBBBBBLBFFF#YYYY#FFSL#......",  // 14
        "..............#SSBBBBBBBBBLFFFF#YYWK##FCS#......",  // 15
        "..............#SSBBBBBBBBBLFFFF#YYKKK#FFF#......",  // 16
        "..............#SSBBBBBBBBBLFFFF#YYKKK#FF##......",  // 17
        "..............#SSBBBBBBBBBLFFFF#RRRRR#FOOO#.....",  // 18
        "..............#SSSBBBBBBBBSFFFFF#####FOOOOO#....",  // 19
        "..............#SSSSBBBBBBBLBFFFFFFFFF##MOOO#....",  // 20
        "..............#SSSSSBBBBBBLBFFFFFFFFS#M#OO#.....",  // 21
        ".............#SSS########LLSSFFFFFFSLS###.......",  // 22
        ".............#SS#BBBLLLLL#LLLSSSSSLLS#..........",  // 23
        "............#SS#BBBBBBBBLL#BBBBBBBCCCL#.........",  // 24
        "............#S#BBBBBBBBBBBL#BBBBBBCCCC#.........",  // 25
        "...........#S#BBBBBBBBBBBBBL#BBBBCCCCCC#........",  // 26
        "...........##BBBBBBBBBBBBBBL#BBBCCCSCSCC#.......",  // 27
        "...........##B#BBL#BBBBBBBBL#BBBCCCCSCCC#.......",  // 28
        "...........#BB#LL#BBL#BB#BBB#BBBCCCCCCCC#.......",  // 29
        "...........#BBS##BBL#BBL#BL##BBCSCSCSCSC#.......",  // 30
        "..........#SBBBS#LL#SLL#LL##SBBCCCSCCCSC#.......",  // 31
        "..........#BBL#SS##S###S##S#BBBCCCCCCCCC#.......",  // 32
        "..........#BL#SLLSSSBSSBBS#SBBBCCCCCCCC#........",  // 33
        ".........#LL#SBL#SLBSBBBS#SBBBCCCCSCSCC#........",  // 34
        ".........#L#SLL#SLL#BBSS#SBBBCCCCCCSCC#.........",  // 35
        "........###SLL#SLL#BBS##SBBBBCCCCCCCC#..........",  // 36
        ".......#S##LL#LLL#SS##SSBBBBCCCCCCCC#...........",  // 37
        "......#SLS##########SSSBBBBCCCCCC##.............",  // 38
        ".....#SLB#SSS####DSSSSBBBBBBCCCC#...............",  // 39
        ".....#LL#####....####SSBB#BBBB##................",  // 40
        ".....###............##########..................",  // 41
        ".....................#MO###MM###................",  // 42
        "....................#MOOOMO#MMOO#...............",  // 43
        "...................#MM##OO#O###MM#..............",  // 44
        "........................#M###...................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    static Bitmap RenderGrid(string[] grid, bool flipHorizontal = false)
    {
        int h = grid.Length;
        int w = grid[0].Length;
        Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);

        for (int y = 0; y < h; y++)
        {
            string row = grid[y];
            for (int x = 0; x < w; x++)
            {
                int srcX = flipHorizontal ? (w - 1 - x) : x;
                char c = row[srcX];
                Color col = Palette.ContainsKey(c) ? Palette[c] : Color.Transparent;
                bmp.SetPixel(x, y, col);
            }
        }
        return bmp;
    }

    static Bitmap ScaleNearestNeighbor(Bitmap src, int factor)
    {
        int sw = src.Width;
        int sh = src.Height;
        int dw = sw * factor;
        int dh = sh * factor;
        Bitmap dst = new Bitmap(dw, dh, PixelFormat.Format32bppArgb);

        for (int y = 0; y < sh; y++)
        {
            for (int x = 0; x < sw; x++)
            {
                Color c = src.GetPixel(x, y);
                for (int dy = 0; dy < factor; dy++)
                {
                    for (int dx = 0; dx < factor; dx++)
                    {
                        dst.SetPixel(x * factor + dx, y * factor + dy, c);
                    }
                }
            }
        }
        return dst;
    }

    static Bitmap CreateSideBySide(Bitmap leftBmp, Bitmap rightBmp, int spacing = 16)
    {
        int w = leftBmp.Width + rightBmp.Width + spacing;
        int h = Math.Max(leftBmp.Height, rightBmp.Height);
        Bitmap combined = new Bitmap(w, h, PixelFormat.Format32bppArgb);

        using (Graphics g = Graphics.FromImage(combined))
        {
            g.Clear(Color.Transparent);
            g.DrawImage(leftBmp, 0, 0);
            g.DrawImage(rightBmp, leftBmp.Width + spacing, 0);
        }
        return combined;
    }

    static void Main()
    {
        Console.WriteLine("==================================================");
        Console.WriteLine(" RENDERING PROFILE OWL 48x48 (NEW SPECIFICATION) ");
        Console.WriteLine("==================================================");

        if (OWL_PROFILE_GRID_48X48.Length != 48)
        {
            Console.WriteLine("ERROR: Grid rows count is " + OWL_PROFILE_GRID_48X48.Length + ", expected 48");
            return;
        }
        for (int i = 0; i < 48; i++)
        {
            if (OWL_PROFILE_GRID_48X48[i].Length != 48)
            {
                Console.WriteLine("ERROR: Row " + i + " has length " + OWL_PROFILE_GRID_48X48[i].Length + ", expected 48");
                return;
            }
        }
        Console.WriteLine("[PASS] Matrix dimensions verified: 48x48 characters.");

        // Target directories
        string baseDir = Directory.Exists("assets") ? Directory.GetCurrentDirectory() : AppDomain.CurrentDomain.BaseDirectory;
        string profileDir = Path.Combine(baseDir, @"assets\sprites\walk_profile");
        Directory.CreateDirectory(profileDir);

        // 1. Render Right-facing (native) and Left-facing (flipped)
        using (Bitmap bmpRight = RenderGrid(OWL_PROFILE_GRID_48X48, false))
        using (Bitmap bmpLeft = RenderGrid(OWL_PROFILE_GRID_48X48, true))
        {
            string fileRight = Path.Combine(profileDir, "owl_profile_right.png");
            string fileLeft = Path.Combine(profileDir, "owl_profile_left.png");
            string fileFrame0 = Path.Combine(profileDir, "frame_0.png");

            bmpRight.Save(fileRight, ImageFormat.Png);
            bmpLeft.Save(fileLeft, ImageFormat.Png);
            bmpRight.Save(fileFrame0, ImageFormat.Png);

            Console.WriteLine("[OK] Saved: " + fileRight);
            Console.WriteLine("[OK] Saved: " + fileLeft);
            Console.WriteLine("[OK] Saved: " + fileFrame0);

            // 2. Generate 8x previews
            using (Bitmap pRight8x = ScaleNearestNeighbor(bmpRight, 8))
            using (Bitmap pLeft8x = ScaleNearestNeighbor(bmpLeft, 8))
            {
                string preview8xPath = Path.Combine(baseDir, @"assets\preview_profile_grid_8x.png");
                pRight8x.Save(preview8xPath, ImageFormat.Png);
                Console.WriteLine("[OK] Saved 8x preview: " + preview8xPath);

                using (Bitmap sideBySide8x = CreateSideBySide(pLeft8x, pRight8x, 32))
                {
                    string previewSideBySidePath = Path.Combine(baseDir, @"assets\preview_profile_left_right_8x.png");
                    sideBySide8x.Save(previewSideBySidePath, ImageFormat.Png);
                    Console.WriteLine("[OK] Saved 8x comparison: " + previewSideBySidePath);

                    // Copy to artifact directory if present
                    string brainDir = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be";
                    if (Directory.Exists(brainDir))
                    {
                        string brainArtifact1 = Path.Combine(brainDir, "preview_profile_grid_8x.png");
                        string brainArtifact2 = Path.Combine(brainDir, "preview_profile_left_right_8x.png");
                        pRight8x.Save(brainArtifact1, ImageFormat.Png);
                        sideBySide8x.Save(brainArtifact2, ImageFormat.Png);
                        Console.WriteLine("[OK] Copied previews to artifact directory: " + brainDir);
                    }
                }
            }
        }

        Console.WriteLine("\n[COMPLETE] Profile owl successfully generated!");
    }
}
