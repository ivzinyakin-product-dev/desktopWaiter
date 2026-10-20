using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class ProfileWalkGenerator
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

    // Profile Frame 0: Contact A (Facing Right. Near leg forward, Far leg back, both on floor Y=44)
    static readonly string[] Frame0 = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "..................##............................",  // 07: Ear tuft
        "..................#L##.......##.................",  // 08: Near & far tuft tips
        "..................#SLL#.....#LL#................",  // 09
        "...................#SLL##...#LLS#...............",  // 10
        "................####SLLL##..#LLLS#..............",  // 11
        "...............#LBSSSLLLL####BBBB#..............",  // 12: Head crown
        "..............#SLLSSSSSLB#BBBBBBB#..............",  // 13
        ".............########SBBSBBBBBBBBB#.............",  // 14
        "............#SLLFFFF##BBSSBBFFFFFF##............",  // 15: Face disc
        "...........#SLLFFFF####BB##FFF###FFF#...........",  // 16: Brow arch
        "..........#SLLFFFFF#RR##FFFFF#RWKK#FF#..........",  // 17: Iris top
        ".........#SLLFFFFFF#RWKK#FFFF#YKKK#FFF#.........",  // 18: Big expressive kind eye
        "........#LLBFFFFFFF#YKKK#FFFF#YKKW#FFF#MM##.....",  // 19: Glints & upper beak
        "........#LBBFFFFFFF#YKKW#FFFF#ROOO#FF#MOOOO#....",  // 20: Eye crescent & beak body
        "........#LBBFFFFFFF#ROOO#FFFFF####FFF#MOOOO#....",  // 21: Eye lower curve & beak tip
        "........#SLBBFFFFFFF####FFFFFFFFFFFFFF#MM##.....",  // 22: Cheek & beak shadow
        "........#SLBBBFFFFFFFFFFFFFFFFFFFFFFFF#.........",  // 23: Chin
        "........###LBBBFFFFBBCCCCCCCCCCCCCCCCC#.........",  // 24: Upper breast curve
        ".......#BBB#LBBBBBBCCCCCCCCCCCCCCCCCCCC#........",  // 25: Belly curves outward
        "......#BBBBB#BBBCCCCCCCCCCCCCCCCCCCCCCC#........",  // 26
        "......#BBBBBB#BBCSCCLCCCCCCLCCCCCCCSCC#.........",  // 27: Fluffy belly chevrons
        ".....#SB#BBBB#BCCCSSCCBCCBCCSSCCCCCCCC#.........",  // 28
        ".....##L#B#BB#BCCCCCCCCBBCCCCCCCCCCCC#..........",  // 29
        ".....#D#SL#BL#BCCCCCCCCCCCCCCCCCCCCCC#..........",  // 30
        ".....#DSS#BL#BCCSCCCSCCCCCCSCCCCCCSC#...........",  // 31
        ".....#DBSBB##BCCCSCCCSCCCCSCCCSCCCC#............",  // 32
        ".....##BSBL#BBBCCCCCCCCCCCCCCCCCCC#.............",  // 33: Lower belly
        "......#B#L#BBBBCCCCSCCCCCCCCSCCCCB#.............",  // 34
        "......#L##SLBBBCCCCCSCCCCCCSCCCCCB#.............",  // 35
        "......##D##SLBBBCCCCCCCSSCCCCCCCBB#.............",  // 36
        "........#D##SLLBBBCCCCCCCCCCCCBBBL#.............",  // 37
        ".........#D##SSLBBBBBBBBBBBBBBBB#...............",  // 38
        "..........##D###SSLLBBBBBBBBLLSS#...............",  // 39
        "............####D###SSSSSSSS###D#...............",  // 40
        "...................##........##.................",  // 41: Far leg back (X=19-20), Near leg forward (X=29-30)
        "..................#MM#......#OO#................",  // 42: Far leg shaded, Near leg bright
        ".................#MMMM#....#OOOO#...............",  // 43
        "................#DD##DD#..#MM##MM#..............",  // 44: Both feet on floor Y=44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    // Profile Frame 1: Passing A (Facing Right. Near leg supports at Y=44, Far leg LIFTED in air at Y=41..43, body dips 1px)
    static readonly string[] Frame1 = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "..................##............................",  // 07
        "..................#L##.......##.................",  // 08
        "..................#SLL#.....#LL#................",  // 09
        "...................#SLL##...#LLS#...............",  // 10
        "................####SLLL##..#LLLS#..............",  // 11
        "...............#LBSSSLLLL####BBBB#..............",  // 12
        "..............#SLLSSSSSLB#BBBBBBB#..............",  // 13
        ".............########SBBSBBBBBBBBB#.............",  // 14
        "............#SLLFFFF##BBSSBBFFFFFF##............",  // 15
        "...........#SLLFFFF####BB##FFF###FFF#...........",  // 16
        "..........#SLLFFFFF#RR##FFFFF#RWKK#FF#..........",  // 17
        ".........#SLLFFFFFF#RWKK#FFFF#YKKK#FFF#.........",  // 18
        "........#LLBFFFFFFF#YKKK#FFFF#YKKW#FFF#MM##.....",  // 19
        "........#LBBFFFFFFF#YKKW#FFFF#ROOO#FF#MOOOO#....",  // 20
        "........#LBBFFFFFFF#ROOO#FFFFF####FFF#MOOOO#....",  // 21
        "........#SLBBFFFFFFF####FFFFFFFFFFFFFF#MM##.....",  // 22
        "........#SLBBBFFFFFFFFFFFFFFFFFFFFFFFF#.........",  // 23
        "........###LBBBFFFFBBCCCCCCCCCCCCCCCCC#.........",  // 24
        ".......#BBB#LBBBBBBCCCCCCCCCCCCCCCCCCCC#........",  // 25
        "......#BBBBB#BBBCCCCCCCCCCCCCCCCCCCCCCC#........",  // 26
        "......#BBBBBB#BBCSCCLCCCCCCLCCCCCCCSCC#.........",  // 27
        ".....#SB#BBBB#BCCCSSCCBCCBCCSSCCCCCCCC#.........",  // 28
        ".....##L#B#BB#BCCCCCCCCBBCCCCCCCCCCCC#..........",  // 29
        ".....#D#SL#BL#BCCCCCCCCCCCCCCCCCCCCCC#..........",  // 30
        ".....#DSS#BL#BCCSCCCSCCCCCCSCCCCCCSC#...........",  // 31
        ".....#DBSBB##BCCCSCCCSCCCCSCCCSCCCC#............",  // 32
        ".....##BSBL#BBBCCCCCCCCCCCCCCCCCCC#.............",  // 33
        "......#B#L#BBBBCCCCSCCCCCCCCSCCCCBB#............",  // 34
        "......#L##SLBBBCCCCCSCCCCCCSCCCCBB#.............",  // 35
        "......##D##SLBBBCCCCCCCSSCCCCCCBB#..............",  // 36
        "........#D##SLLBBBCCCCCCCCCCCCBBBL#.............",  // 37
        ".........#D##SSLBBBBBBBBBBBBBBBB#...............",  // 38
        "..........##D###SSLLBBBBBBLLSS#.................",  // 39
        "............####D###SSSSSSSS##..................",  // 40: Body bob
        ".......................#MM#......##.............",  // 41: Far leg LIFTED swinging forward (Y=41)
        "......................#MMMM#....#OO#............",  // 42: Far leg lifted (Y=42)
        ".....................#DD##DD#..#OOOO#...........",  // 43: Far claws lifted (Y=43)
        "..............................#MM##MM#..........",  // 44: Near leg ON FLOOR Y=44, Far leg IN AIR!
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    // Profile Frame 2: Contact B (Facing Right. Far leg forward, Near leg back, both on floor Y=44)
    static readonly string[] Frame2 = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "..................##............................",  // 07
        "..................#L##.......##.................",  // 08
        "..................#SLL#.....#LL#................",  // 09
        "...................#SLL##...#LLS#...............",  // 10
        "................####SLLL##..#LLLS#..............",  // 11
        "...............#LBSSSLLLL####BBBB#..............",  // 12
        "..............#SLLSSSSSLB#BBBBBBB#..............",  // 13
        ".............########SBBSBBBBBBBBB#.............",  // 14
        "............#SLLFFFF##BBSSBBFFFFFF##............",  // 15
        "...........#SLLFFFF####BB##FFF###FFF#...........",  // 16
        "..........#SLLFFFFF#RR##FFFFF#RWKK#FF#..........",  // 17
        ".........#SLLFFFFFF#RWKK#FFFF#YKKK#FFF#.........",  // 18
        "........#LLBFFFFFFF#YKKK#FFFF#YKKW#FFF#MM##.....",  // 19
        "........#LBBFFFFFFF#YKKW#FFFF#ROOO#FF#MOOOO#....",  // 20
        "........#LBBFFFFFFF#ROOO#FFFFF####FFF#MOOOO#....",  // 21
        "........#SLBBFFFFFFF####FFFFFFFFFFFFFF#MM##.....",  // 22
        "........#SLBBBFFFFFFFFFFFFFFFFFFFFFFFF#.........",  // 23
        "........###LBBBFFFFBBCCCCCCCCCCCCCCCCC#.........",  // 24
        ".......#BBB#LBBBBBBCCCCCCCCCCCCCCCCCCCC#........",  // 25
        "......#BBBBB#BBBCCCCCCCCCCCCCCCCCCCCCCC#........",  // 26
        "......#BBBBBB#BBCSCCLCCCCCCLCCCCCCCSCC#.........",  // 27
        ".....#SB#BBBB#BCCCSSCCBCCBCCSSCCCCCCCC#.........",  // 28
        ".....##L#B#BB#BCCCCCCCCBBCCCCCCCCCCCC#..........",  // 29
        ".....#D#SL#BL#BCCCCCCCCCCCCCCCCCCCCCC#..........",  // 30
        ".....#DSS#BL#BCCSCCCSCCCCCCSCCCCCCSC#...........",  // 31
        ".....#DBSBB##BCCCSCCCSCCCCSCCCSCCCC#............",  // 32
        ".....##BSBL#BBBCCCCCCCCCCCCCCCCCCC#.............",  // 33
        "......#B#L#BBBBCCCCSCCCCCCCCSCCCCBB#............",  // 34
        "......#L##SLBBBCCCCCSCCCCCCSCCCCBB#.............",  // 35
        "......##D##SLBBBCCCCCCCSSCCCCCCBB#..............",  // 36
        "........#D##SLLBBBCCCCCCCCCCCCBBBL#.............",  // 37
        ".........#D##SSLBBBBBBBBBBBBBBBB#...............",  // 38
        "..........##D###SSLLBBBBBBLLSS#.................",  // 39
        "............####D###SSSSSSSS##..................",  // 40
        "...................##........##.................",  // 41: Near leg back (X=19-20), Far leg forward (X=29-30)
        "..................#OO#......#MM#................",  // 42: Near leg back, Far leg forward
        ".................#OOOO#....#MMMM#...............",  // 43
        "................#MM##MM#..#DD##DD#..............",  // 44: Both on floor Y=44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    // Profile Frame 3: Passing B (Facing Right. Far leg supports at Y=44, Near leg LIFTED in air at Y=41..43)
    static readonly string[] Frame3 = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "..................##............................",  // 07
        "..................#L##.......##.................",  // 08
        "..................#SLL#.....#LL#................",  // 09
        "...................#SLL##...#LLS#...............",  // 10
        "................####SLLL##..#LLLS#..............",  // 11
        "...............#LBSSSLLLL####BBBB#..............",  // 12
        "..............#SLLSSSSSLB#BBBBBBB#..............",  // 13
        ".............########SBBSBBBBBBBBB#.............",  // 14
        "............#SLLFFFF##BBSSBBFFFFFF##............",  // 15
        "...........#SLLFFFF####BB##FFF###FFF#...........",  // 16
        "..........#SLLFFFFF#RR##FFFFF#RWKK#FF#..........",  // 17
        ".........#SLLFFFFFF#RWKK#FFFF#YKKK#FFF#.........",  // 18
        "........#LLBFFFFFFF#YKKK#FFFF#YKKW#FFF#MM##.....",  // 19
        "........#LBBFFFFFFF#YKKW#FFFF#ROOO#FF#MOOOO#....",  // 20
        "........#LBBFFFFFFF#ROOO#FFFFF####FFF#MOOOO#....",  // 21
        "........#SLBBFFFFFFF####FFFFFFFFFFFFFF#MM##.....",  // 22
        "........#SLBBBFFFFFFFFFFFFFFFFFFFFFFFF#.........",  // 23
        "........###LBBBFFFFBBCCCCCCCCCCCCCCCCC#.........",  // 24
        ".......#BBB#LBBBBBBCCCCCCCCCCCCCCCCCCCC#........",  // 25
        "......#BBBBB#BBBCCCCCCCCCCCCCCCCCCCCCCC#........",  // 26
        "......#BBBBBB#BBCSCCLCCCCCCLCCCCCCCSCC#.........",  // 27
        ".....#SB#BBBB#BCCCSSCCBCCBCCSSCCCCCCCC#.........",  // 28
        ".....##L#B#BB#BCCCCCCCCBBCCCCCCCCCCCC#..........",  // 29
        ".....#D#SL#BL#BCCCCCCCCCCCCCCCCCCCCCC#..........",  // 30
        ".....#DSS#BL#BCCSCCCSCCCCCCSCCCCCCSC#...........",  // 31
        ".....#DBSBB##BCCCSCCCSCCCCSCCCSCCCC#............",  // 32
        ".....##BSBL#BBBCCCCCCCCCCCCCCCCCCC#.............",  // 33
        "......#B#L#BBBBCCCCSCCCCCCCCSCCCCBB#............",  // 34
        "......#L##SLBBBCCCCCSCCCCCCSCCCCBB#.............",  // 35
        "......##D##SLBBBCCCCCCCSSCCCCCCBB#..............",  // 36
        "........#D##SLLBBBCCCCCCCCCCCCBBBL#.............",  // 37
        ".........#D##SSLBBBBBBBBBBBBBBBB#...............",  // 38
        "..........##D###SSLLBBBBBBLLSS#.................",  // 39
        "............####D###SSSSSSSS##..................",  // 40
        ".......................#OO#......##.............",  // 41: Near leg LIFTED swinging forward (Y=41)
        "......................#OOOO#....#MM#............",  // 42: Near leg lifted (Y=42)
        ".....................#MM##MM#..#MMMM#...........",  // 43: Near claws lifted (Y=43)
        "..............................#DD##DD#..........",  // 44: Far leg ON FLOOR Y=44, Near leg IN AIR!
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    static Bitmap RenderGrid(string[] grid)
    {
        if (grid.Length != 48)
            throw new Exception("Grid rows != 48!");

        Bitmap bmp = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
        for (int y = 0; y < 48; y++)
        {
            string row = grid[y];
            if (row.Length != 48)
                throw new Exception(string.Format("Row {0} length is {1}, expected 48: '{2}'", y, row.Length, row));

            for (int x = 0; x < 48; x++)
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
        return bmp;
    }

    static void Main()
    {
        Console.WriteLine("Generating Profile WALK prototype (4 frames, facing right)...");

        string baseDir = Directory.Exists("assets") ? Directory.GetCurrentDirectory() : AppDomain.CurrentDomain.BaseDirectory;
        string outDir = Path.Combine(baseDir, @"assets\sprites\walk_profile");
        Directory.CreateDirectory(outDir);

        var frames = new Dictionary<string, string[]>()
        {
            { "frame_0.png", Frame0 },
            { "frame_1.png", Frame1 },
            { "frame_2.png", Frame2 },
            { "frame_3.png", Frame3 }
        };

        var bitmaps = new List<Bitmap>();

        foreach (var kvp in frames)
        {
            Bitmap bmp = RenderGrid(kvp.Value);
            string fullPath = Path.Combine(outDir, kvp.Key);
            bmp.Save(fullPath, ImageFormat.Png);
            Console.WriteLine("Saved: {0}", fullPath);
            bitmaps.Add(bmp);
        }

        // Render combined 8x preview strip (4 frames side-by-side: 1536x384)
        int scale = 8;
        int frameW = 48 * scale;
        int frameH = 48 * scale;
        int totalW = frameW * bitmaps.Count;
        int totalH = frameH;

        using (Bitmap strip = new Bitmap(totalW, totalH, PixelFormat.Format32bppArgb))
        {
            using (Graphics g = Graphics.FromImage(strip))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                for (int i = 0; i < bitmaps.Count; i++)
                {
                    g.DrawImage(bitmaps[i], i * frameW, 0, frameW, frameH);
                }
            }

            string stripPath = Path.Combine(baseDir, @"assets\preview_profile_walk_8x.png");
            strip.Save(stripPath, ImageFormat.Png);
            Console.WriteLine("Saved 8x Profile WALK strip preview to: {0}", stripPath);
        }

        // Clean up
        foreach (var bmp in bitmaps)
        {
            bmp.Dispose();
        }

        Console.WriteLine("[OK] Profile WALK Animation Set generated successfully!");
    }
}
