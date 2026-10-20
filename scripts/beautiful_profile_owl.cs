using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class BeautifulProfileOwl
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

    // Correct Proportions:
    // Head: Rounded brown skull on back, circular cream facial disc around the big eye, hooked cute beak.
    // Body: Plump, fluffy beige chest in front, beautiful folded wing on back, two feet on Y=44.
    static readonly string[] Grid = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "................##..............................",  // 07: Ear tuft tip
        "...............#L##.............................",  // 08
        "..............#SLL#.............................",  // 09
        ".............#SLL##...########..................",  // 10: Head crown arch
        "............#SLLL##SBBBBBBBBBBS#................",  // 11
        "...........#LBSSLLL#BBBBBBBBBBBB#...............",  // 12: Forehead & crown
        "..........#SLLSSSL#BBBBBBBBBBBBBB#..............",  // 13
        ".........#SLLSSS#BBBBBSSBBFFFFFFFF#.............",  // 14: Facial disc starts
        "........#SLLFF#BBBBB####FFFF###FFFF#............",  // 15: Brow arch
        ".......#SLLF#BBBBBB#FFFFFFF#RR##FFFF#...........",  // 16: Cream cushion over eye
        "......#SLLF#BBBBBB#FFFFFFF#RWKK#FFFFF#..........",  // 17: Big kind eye top & glint
        "......#LLB#BBBBBB#FFFFFFF#YKKKK#FFFFFF#.........",  // 18: Big black pupil K & amber Y
        "......#LBB#BBBBBB#FFFFFFF#YKKKW#FFFFFF#MM#......",  // 19: Glint W + beak base
        "......#LBB#BBBBBB#FFFFFFF#ROOOR#FFFFF#MOOO#.....",  // 20: Orange iris + beak body
        "......#SLB#BBBBBBB#FFFFFFF#####FFFFFF#MOOO#.....",  // 21: Eye bottom + beak tip hook
        "......#SLB##BBBBBBB#FFFFFFFFFFFFFFFFF#MMM#......",  // 22: Cheek + beak underside
        ".......#BBB##BBBBBBB#FFFFFFFFFFFFFF##...........",  // 23: Chin curves under
        ".......#BBBBB##BBBBBBB#CCCCCCCCCCCC#............",  // 24: Chest curves out
        "......#BBBBBBB##BBBBBBB#CCCCCCCCCCCC#...........",  // 25: Belly forward
        ".....#BBBBBBBBB##BBBBBBB#CCCCCCCCCCCC#..........",  // 26: Plump round tummy
        ".....#BB#BBBBBBB#BBBBBBB#CCCSCCCCCCCC#..........",  // 27: Down chevrons
        "....#SB#B#BBBBBB##BBBBB#CCCCSSCCCCCCCC#.........",  // 28
        "....##L#B##BBBBB#DBBBBB#CCCCCCCCCCCCCC#.........",  // 29
        "....#D#SL#B#BBBB#DDBBBB#CCCCCCCCCCCCCC#.........",  // 30
        "....#DSS#BL#BBB#DDSBBBB#CCSCCCCCSCCCCC#.........",  // 31
        "....#DBSBB##BB#DDSSBBBB#CCSCCCCCSCCCCC#.........",  // 32
        "....##BSBL#BB#DDSSBBBBB#CCCCCCCCCCCCC#..........",  // 33: Lower belly
        ".....#B#L#BBB#DSSBBBBBBBCCCCSCCCCCCC#...........",  // 34
        ".....#L##SLBB#SBBBBBBBBBCCCCCSCCCCC#............",  // 35
        ".....##D##SLB#BBBBBBBBBBBCCCCCCCCC#.............",  // 36
        ".......#D##SLLBBBBBBBBBBBCCCCCCCC#..............",  // 37
        "........#D##SSLBBBBBBBBBBBBBCCCC#...............",  // 38: Rump & lower body
        ".........##D###SSLLBBBBBBBBLLSS#................",  // 39
        "...........####D###SSSSSSSS###D#................",  // 40
        "..................##........##..................",  // 41: Far leg, Near leg
        ".................#MM#......#OO#.................",  // 42: Far foot shaded, Near foot bright
        "................#MMMM#....#OOOO#................",  // 43
        "...............#DD##DD#..#MM##MM#...............",  // 44: Baseline contact on Y=44
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
            preview.Save("assets/preview_beautiful_profile_8x.png", ImageFormat.Png);
        }

        bmp.Save("assets/profile_beautiful_base.png", ImageFormat.Png);
        Console.WriteLine("Done! Created assets/preview_beautiful_profile_8x.png");
    }
}
