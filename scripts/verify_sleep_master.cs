using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class VerifySleepMaster
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

    static readonly string[] Grid = new string[]
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

    static char FindClosestPalette(Color c)
    {
        if (c.A < 128 || (c.R >= 252 && c.G >= 252 && c.B >= 252)) return '.';

        double minDiff = double.MaxValue;
        char bestChar = '.';

        foreach (var kvp in Palette)
        {
            if (kvp.Key == '.') continue;
            Color pal = kvp.Value;
            // RGB Euclidean distance
            int dr = c.R - pal.R;
            int dg = c.G - pal.G;
            int db = c.B - pal.B;
            double diff = dr * dr + dg * dg + db * db;
            if (diff < minDiff)
            {
                minDiff = diff;
                bestChar = kvp.Key;
            }
        }
        return bestChar;
    }

    static void Main(string[] args)
    {
        string refPath = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\.user_uploaded\media_1792303321296_d9d6e6d2.png";
        
        Console.WriteLine("Loading reference image: " + refPath);
        using (Bitmap refBmp = new Bitmap(refPath))
        {
            int w = refBmp.Width;
            int h = refBmp.Height;
            int blockX = w / 48;
            int blockY = h / 48;

            Console.WriteLine("Image size: {0}x{1}, Block: {2}x{3}", w, h, blockX, blockY);

            // 1. Render Grid to 48x48
            Bitmap gridBmp = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
            for (int y = 0; y < 48; y++)
            {
                for (int x = 0; x < 48; x++)
                {
                    char ch = Grid[y][x];
                    gridBmp.SetPixel(x, y, Palette[ch]);
                }
            }

            // 2. Extract downsampled 48x48 from reference image
            Bitmap refDown = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
            char[,] refGrid = new char[48, 48];

            int totalDifferences = 0;
            List<string> diffList = new List<string>();

            for (int y = 0; y < 48; y++)
            {
                for (int x = 0; x < 48; x++)
                {
                    Color sample = refBmp.GetPixel(x * blockX + blockX / 2, y * blockY + blockY / 2);
                    char closest = FindClosestPalette(sample);
                    refGrid[y, x] = closest;
                    refDown.SetPixel(x, y, Palette[closest]);

                    char gridChar = Grid[y][x];
                    if (closest != gridChar)
                    {
                        totalDifferences++;
                        if (diffList.Count < 50)
                        {
                            diffList.Add(string.Format("Y={0,2}, X={1,2}: Grid='{2}' ({3}), Ref='{4}' (Sample: #{5:X2}{6:X2}{7:X2}, Alpha:{8})",
                                y, x, gridChar, Palette[gridChar].Name, closest, sample.R, sample.G, sample.B, sample.A));
                        }
                    }
                }
            }

            Console.WriteLine("==================================================");
            Console.WriteLine("Total differences between Grid and Ref Image: {0}", totalDifferences);
            Console.WriteLine("==================================================");

            if (totalDifferences > 0)
            {
                Console.WriteLine("Sample differences (up to 50):");
                foreach (var d in diffList)
                {
                    Console.WriteLine(d);
                }
            }
            else
            {
                Console.WriteLine("PERFECT 100% MATCH! Every single pixel matches between Grid and Ref Image!");
            }

            // Save visual comparison
            // Side-by-side 8x: [Grid (384x384)] [Ref (384x384)] [Diff Map (384x384)]
            int compW = 384 * 3;
            int compH = 384;
            using (Bitmap comp = new Bitmap(compW, compH, PixelFormat.Format32bppArgb))
            using (Graphics g = Graphics.FromImage(comp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                // 1. Grid rendered 8x
                g.DrawImage(gridBmp, 0, 0, 384, 384);

                // 2. Reference image 8x
                g.DrawImage(refBmp, 384, 0, 384, 384);

                // 3. Diff map: matching pixels transparent/faded, diff pixels glowing RED
                using (Bitmap diffMap = new Bitmap(48, 48, PixelFormat.Format32bppArgb))
                {
                    for (int y = 0; y < 48; y++)
                    {
                        for (int x = 0; x < 48; x++)
                        {
                            if (Grid[y][x] != refGrid[y, x])
                            {
                                diffMap.SetPixel(x, y, Color.Red);
                            }
                            else
                            {
                                Color c = Palette[Grid[y][x]];
                                if (c.A > 0)
                                    diffMap.SetPixel(x, y, Color.FromArgb(40, c.R, c.G, c.B));
                                else
                                    diffMap.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                            }
                        }
                    }
                    g.DrawImage(diffMap, 384 * 2, 0, 384, 384);
                }

                string outComp = @"assets\preview_sleep_grid_vs_ref.png";
                comp.Save(outComp, ImageFormat.Png);
                Console.WriteLine("Saved visual comparison to: " + outComp);

                string outBrain = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\preview_sleep_grid_vs_ref.png";
                comp.Save(outBrain, ImageFormat.Png);
            }

            // Also save the 48x48 sleep master sprite to assets/sprites/sleep/frame_0.png
            string sleepDir = @"assets\sprites\sleep";
            Directory.CreateDirectory(sleepDir);
            gridBmp.Save(Path.Combine(sleepDir, "frame_0.png"), ImageFormat.Png);
            Console.WriteLine("Saved master sleep sprite to assets/sprites/sleep/frame_0.png");

            // Also save 8x standalone preview
            using (Bitmap preview8x = new Bitmap(384, 384, PixelFormat.Format32bppArgb))
            using (Graphics g = Graphics.FromImage(preview8x))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(gridBmp, 0, 0, 384, 384);
                preview8x.Save(@"assets\preview_sleep_master_8x.png", ImageFormat.Png);
                preview8x.Save(@"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\preview_sleep_master_8x.png", ImageFormat.Png);
                Console.WriteLine("Saved 8x preview to assets/preview_sleep_master_8x.png");
            }

            gridBmp.Dispose();
            refDown.Dispose();
        }
    }
}
