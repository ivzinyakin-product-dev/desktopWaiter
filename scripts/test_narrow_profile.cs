using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class NarrowProfileBuilder
{
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

    // SLENDER 90° PROFILE (Width ~ 18-20 px, Clean Anatomical Wing, Zero Artifacts)
    static readonly string[] Grid = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "..................##............................",  // 07: Perky ear tuft tip
        ".................#L##...........................",  // 08
        "................#SLL#...........................",  // 09
        "...............#SLL##...#######.................",  // 10: Crown arch
        "..............##SLLL##SBBBBBBBBS#...............",  // 11
        ".............#LBSSLLL#BBBBBBBBBB#...............",  // 12: Forehead
        "............#SLLSSSL#BBBBBBBBBBBB#..............",  // 13
        "...........#SLLSSS#BBBBBBSSBBBBBBB#.............",  // 14: Forehead brow
        "...........#SLLFF#BBBBBBB##BB###BBB#............",  // 15: Brow arch
        "...........#SLLF#BBBBBBB#FFFF###FFFF#...........",  // 16: Cream cushion
        "...........#SLL#BBBBBBB#FFF#RR##FFFFF#..........",  // 17: Eye top
        "...........#LLB#BBBBBBB#FFF#RWKK#FFFFF#.........",  // 18: Big kind eye with glint
        "...........#LBB#BBBBBBB#FFF#YKKK#FFFFFF#........",  // 19: Pupil K
        "...........#LBB#BBBBBBB#FFF#YKKW#FFFFFF#OO#.....",  // 20: Glint W + beak base
        "...........#SLB#BBBBBBB#FFF#ROOO#FFFFF#OOOO#....",  // 21: Orange iris + curved beak body
        "...........#SLB##BBBBBBB#FFF####FFFFFF#MMO#.....",  // 22: Cheek + hooked beak tip
        "............#BBB##BBBBBBB#FFFFFFFFFFF#MMM#......",  // 23: Chin tucks under beak
        "............#BBBBB##BBBBBB#CCCCCCCCCCC#.........",  // 24: Slender chest curve
        ".............#BBBBB##BBBBBB#CCCCCCCCCCC#........",  // 25: Belly (not overly wide!)
        ".............#LBBBBB##BBBBBB#CCCCCCCCCCC#.......",  // 26
        ".............#LLBBBBB#BBBBBB#CCCSCCCCCCC#.......",  // 27: Clean wing shoulder + chevrons
        ".............#SLLBBBB##BBBBB#CCCCSSCCCCC#.......",  // 28
        ".............#SSLLBBB#DBBBBB#CCCCCCCCCCC#.......",  // 29: Smooth wing contour
        ".............#DSSLLBB#DDBBBB#CCCCCCCCCCC#.......",  // 30
        "..............#DDSSLB#DDSBBB#CCSCCCCCCC#........",  // 31: Layered wing feathers
        "..............#DDDSSL#DDSSBB#CCSCCCCCCC#........",  // 32
        "...............#DDDSSL#DDSSB#CCCCCCCCCC#........",  // 33: Primary flight feathers
        "................#DDDSLL#DSSB#CCCCSCCCC#.........",  // 34: Slender lower tummy
        ".................#DDDSLL#SBB#CCCCSCCCC#.........",  // 35
        "..................#DDDSSL#BB#CCCCCCCC#..........",  // 36
        "...................#DDDSSL#B#CCCCCCC#...........",  // 37: Wing tip & tail tuck
        "....................##DDDSSL#CCCCCC#............",  // 38
        "......................##DDSS#LLSS##.............",  // 39
        "........................####SSSS##..............",  // 40: Body base
        ".........................##.....##..............",  // 41: Far leg, Near leg
        "........................#MM#...#OO#.............",  // 42: Far shaded, Near bright
        ".......................#MMMM#.#OOOO#............",  // 43
        "......................#DD##DD#MM##MM#...........",  // 44: Floor contact on Y=44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    static void Main()
    {
        for (int y = 0; y < 48; y++)
        {
            if (Grid[y].Length != 48)
                throw new Exception(string.Format("Row {0} length is {1}, expected 48: '{2}'", y, Grid[y].Length, Grid[y]));
        }

        Bitmap bmp = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
        for (int y = 0; y < 48; y++)
        {
            for (int x = 0; x < 48; x++)
            {
                char c = Grid[y][x];
                bmp.SetPixel(x, y, Palette.ContainsKey(c) ? Palette[c] : Color.Transparent);
            }
        }

        using (Bitmap preview = new Bitmap(384, 384, PixelFormat.Format32bppArgb))
        {
            using (Graphics g = Graphics.FromImage(preview))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(bmp, 0, 0, 384, 384);
            }
            preview.Save("assets/preview_narrow_profile_8x.png", ImageFormat.Png);
        }

        Console.WriteLine("Done! Created assets/preview_narrow_profile_8x.png");
    }
}
