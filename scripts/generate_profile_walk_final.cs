using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class ProfileWalkFinalGenerator
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

    // Frame 0: Contact A (Near leg forward, Far leg back, both on floor Y=44)
    static readonly string[] Frame0 = new string[]
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
        "...............#SLL##...########................",  // 10: Crown
        ".............###SLLL##SBBBBBBBBS#...............",  // 11
        "............#LBSSLLL#BBBBBBBBBBBB#..............",  // 12
        "...........#SLLSSSL#BBBBBBBBBBBBBB#.............",  // 13: Forehead curves forward
        "..........#SLLSSS#BBBBBBSSBBBBBBBBB#............",  // 14: Forehead
        ".........#SLLFF#BBBBBBBBB##BB###BBBB#...........",  // 15: Brow arch
        "........#SLLF#BBBBBBBBBB#FFFF###FFFF#...........",  // 16: Cream cushion
        ".......#SLLF#BBBBBBBBB#FFF#RR##FFFFFF#..........",  // 17: Eye top
        "......#SLLF#BBBBBBBBB#FFF#RWKK#FFFFFF#..........",  // 18: Big kind eye with glint
        "......#LLB#BBBBBBBBB#FFF#YKKK#FFFFFF##..........",  // 19: Pupil K
        "......#LBB#BBBBBBBBB#FFF#YKKW#FFFFFF#OO#........",  // 20: Secondary glint W + beak base
        "......#LBB#BBBBBBBBB#FFF#ROOO#FFFFF#MOOO#.......",  // 21: Orange iris + beak body
        "......#SLB#BBBBBBBBBB#FFF####FFFFFF#MMO#........",  // 22: Cheek + beak hook
        "......#SLB##BBBBBBBBBB#FFFFFFFFFFFF##M#.........",  // 23: Chin connects
        ".......#BBB##BBBBBBBBBB#FFFFFFFFFFF##...........",  // 24: Throat
        ".......#BBBBB##BBBBBBBBB#CCCCCCCCCCCC#..........",  // 25: Chest curves forward
        "......#BBBBBBB##BBBBBBBBB#CCCCCCCCCCCC#.........",  // 26: Plump round tummy
        ".....#BBBBBBBBB##BBBBBBBBB#CCCCCCCCCCC#.........",  // 27
        ".....#BB#BBBBBBB#BBBBBBBBB#CCCSCCCCCCCC#........",  // 28: Down chevrons
        "....#SB#B#BBBBBB##BBBBBBB#CCCCSSCCCCCCB#........",  // 29
        "....##L#B##BBBBB#DBBBBBB#CCCCCCCCCCCCB#.........",  // 30
        "....#D#SL#B#BBBB#DDBBBB#CCCCCCCCCCCCB#..........",  // 31
        "....#DSS#BL#BBB#DDSBBBB#CCSCCCCCSCCCB#..........",  // 32: Wing layers
        "....#DBSBB##BB#DDSSBBBB#CCSCCCCCSCCC#...........",  // 33
        "....##BSBL#BB#DDSSBBBBB#CCCCCCCCCCCC#...........",  // 34: Lower tummy
        ".....#B#L#BBB#DSSBBBBBBBCCCCSCCCCCCC#...........",  // 35
        ".....#L##SLBB#SBBBBBBBBBCCCCCSCCCCC#............",  // 36
        ".....##D##SLB#BBBBBBBBBBBCCCCCCCCC#.............",  // 37
        ".......#D##SLLBBBBBBBBBBBCCCCCCCC#..............",  // 38
        "........#D##SSLBBBBBBBBBBBBBC###................",  // 39: Tail tuck
        ".........##D###SSLLBBBBBBLLSS#..................",  // 40
        "...........####D###SSSSSSSS##...................",  // 41: Body bottom
        ".................##.........##..................",  // 42: Far leg back (X=17-18), Near leg forward (X=28-29)
        "................#MM#.......#OO#.................",  // 43: Far foot shaded, Near foot bright
        "...............#DD##DD#...#MM##MM#..............",  // 44: Both on floor Y=44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    // Frame 1: Passing A (Near leg supporting at Y=44, Far leg LIFTED at Y=41..43, body 1px step bob)
    static readonly string[] Frame1 = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "...............##...............................",  // 07
        "..............#L##..............................",  // 08
        "..............#SLL#.............................",  // 09
        "...............#SLL##...########................",  // 10
        ".............###SLLL##SBBBBBBBBS#...............",  // 11
        "............#LBSSLLL#BBBBBBBBBBBB#..............",  // 12
        "...........#SLLSSSL#BBBBBBBBBBBBBB#.............",  // 13
        "..........#SLLSSS#BBBBBBSSBBBBBBBBB#............",  // 14
        ".........#SLLFF#BBBBBBBBB##BB###BBBB#...........",  // 15
        "........#SLLF#BBBBBBBBBB#FFFF###FFFF#...........",  // 16
        ".......#SLLF#BBBBBBBBB#FFF#RR##FFFFFF#..........",  // 17
        "......#SLLF#BBBBBBBBB#FFF#RWKK#FFFFFF#..........",  // 18
        "......#LLB#BBBBBBBBB#FFF#YKKK#FFFFFF##..........",  // 19
        "......#LBB#BBBBBBBBB#FFF#YKKW#FFFFFF#OO#........",  // 20
        "......#LBB#BBBBBBBBB#FFF#ROOO#FFFFF#MOOO#.......",  // 21
        "......#SLB#BBBBBBBBBB#FFF####FFFFFF#MMO#........",  // 22
        "......#SLB##BBBBBBBBBB#FFFFFFFFFFFF##M#.........",  // 23
        ".......#BBB##BBBBBBBBBB#FFFFFFFFFFF##...........",  // 24
        ".......#BBBBB##BBBBBBBBB#CCCCCCCCCCCC#..........",  // 25
        "......#BBBBBBB##BBBBBBBBB#CCCCCCCCCCCC#.........",  // 26
        ".....#BBBBBBBBB##BBBBBBBBB#CCCCCCCCCCC#.........",  // 27
        ".....#BB#BBBBBBB#BBBBBBBBB#CCCSCCCCCCCC#........",  // 28
        "....#SB#B#BBBBBB##BBBBBBB#CCCCSSCCCCCCB#........",  // 29
        "....##L#B##BBBBB#DBBBBBB#CCCCCCCCCCCCB#.........",  // 30
        "....#D#SL#B#BBBB#DDBBBB#CCCCCCCCCCCCB#..........",  // 31
        "....#DSS#BL#BBB#DDSBBBB#CCSCCCCCSCCCB#..........",  // 32
        "....#DBSBB##BB#DDSSBBBB#CCSCCCCCSCCC#...........",  // 33
        "....##BSBL#BB#DDSSBBBBB#CCCCCCCCCCCC#...........",  // 34
        ".....#B#L#BBB#DSSBBBBBBBCCCCSCCCCCCC#...........",  // 35
        ".....#L##SLBB#SBBBBBBBBBCCCCCSCCCCC#............",  // 36
        ".....##D##SLB#BBBBBBBBBBBCCCCCCCCC#.............",  // 37
        ".......#D##SLLBBBCCCCCCCCCCCCCCCC#..............",  // 38
        "........#D##SSLBBBBBBBBBBBBBC###................",  // 39
        ".........##D###SSLLBBBBBBLLSS#..................",  // 40
        "...........####D###SSSSSSSS##...................",  // 41
        "......................#MM#......##..............",  // 42: Far foot LIFTED (Y=42), Near leg planted
        ".....................#DD##DD#..#OO#.............",  // 43: Far claws LIFTED (Y=43)
        "..............................#MM##MM#..........",  // 44: Near leg ON FLOOR Y=44, Far leg IN AIR!
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    // Frame 2: Contact B (Far leg forward, Near leg back, both on floor Y=44)
    static readonly string[] Frame2 = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "...............##...............................",  // 07
        "..............#L##..............................",  // 08
        "..............#SLL#.............................",  // 09
        "...............#SLL##...########................",  // 10
        ".............###SLLL##SBBBBBBBBS#...............",  // 11
        "............#LBSSLLL#BBBBBBBBBBBB#..............",  // 12
        "...........#SLLSSSL#BBBBBBBBBBBBBB#.............",  // 13
        "..........#SLLSSS#BBBBBBSSBBBBBBBBB#............",  // 14
        ".........#SLLFF#BBBBBBBBB##BB###BBBB#...........",  // 15
        "........#SLLF#BBBBBBBBBB#FFFF###FFFF#...........",  // 16
        ".......#SLLF#BBBBBBBBB#FFF#RR##FFFFFF#..........",  // 17
        "......#SLLF#BBBBBBBBB#FFF#RWKK#FFFFFF#..........",  // 18
        "......#LLB#BBBBBBBBB#FFF#YKKK#FFFFFF##..........",  // 19
        "......#LBB#BBBBBBBBB#FFF#YKKW#FFFFFF#OO#........",  // 20
        "......#LBB#BBBBBBBBB#FFF#ROOO#FFFFF#MOOO#.......",  // 21
        "......#SLB#BBBBBBBBBB#FFF####FFFFFF#MMO#........",  // 22
        "......#SLB##BBBBBBBBBB#FFFFFFFFFFFF##M#.........",  // 23
        ".......#BBB##BBBBBBBBBB#FFFFFFFFFFF##...........",  // 24
        ".......#BBBBB##BBBBBBBBB#CCCCCCCCCCCC#..........",  // 25
        "......#BBBBBBB##BBBBBBBBB#CCCCCCCCCCCC#.........",  // 26
        ".....#BBBBBBBBB##BBBBBBBBB#CCCCCCCCCCC#.........",  // 27
        ".....#BB#BBBBBBB#BBBBBBBBB#CCCSCCCCCCCC#........",  // 28
        "....#SB#B#BBBBBB##BBBBBBB#CCCCSSCCCCCCB#........",  // 29
        "....##L#B##BBBBB#DBBBBBB#CCCCCCCCCCCCB#.........",  // 30
        "....#D#SL#B#BBBB#DDBBBB#CCCCCCCCCCCCB#..........",  // 31
        "....#DSS#BL#BBB#DDSBBBB#CCSCCCCCSCCCB#..........",  // 32
        "....#DBSBB##BB#DDSSBBBB#CCSCCCCCSCCC#...........",  // 33
        "....##BSBL#BB#DDSSBBBBB#CCCCCCCCCCCC#...........",  // 34
        ".....#B#L#BBB#DSSBBBBBBBCCCCSCCCCCCC#...........",  // 35
        ".....#L##SLBB#SBBBBBBBBBCCCCCSCCCCC#............",  // 36
        ".....##D##SLB#BBBBBBBBBBBCCCCCCCCC#.............",  // 37
        ".......#D##SLLBBBCCCCCCCCCCCCCCCC#..............",  // 38
        "........#D##SSLBBBBBBBBBBBBBC###................",  // 39
        ".........##D###SSLLBBBBBBLLSS#..................",  // 40
        "...........####D###SSSSSSSS##...................",  // 41
        ".................##.........##..................",  // 42: Near leg back (X=17-18), Far leg forward (X=28-29)
        "................#OO#.......#MM#.................",  // 43: Near foot back, Far foot forward
        "...............#MM##MM#...#DD##DD#..............",  // 44: Both on floor Y=44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    // Frame 3: Passing B (Far leg supporting at Y=44, Near leg LIFTED at Y=41..43)
    static readonly string[] Frame3 = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "...............##...............................",  // 07
        "..............#L##..............................",  // 08
        "..............#SLL#.............................",  // 09
        "...............#SLL##...########................",  // 10
        ".............###SLLL##SBBBBBBBBS#...............",  // 11
        "............#LBSSLLL#BBBBBBBBBBBB#..............",  // 12
        "...........#SLLSSSL#BBBBBBBBBBBBBB#.............",  // 13
        "..........#SLLSSS#BBBBBBSSBBBBBBBBB#............",  // 14
        ".........#SLLFF#BBBBBBBBB##BB###BBBB#...........",  // 15
        "........#SLLF#BBBBBBBBBB#FFFF###FFFF#...........",  // 16
        ".......#SLLF#BBBBBBBBB#FFF#RR##FFFFFF#..........",  // 17
        "......#SLLF#BBBBBBBBB#FFF#RWKK#FFFFFF#..........",  // 18
        "......#LLB#BBBBBBBBB#FFF#YKKK#FFFFFF##..........",  // 19
        "......#LBB#BBBBBBBBB#FFF#YKKW#FFFFFF#OO#........",  // 20
        "......#LBB#BBBBBBBBB#FFF#ROOO#FFFFF#MOOO#.......",  // 21
        "......#SLB#BBBBBBBBBB#FFF####FFFFFF#MMO#........",  // 22
        "......#SLB##BBBBBBBBBB#FFFFFFFFFFFF##M#.........",  // 23
        ".......#BBB##BBBBBBBBBB#FFFFFFFFFFF##...........",  // 24
        ".......#BBBBB##BBBBBBBBB#CCCCCCCCCCCC#..........",  // 25
        "......#BBBBBBB##BBBBBBBBB#CCCCCCCCCCCC#.........",  // 26
        ".....#BBBBBBBBB##BBBBBBBBB#CCCCCCCCCCC#.........",  // 27
        ".....#BB#BBBBBBB#BBBBBBBBB#CCCSCCCCCCCC#........",  // 28
        "....#SB#B#BBBBBB##BBBBBBB#CCCCSSCCCCCCB#........",  // 29
        "....##L#B##BBBBB#DBBBBBB#CCCCCCCCCCCCB#.........",  // 30
        "....#D#SL#B#BBBB#DDBBBB#CCCCCCCCCCCCB#..........",  // 31
        "....#DSS#BL#BBB#DDSBBBB#CCSCCCCCSCCCB#..........",  // 32
        "....#DBSBB##BB#DDSSBBBB#CCSCCCCCSCCC#...........",  // 33
        "....##BSBL#BB#DDSSBBBBB#CCCCCCCCCCCC#...........",  // 34
        ".....#B#L#BBB#DSSBBBBBBBCCCCSCCCCCCC#...........",  // 35
        ".....#L##SLBB#SBBBBBBBBBCCCCCSCCCCC#............",  // 36
        ".....##D##SLB#BBBBBBBBBBBCCCCCCCCC#.............",  // 37
        ".......#D##SLLBBBCCCCCCCCCCCCCCCC#..............",  // 38
        "........#D##SSLBBBBBBBBBBBBBC###................",  // 39
        ".........##D###SSLLBBBBBBLLSS#..................",  // 40
        "...........####D###SSSSSSSS##...................",  // 41
        "......................#OO#......##..............",  // 42: Near foot LIFTED (Y=42), Far leg planted
        ".....................#MM##MM#..#MM#.............",  // 43: Near claws LIFTED (Y=43)
        "..............................#DD##DD#..........",  // 44: Far leg ON FLOOR Y=44, Near leg IN AIR!
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
            if (row.Length != 48) throw new Exception(string.Format("Row {0} length is {1}", y, row.Length));
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
        Console.WriteLine("Generating Final Profile WALK Animation (4 frames)...");

        string baseDir = Directory.Exists("assets") ? Directory.GetCurrentDirectory() : AppDomain.CurrentDomain.BaseDirectory;
        string outDir = Path.Combine(baseDir, @"assets\sprites\walk");
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
            Bitmap bmp = Render(kvp.Value);
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

            string stripPath = Path.Combine(baseDir, @"assets\preview_profile_walk_final_8x.png");
            strip.Save(stripPath, ImageFormat.Png);
            Console.WriteLine("Saved 8x Profile WALK strip preview to: {0}", stripPath);
        }

        // Clean up
        foreach (var bmp in bitmaps)
        {
            bmp.Dispose();
        }

        Console.WriteLine("[OK] Final Profile WALK Animation Set generated successfully!");
    }
}
