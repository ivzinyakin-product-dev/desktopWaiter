using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class CenteredDiscOwl
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

    static readonly string[] Grid = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "...............##...............................",  // 07: Ear tuft tip
        "..............#L##..............................",  // 08
        "..............#SLL#.............................",  // 09
        "...............#SLL##...########................",  // 10: Crown top
        ".............###SLLL##SBBBBBBBBS#...............",  // 11
        "............#LBSSLLL#BBBBBBBBBBBB#..............",  // 12
        "...........#SLLSSSL#BBBBBBBBBBBBBB#.............",  // 13: Forehead curves forward
        "..........#SLLSSS#BBBBBBSSBBBBBBBBB#............",  // 14: Brown forehead over eye
        ".........#SLLFF#BBBBBBBBB##BB###BBBB#...........",  // 15: Brow line
        "........#SLLF#BBBBBBBBBB#FFFF###FFFF#...........",  // 16: Cream cushion over eye
        ".......#SLLF#BBBBBBBBB#FFF#RR##FFFFFF#..........",  // 17: Eye top
        "......#SLLF#BBBBBBBBB#FFF#RWKK#FFFFFF#..........",  // 18: Big eye with glint
        "......#LLB#BBBBBBBBB#FFF#YKKK#FFFFFF##..........",  // 19: Pupil K
        "......#LBB#BBBBBBBBB#FFF#YKKW#FFFFFF#OO#........",  // 20: Secondary glint W + beak base
        "......#LBB#BBBBBBBBB#FFF#ROOO#FFFFF#MOOO#.......",  // 21: Orange iris + beak body
        "......#SLB#BBBBBBBBBB#FFF####FFFFFF#MMO#........",  // 22: Cheek + beak hook curving down
        "......#SLB##BBBBBBBBBB#FFFFFFFFFFFF##M#.........",  // 23: Chin connects under beak
        ".......#BBB##BBBBBBBBBB#FFFFFFFFFFF##...........",  // 24: Chin to throat transition
        ".......#BBBBB##BBBBBBBBB#CCCCCCCCCCCC#..........",  // 25: Chest curves forward
        "......#BBBBBBB##BBBBBBBBB#CCCCCCCCCCCC#.........",  // 26: Plump round tummy
        ".....#BBBBBBBBB##BBBBBBBBB#CCCCCCCCCCC#.........",  // 27
        ".....#BB#BBBBBBB#BBBBBBBBB#CCCSCCCCCCCC#........",  // 28: Down chevrons
        "....#SB#B#BBBBBB##BBBBBBB#CCCCSSCCCCCCB#........",  // 29
        "....##L#B##BBBBB#DBBBBBB#CCCCCCCCCCCCB#.........",  // 30
        "....#D#SL#B#BBBB#DDBBBB#CCCCCCCCCCCCB#..........",  // 31
        "....#DSS#BL#BBB#DDSBBBB#CCSCCCCCSCCCB#..........",  // 32: Folded wing
        "....#DBSBB##BB#DDSSBBBB#CCSCCCCCSCCC#...........",  // 33
        "....##BSBL#BB#DDSSBBBBB#CCCCCCCCCCCC#...........",  // 34: Lower tummy
        ".....#B#L#BBB#DSSBBBBBBBCCCCSCCCCCCC#...........",  // 35
        ".....#L##SLBB#SBBBBBBBBBCCCCCSCCCCC#............",  // 36
        ".....##D##SLB#BBBBBBBBBBBCCCCCCCCC#.............",  // 37
        ".......#D##SLLBBBBBBBBBBBCCCCCCCC#..............",  // 38
        "........#D##SSLBBBBBBBBBBBBBC###................",  // 39: Tail tuck
        ".........##D###SSLLBBBBBBLLSS#..................",  // 40
        "...........####D###SSSSSSSS##...................",  // 41: Body base
        "..................##........##..................",  // 42: Far leg, Near leg
        ".................#MM#......#OO#.................",  // 43: Far shaded, Near bright
        "................#DD##DD#..#MM##MM#..............",  // 44: Baseline Y=44
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
            preview.Save("assets/preview_centered_disc_8x.png", ImageFormat.Png);
        }

        Console.WriteLine("Done! Created assets/preview_centered_disc_8x.png");
    }
}
