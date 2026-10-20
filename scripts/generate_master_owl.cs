using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class MasterOwlGenerator
{
    // 14-Color Palette from the Neural Grid
    static readonly Dictionary<char, Color> Palette = new Dictionary<char, Color>()
    {
        { '.', Color.FromArgb(0, 0, 0, 0) },            // Transparent
        { '#', ColorTranslator.FromHtml("#1A0F0A") },   // Outline
        { 'K', ColorTranslator.FromHtml("#120906") },   // Dark Pupil
        { 'D', ColorTranslator.FromHtml("#361F12") },   // Deep Plumage Shadow
        { 'S', ColorTranslator.FromHtml("#5A3820") },   // Feather Shadow / Down Chevrons
        { 'B', ColorTranslator.FromHtml("#7D5330") },   // Base Brown Body
        { 'L', ColorTranslator.FromHtml("#966840") },   // Light Brown Highlights (Brows/Wings)
        { 'C', ColorTranslator.FromHtml("#D6BA98") },   // Beige Chest & Belly
        { 'F', ColorTranslator.FromHtml("#EAD8BC") },   // Light Cream Facial Disc (Spectacles)
        { 'W', ColorTranslator.FromHtml("#F5ECE0") },   // Glint / Eye Highlight
        { 'Y', ColorTranslator.FromHtml("#FFA726") },   // Yellow-Amber Upper Iris
        { 'O', ColorTranslator.FromHtml("#F59E0B") },   // Orange Beak & Feet
        { 'R', ColorTranslator.FromHtml("#E65100") },   // Rich Orange Lower Iris
        { 'M', ColorTranslator.FromHtml("#B45309") }    // Beak & Talon Shadow
    };

    // 48x48 Grid with sweet, kind, big open eyes matching media_1792255575337_e988cba3.png
    static readonly string[] Grid = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "..........##........................##..........",  // 07
        "..........#L##....................##L#..........",  // 08
        "..........#SLL#..................#LLS#..........",  // 09
        "...........#SLL##..##########..##LLS#...........",  // 10
        ".........####SLLL##SBBBBBBBBS##LLLS####.........",  // 11
        ".........#LBSSSLLLL##BBBBBB##LLLLSSSBL#.........",  // 12
        "..........#SLLSSSSSLB#BBBB#BLSSSSSLLS#..........",  // 13
        "...........########SBBSBBSBBS########...........",  // 14
        "...........#SLLFFFF##BBSSBB##FFFFLLS#...........",  // 15
        "..........#SLLFF###FF#BBBB#FF###FFLLS#..........",  // 16
        ".........#SLLFF##RR#FFLBBLFF#RR##FFLLS#.........",  // 17
        "........#SLLFFF#RWKK#FFLLFF#WKKR#FFFLLS#........",  // 18
        "........#LLBFFF#YKKK#FFFFFF#KKKY#FFFBLL#........",  // 19
        "........#LBBFFF#YKKW#FFMMFF#KKWY#FFFBBL#........",  // 20
        "........#LBBFFF#ROOO#FMOOMF#OOOR#FFFBBL#........",  // 21
        "........#SLBBFFF####FFMOOMFF####FFFBBLS#........",  // 22
        "........#SLBBBFFFFFFFB#MM#BFFFFFFFBBBLS#........",  // 23
        "........###LBBBFFFFBBCC##CCBBFFFFBBBL###........",  // 24
        ".......#BBB#LBBBBBBCCCCCCCCCCBBBBBBL#BBB#.......",  // 25
        "......#BBBBB#BBBCCCCCCCCCCCCCCCCBBB#BBBBB#......",  // 26
        "......#BBBBBB#BBCSCCLCCCCCCLCCSCBB#BBBBBB#......",  // 27
        ".....#SB#BBBB#BCCCSSCCBCCBCCSSCCCB#BBBB#BS#.....",  // 28
        ".....##L#B#BB#BCCCCCCCCBBCCCCCCCCB#BB#B#L##.....",  // 29
        ".....#D#SL#BL#BCCCCCCCCCCCCCCCCCCB#LB#LS#D#.....",  // 30
        ".....#DSS#BL#BCCSCCCSCCCCCCSCCCSCCB#LB#SSD#.....",  // 31
        ".....#DBSBB##BCCCSCCCSCCCCSCCCSCCCB##BBSBD#.....",  // 32
        ".....##BSBL#BBBCCCCCCCCCCCCCCCCCCBBB#LBSB##.....",  // 33
        "......#B#L#BBBBCCCCSCCCCCCCCSCCCCBBBB#L#B#......",  // 34
        "......#L##SLBBBCCCCCSCCCCCCSCCCCCBBBLS##L#......",  // 35
        "......##D##SLBBBCCCCCCCSSCCCCCCCBBBLS##D##......",  // 36
        "........#D##SLLBBBCCCCCCCCCCCCBBBLLS##D#........",  // 37
        ".........#D##SSLBBBBBBBBBBBBBBBBLSS##D#.........",  // 38
        "..........##D###SSLLBBBBBBBBLLSS###D##..........",  // 39
        "............####D###SSSSSSSS###D####............",  // 40
        "..................##........##..................",  // 41
        ".................#OO#......#OO#.................",  // 42
        "................#OOOO#....#OOOO#................",  // 43
        "...............#MM##MM#..#MM##MM#...............",  // 44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    static void Main()
    {
        Console.WriteLine("Generating Master Frame 0 (48x48) with kind open eyes...");

        if (Grid.Length != 48)
        {
            throw new Exception("Grid rows count is not 48!");
        }

        int width = 48;
        int height = 48;

        using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
        {
            for (int y = 0; y < height; y++)
            {
                string row = Grid[y];
                if (row.Length != 48)
                {
                    throw new Exception(string.Format("Row {0} length is {1}, expected 48: '{2}'", y, row.Length, row));
                }

                for (int x = 0; x < width; x++)
                {
                    char c = row[x];
                    if (Palette.ContainsKey(c))
                    {
                        bmp.SetPixel(x, y, Palette[c]);
                    }
                    else
                    {
                        bmp.SetPixel(x, y, Color.Transparent);
                    }
                }
            }

            string baseDir = Directory.Exists("assets") ? Directory.GetCurrentDirectory() : AppDomain.CurrentDomain.BaseDirectory;
            string outDir = Path.Combine(baseDir, @"assets\sprites\idle");
            Directory.CreateDirectory(outDir);

            string outPath = Path.Combine(outDir, "frame_0.png");
            bmp.Save(outPath, ImageFormat.Png);

            // Also save 8x magnified preview
            string previewPath = Path.Combine(baseDir, @"assets\preview_master_frame_8x.png");
            using (Bitmap preview = new Bitmap(384, 384, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(preview))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    g.DrawImage(bmp, 0, 0, 384, 384);
                }
                preview.Save(previewPath, ImageFormat.Png);
            }

            Console.WriteLine("Done! Created {0} and 8x preview at {1}", outPath, previewPath);
        }
    }
}
