using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class PureProfileBuilder
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

    // Beautiful Pure 90° Profile: Facing Right
    // 1 Big expressive eye, cute hooked beak, swept-back tuft, folded wing on back, plump belly
    static readonly string[] BaseProfile = new string[]
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
        "..............#SLL##...########.................",  // 10: Crown top
        "............###SLLL##SBBBBBBBBS#................",  // 11
        "...........#LBSSSLLLL#BBBBBBBBBB#...............",  // 12
        "..........#SLLSSSSSLB#BBBBBBBBBBB#..............",  // 13: Forehead brow
        ".........########SBBSBBBBBBBBBBBBB#.............",  // 14
        "........#SLLFFFF##BBSSBBBBFFFFFFFFFF#...........",  // 15: Face disc starts
        ".......#SLLFFFF####BB##FFF###FFFFFFFF#..........",  // 16: Brow cushion over eye
        "......#SLLFFFFF#RR##FFFFF#RR##FFFFFFFF#.........",  // 17: Eye top
        ".....#SLLFFFFFF#RWKK#FFF#RWKK#FFFFFFFF#.........",  // 18: Big kind eye with glint
        "....#LLBFFFFFF#YKKK#FFF#YKKK#FFFFFF#MM##........",  // 19: Glint + beak base
        "....#LBBFFFFFF#YKKW#FFF#YKKW#FFFFF#MOOOO#.......",  // 20: Eye crescent + upper beak
        "....#LBBFFFFFF#ROOO#FFF#ROOO#FFFF#MOOOOO#.......",  // 21: Eye crescent + hooked beak tip
        "....#SLBBFFFFF####FFFFF####FFFFFF#MMMM##........",  // 22: Cheek + beak underside
        "....#SLBBBFFFFFFFFFFFFFFFFFFFFFFFF###...........",  // 23: Chin
        "....###LBBBFFFFBBCCCCCCCCCCCCCCCCC#.............",  // 24: Chest curve
        "...#BBB#LBBBBBBCCCCCCCCCCCCCCCCCCCC#............",  // 25: Belly curves forward
        "..#BBBBB#BBBCCCCCCCCCCCCCCCCCCCCCCC#............",  // 26
        "..#BBBBBB#BBCSCCLCCCCCCLCCCCCCCSCC#.............",  // 27: Down chevrons
        ".#SB#BBBB#BCCCSSCCBCCBCCSSCCCCCCCC#.............",  // 28
        ".##L#B#BB#BCCCCCCCCBBCCCCCCCCCCCC#..............",  // 29
        ".#D#SL#BL#BCCCCCCCCCCCCCCCCCCCCCC#..............",  // 30
        ".#DSS#BL#BCCSCCCSCCCCCCSCCCCCCSC#...............",  // 31
        ".#DBSBB##BCCCSCCCSCCCCSCCCSCCCC#................",  // 32
        ".##BSBL#BBBCCCCCCCCCCCCCCCCCCC#.................",  // 33: Lower belly
        "..#B#L#BBBBCCCCSCCCCCCCCSCCCCBB#................",  // 34
        "..#L##SLBBBCCCCCSCCCCCCSCCCCBB#.................",  // 35
        "..##D##SLBBBCCCCCCCSSCCCCCCBB#..................",  // 36
        "....#D##SLLBBBCCCCCCCCCCCCBBBL#.................",  // 37: Wing & tail tip
        ".....#D##SSLBBBBBBBBBBBBBBBB#...................",  // 38
        "......##D###SSLLBBBBBBLLSS#.....................",  // 39
        "........####D###SSSSSSSS##......................",  // 40
        "...............##........##.....................",  // 41: Far leg, Near leg
        "..............#MM#......#OO#....................",  // 42: Far shaded, Near bright
        ".............#MMMM#....#OOOO#...................",  // 43
        "............#DD##DD#..#MM##MM#..................",  // 44: Floor baseline Y=44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    static void Main()
    {
        // Wait! Let's check BaseProfile lines 17-22:
        // Notice in BaseProfile above, it STILL had "#RR##FFFFF#RR##" (two eyes)!
        // Let's replace lines 16-23 so there is STRICTLY ONE EYE in the center of the head!
        string[] grid = (string[])BaseProfile.Clone();

        grid[15] = ".........#SLLFFFF##BBSSBBBBFFFFFFFF#............";  // Brow
        grid[16] = "........#SLLFFFF####BB##FFFF###FFFF#............";  // Cream arch over eye
        grid[17] = ".......#SLLFFFFFFFFFFFFFFFF#RR##FFF#............";  // Socket top
        grid[18] = "......#SLLFFFFFFFFFFFFFFFF#RWKK#FFFF#...........";  // Upper eye: Glint W, Pupil K
        grid[19] = ".....#LLBFFFFFFFFFFFFFFFF#YKKK#FFFFF#MM##.......";  // Center eye: Pupil K, Beak base
        grid[20] = ".....#LBBFFFFFFFFFFFFFFFF#YKKW#FFFF#MOOOO#......";  // Lower eye: Glint W, Beak body
        grid[21] = ".....#LBBFFFFFFFFFFFFFFFF#ROOO#FFF#MOOOOO#......";  // Orange iris, Hooked beak
        grid[22] = ".....#SLBBFFFFFFFFFFFFFFFF####FFFF#MMMM##.......";  // Cheek cream, Beak underside
        grid[23] = ".....#SLBBBFFFFFFFFFFFFFFFFFFFFFFFF###..........";  // Chin curve
        grid[24] = ".....###LBBBFFFFBBCCCCCCCCCCCCCCCCC#............";  // Chest curve
        grid[25] = "....#BBB#LBBBBBBCCCCCCCCCCCCCCCCCCCC#...........";  // Belly forward
        grid[26] = "...#BBBBB#BBBCCCCCCCCCCCCCCCCCCCCCCC#...........";  // Plump tummy
        grid[27] = "...#BBBBBB#BBCSCCLCCCCCCLCCCCCCCSCC#............";  // Chevrons
        grid[28] = "..#SB#BBBB#BCCCSSCCBCCBCCSSCCCCCCCC#............";
        grid[29] = "..##L#B#BB#BCCCCCCCCBBCCCCCCCCCCCC#.............";
        grid[30] = "..#D#SL#BL#BCCCCCCCCCCCCCCCCCCCCCC#.............";
        grid[31] = "..#DSS#BL#BCCSCCCSCCCCCCSCCCCCCSC#..............";
        grid[32] = "..#DBSBB##BCCCSCCCSCCCCSCCCSCCCC#...............";
        grid[33] = "..##BSBL#BBBCCCCCCCCCCCCCCCCCCC#................";
        grid[34] = "...#B#L#BBBBCCCCSCCCCCCCCSCCCCBB#...............";
        grid[35] = "...#L##SLBBBCCCCCSCCCCCCSCCCCBB#................";
        grid[36] = "...##D##SLBBBCCCCCCCSSCCCCCCBB#.................";
        grid[37] = ".....#D##SLLBBBCCCCCCCCCCCCBBBL#................";
        grid[38] = "......#D##SSLBBBBBBBBBBBBBBBB#..................";
        grid[39] = ".......##D###SSLLBBBBBBLLSS#....................";
        grid[40] = ".........####D###SSSSSSSS##.....................";
        grid[41] = "................##........##....................";
        grid[42] = "...............#MM#......#OO#...................";
        grid[43] = "..............#MMMM#....#OOOO#..................";
        grid[44] = ".............#DD##DD#..#MM##MM#.................";

        // Verify row lengths
        for (int y = 0; y < 48; y++)
        {
            if (grid[y].Length != 48)
                throw new Exception(string.Format("Row {0} length is {1}, expected 48: '{2}'", y, grid[y].Length, grid[y]));
        }

        Bitmap bmp = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
        for (int y = 0; y < 48; y++)
        {
            for (int x = 0; x < 48; x++)
            {
                char c = grid[y][x];
                bmp.SetPixel(x, y, Palette.ContainsKey(c) ? Palette[c] : Color.Transparent);
            }
        }

        // Render 8x magnified preview
        using (Bitmap preview = new Bitmap(384, 384, PixelFormat.Format32bppArgb))
        {
            using (Graphics g = Graphics.FromImage(preview))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(bmp, 0, 0, 384, 384);
            }
            preview.Save("assets/preview_pure_profile_8x.png", ImageFormat.Png);
        }

        bmp.Save("assets/profile_pure_base.png", ImageFormat.Png);
        Console.WriteLine("Done! Created assets/preview_pure_profile_8x.png");
    }
}
