using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class SingleEyeProfileTest
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

    static readonly string[] ProfileSingleEye = new string[]
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
        "...............#SLL#............................",  // 09
        "................#SLL##...########...............",  // 10: Forehead top
        "..............###SLLL##SBBBBBBBBS#..............",  // 11
        ".............#LBSSSLLLL#BBBBBBBBBB#.............",  // 12
        "............#SLLSSSSSLB#BBBBBBBBBBB#............",  // 13
        "...........########SBBSBBBBBBBBBBBBB#...........",  // 14
        "...........#SLLFFFF##BBSSBBFFFFFFFFFF#..........",  // 15: Face disc starts
        "..........#SLLFFFF####BB##FFFF###FFFF#..........",  // 16: Brow over single eye
        ".........#SLLFFFFF#RR##FFFFFF#RR##FFFF#.........",  // 17: Iris top
        "........#SLLFFFFFF#RWKK#FFFF#RWKK#FFFF#.........",  // 18: Big kind eye with glint
        "........#LLBFFFFFF#YKKK#FFFF#YKKK#FFFF#MM##.....",  // 19: Glint + beak root
        "........#LBBFFFFFF#YKKW#FFFF#YKKW#FFF#MOOOO#....",  // 20: Eye crescent + beak body
        "........#LBBFFFFFF#ROOO#FFFF#ROOO#FFF#MOOOO#....",  // 21: Iris crescent + beak tip
        "........#SLBBFFFFF####FFFFFFF####FFFFF#MM##.....",  // 22: Lower eye arc + beak shadow
        "........#SLBBBFFFFFFFFFFFFFFFFFFFFFFFF#.........",  // 23: Chin
        "........###LBBBFFFFBBCCCCCCCCCCCCCCCCC#.........",  // 24: Breast curve
        ".......#BBB#LBBBBBBCCCCCCCCCCCCCCCCCCCC#........",  // 25: Belly forward
        "......#BBBBB#BBBCCCCCCCCCCCCCCCCCCCCCCC#........",  // 26
        "......#BBBBBB#BBCSCCLCCCCCCLCCCCCCCSCC#.........",  // 27: Down chevrons
        ".....#SB#BBBB#BCCCSSCCBCCBCCSSCCCCCCCC#.........",  // 28
        ".....##L#B#BB#BCCCCCCCCBBCCCCCCCCCCCC#..........",  // 29
        ".....#D#SL#BL#BCCCCCCCCCCCCCCCCCCCCCC#..........",  // 30
        ".....#DSS#BL#BCCSCCCSCCCCCCSCCCCCCSC#...........",  // 31
        ".....#DBSBB##BCCCSCCCSCCCCSCCCSCCCC#............",  // 32
        ".....##BSBL#BBBCCCCCCCCCCCCCCCCCCC#.............",  // 33: Lower belly
        "......#B#L#BBBBCCCCSCCCCCCCCSCCCCBB#............",  // 34
        "......#L##SLBBBCCCCCSCCCCCCSCCCCBB#.............",  // 35
        "......##D##SLBBBCCCCCCCSSCCCCCCBB#..............",  // 36
        "........#D##SLLBBBCCCCCCCCCCCCBBBL#.............",  // 37
        ".........#D##SSLBBBBBBBBBBBBBBBB#...............",  // 38
        "..........##D###SSLLBBBBBBLLSS#.................",  // 39
        "............####D###SSSSSSSS##..................",  // 40
        "...................##........##.................",  // 41: Far leg, Near leg
        "..................#MM#......#OO#................",  // 42: Far shaded, Near bright
        ".................#MMMM#....#OOOO#...............",  // 43
        "................#DD##DD#..#MM##MM#..............",  // 44: Floor Y=44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    static void Main()
    {
        // Let's create an exact single eye grid:
        // Notice in ProfileSingleEye lines 17-22 above, it STILL had two eyes: #RR## and #RWKK# twice!
        // Let's replace the eye section so there is strictly ONE eye!
        string[] singleEyeGrid = (string[])ProfileSingleEye.Clone();
        singleEyeGrid[16] = "..........#SLLFFFF####BBBB####FFFF###FF#........";
        singleEyeGrid[17] = ".........#SLLFFFFFFFFFFFFFFFF#RR##FFFFF#........";
        singleEyeGrid[18] = "........#SLLFFFFFFFFFFFFFFFF#RWKK#FFFFFF#.......";
        singleEyeGrid[19] = "........#LLBFFFFFFFFFFFFFFFF#YKKK#FFFFFF#MM##...";
        singleEyeGrid[20] = "........#LBBFFFFFFFFFFFFFFFF#YKKW#FFFFF#MOOOO#..";
        singleEyeGrid[21] = "........#LBBFFFFFFFFFFFFFFFF#ROOO#FFFFF#MOOOO#..";
        singleEyeGrid[22] = "........#SLBBFFFFFFFFFFFFFFFF####FFFFFF#MM##....";

        Bitmap bmp = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
        for (int y = 0; y < 48; y++)
        {
            string row = singleEyeGrid[y];
            if (row.Length != 48) throw new Exception(string.Format("Row {0} length is {1}", y, row.Length));
            for (int x = 0; x < 48; x++)
            {
                char c = row[x];
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
            preview.Save("assets/preview_single_eye_profile_8x.png", ImageFormat.Png);
        }

        Console.WriteLine("Done! Created assets/preview_single_eye_profile_8x.png");
    }
}
