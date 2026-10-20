using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class CompareProfiles
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

    // 1. TRUE 90° PROFILE (1 Big Eye, Beak on right silhouette, Wing on left)
    static readonly string[] TrueProfile = new string[]
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
        "................#SLL##..#######.................",  // 10
        "..............###SLLL##SBBBBBBS##...............",  // 11
        ".............#LBSSSLLLL#BBBBBBBB#...............",  // 12: Forehead crown
        "............#SLLSSSSSLB#BBBBBBBBB#..............",  // 13
        "...........########SBBSBBBBBBBBBBB#.............",  // 14
        "..........#SLLFFFF##BBSSBBFFFFFFFFF#............",  // 15: Face disc
        ".........#SLLFFFF####BB##FFFF###FFFF#...........",  // 16: Cream cushion over eye
        "........#SLLFFFFF#RR##FFFFFFF#RWKK#FF#..........",  // 17: Eye socket top
        ".......#SLLFFFFFF#RWKK#FFFFFF#KKKY#FFF#.........",  // 18: Big glowing eye (facing right)
        "......#LLBFFFFFFF#YKKK#FFFFFF#YKKW#FFF#MMM#.....",  // 19: Glint + upper beak root
        "......#LBBFFFFFFF#YKKW#FFFFFF#ROOO#FFF#OOOO#....",  // 20: Lower iris + beak body
        "......#LBBFFFFFFF#ROOO#FFFFFF####FFFF#OOOOO#....",  // 21: Eye lower arc + beak tip
        "......#SLBBFFFFFF####FFFFFFFFFFFFFFFF#MMMM#.....",  // 22: Soft cream cheek + beak shadow
        "......#SLBBBFFFFFFFFFFFFFFFFFFFFFFFFF##.........",  // 23: Chin
        "......###LBBBFFFFBBCCCCCCCCCCCCCCCCCCCC#........",  // 24: Chest curves out
        ".....#BBB#LBBBBBBCCCCCCCCCCCCCCCCCCCCCCC#.......",  // 25
        "....#BBBBB#BBBCCCCCCCCCCCCCCCCCCCCCCCCCCC#......",  // 26: Plump belly forward
        "....#BBBBBB#BBCSCCLCCCCCCLCCCCCCLCCCSCCCCB#.....",  // 27: Chevrons
        "...#SB#BBBB#BCCCSSCCBCCBCCSSCCCCCCSSCCCCCB#.....",  // 28
        "...##L#B#BB#BCCCCCCCCBBCCCCCCCCCCCCCCCCCCB#.....",  // 29
        "...#D#SL#BL#BCCCCCCCCCCCCCCCCCCCCCCCCCCCCB#.....",  // 30
        "...#DSS#BL#BCCSCCCSCCCCCCSCCCSCCCCSCCCSCCB#.....",  // 31
        "...#DBSBB##BCCCSCCCSCCCCSCCCSCCCCSCCCSCCCB#.....",  // 32
        "...##BSBL#BBBCCCCCCCCCCCCCCCCCCCCCCCCCCBBB#.....",  // 33: Lower belly
        "....#B#L#BBBBCCCCSCCCCCCCCSCCCCCCCCCCCCBB#......",  // 34
        "....#L##SLBBBCCCCCSCCCCCCSCCCCCCCSCCCCBB#.......",  // 35
        "....##D##SLBBBCCCCCCCSSCCCCCCCCCSSCCCBB#........",  // 36
        "......#D##SLLBBBCCCCCCCCCCCCCCBBBBBLLS#.........",  // 37
        ".......#D##SSLBBBBBBBBBBBBBBBBLSS###D#..........",  // 38
        "........##D###SSLLBBBBBBBBLLSS###D##............",  // 39
        "..........####D###SSSSSSSS###D####..............",  // 40
        ".................##.........##..................",  // 41: Legs
        "................#MM#.......#OO#.................",  // 42: Far foot shaded, Near foot bright
        "...............#MMMM#.....#OOOO#................",  // 43
        "..............#DD##DD#...#MM##MM#...............",  // 44: Baseline Y=44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    // 2. 3/4 PERSPECTIVE PROFILE (Near eye big, far eye foreshortened, beak between them, body angled)
    static readonly string[] ThreeQuarterProfile = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "..........##......................##............",  // 07: Tufts angled
        "..........#L##..................##L#............",  // 08
        "..........#SLL#................#LLS#............",  // 09
        "...........#SLL##..##########.##LLS#............",  // 10
        ".........####SLLL##SBBBBBBBBS#LLLS####..........",  // 11
        ".........#LBSSSLLLL##BBBBBB##LLLLSSSBL#.........",  // 12
        "..........#SLLSSSSSLB#BBBB#BLSSSSSLLS#..........",  // 13
        "...........########SBBSBBSBBS########...........",  // 14
        "...........#SLLFFFF##BBSSBB##FFFFLLS#...........",  // 15
        "..........#SLLFF##FF#BBBB#FFF###FFLLS#..........",  // 16
        ".........#SLLFF#RR#FFLBBLFFF#RR##FFLLS#.........",  // 17
        "........#SLLFFF#WKK#FFLLFFF#RWKK#FFFLLS#........",  // 18: Far eye compressed (WKK), Near eye full (RWKK)
        "........#LLBFFF#KKK#FFFFFF#YKKKK#FFFBLL#........",  // 19: Far eye (KKK), Near eye (YKKKK)
        "........#LBBFFF#KKW#FFMMFF#YKKKW#FFFBBL#........",  // 20: Both glints W visible
        "........#LBBFFF#ROO#FMOOMF#ROOOR#FFFBBL#..##....",  // 21: Beak begins angling out
        "........#SLBBFFF###FFMOOMFF#####FFFBBLS#.###....",  // 22: Beak tip extends slightly right
        "........#SLBBBFFFFFFF#MM#BFFFFFFFFBBBLS#OO#.....",  // 23: Beak contour
        "........###LBBBFFFFBBCC##CCBBFFFFBBBL###M#......",  // 24
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

    static Bitmap Render(string[] grid)
    {
        Bitmap bmp = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
        for (int y = 0; y < 48; y++)
        {
            string row = grid[y];
            for (int x = 0; x < 48; x++)
            {
                char c = row[x];
                bmp.SetPixel(x, y, Palette.ContainsKey(c) ? Palette[c] : Color.Transparent);
            }
        }
        return bmp;
    }

    static void Main()
    {
        Bitmap b1 = Render(TrueProfile);
        Bitmap b2 = Render(ThreeQuarterProfile);

        // Save side by side 8x comparison (96x48 * 8 = 768 x 384)
        using (Bitmap comp = new Bitmap(768, 384, PixelFormat.Format32bppArgb))
        {
            using (Graphics g = Graphics.FromImage(comp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(b1, 0, 0, 384, 384);
                g.DrawImage(b2, 384, 0, 384, 384);
            }
            comp.Save("assets/preview_profile_comparison_8x.png", ImageFormat.Png);
        }

        b1.Save("assets/profile_true.png", ImageFormat.Png);
        b2.Save("assets/profile_34.png", ImageFormat.Png);

        Console.WriteLine("Done! Created assets/preview_profile_comparison_8x.png");
    }
}
