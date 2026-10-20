using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class CuteProfileTuner
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

    // Pixel-perfect Chibi Profile:
    // - Cute small curved beak (not a duck bill)
    // - Big kind eye centered in the face
    // - Balanced cream facial disc framed by brown skull
    // - Plump body with folded wing and cute feet
    static readonly string[] Grid = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "..............##................................",  // 07: Ear tuft tip
        ".............#L##...............................",  // 08
        ".............#SLL#..............................",  // 09
        "..............#SLL##...########.................",  // 10: Crown
        "............###SLLL##SBBBBBBBBS#................",  // 11
        "...........#LBSSLLL#BBBBBBBBBBBB#...............",  // 12
        "..........#SLLSSSL#BBBBBBBBBBBBBB#..............",  // 13: Forehead curves forward
        ".........#SLLSSS#BBBBBSSBB##BBBBBB#.............",  // 14
        "........#SLLFF#BBBBBB####FFFF####BB#............",  // 15: Brow arch over eye
        ".......#SLLF#BBBBBB##FFFFFFFFFFFF##F#...........",  // 16: Cream cushion
        "......#SLLF#BBBBBB#FFFFFF#RR##FFFFFFF#..........",  // 17: Eye socket top
        "......#LLB#BBBBBB#FFFFFF#RWKK#FFFFFFF#..........",  // 18: Glint W + Pupil K
        "......#LBB#BBBBBB#FFFFFF#YKKK#FFFFFF#MM#........",  // 19: Pupil K + Small beak root
        "......#LBB#BBBBBB#FFFFFF#YKKW#FFFFF#MOO#........",  // 20: Secondary glint W + Cute beak upper
        "......#LBB#BBBBBB#FFFFFF#ROOO#FFFF#MOOO#........",  // 21: Orange iris + Cute beak tip
        "......#SLB#BBBBBBB#FFFFFF####FFFFFF#MM#.........",  // 22: Lower eye socket + beak underside
        "......#SLB##BBBBBBB#FFFFFFFFFFFFFF###...........",  // 23: Soft cream cheek & chin
        ".......#BBB##BBBBBBB#FFFFFFFFFFFF##.............",  // 24: Chin tucks under
        ".......#BBBBB##BBBBBBB#CCCCCCCCCCCC#............",  // 25: Chest curves forward
        "......#BBBBBBB##BBBBBBB#CCCCCCCCCCCC#...........",  // 26: Plump round tummy
        ".....#BBBBBBBBB##BBBBBBB#CCCCCCCCCCCC#..........",  // 27
        ".....#BB#BBBBBBB#BBBBBBB#CCCSCCCCCCCC#..........",  // 28: Down chevrons
        "....#SB#B#BBBBBB##BBBBB#CCCCSSCCCCCCCC#.........",  // 29
        "....##L#B##BBBBB#DBBBBB#CCCCCCCCCCCCCC#.........",  // 30
        "....#D#SL#B#BBBB#DDBBBB#CCCCCCCCCCCCCC#.........",  // 31
        "....#DSS#BL#BBB#DDSBBBB#CCSCCCCCSCCCCC#.........",  // 32: Wing layers
        "....#DBSBB##BB#DDSSBBBB#CCSCCCCCSCCCCC#.........",  // 33
        "....##BSBL#BB#DDSSBBBBB#CCCCCCCCCCCCC#..........",  // 34: Lower tummy
        ".....#B#L#BBB#DSSBBBBBBBCCCCSCCCCCCC#...........",  // 35
        ".....#L##SLBB#SBBBBBBBBBCCCCCSCCCCC#............",  // 36
        ".....##D##SLB#BBBBBBBBBBBCCCCCCCCC#.............",  // 37
        ".......#D##SLLBBBBBBBBBBBCCCCCCCC#..............",  // 38
        "........#D##SSLBBBBBBBBBBBBBC###................",  // 39: Tail & rump tuck
        ".........##D###SSLLBBBBBBLLSS#..................",  // 40
        "...........####D###SSSSSSSS##...................",  // 41: Body bottom
        "..................##........##..................",  // 42: Far leg, Near leg
        ".................#MM#......#OO#.................",  // 43: Far shaded, Near bright
        "................#DD##DD#..#MM##MM#..............",  // 44: Feet on floor Y=44
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
            preview.Save("assets/preview_cute_profile_8x.png", ImageFormat.Png);
        }

        Console.WriteLine("Done! Created assets/preview_cute_profile_8x.png");
    }
}
