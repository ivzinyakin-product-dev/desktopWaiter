using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class NarrowProfileWalkGenerator
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
        "..................##............................",  // 07
        ".................#L##...........................",  // 08
        "................#SLL#...........................",  // 09
        "...............#SLL##...#######.................",  // 10
        "..............##SLLL##SBBBBBBBBS#...............",  // 11
        ".............#LBSSLLL#BBBBBBBBBB#...............",  // 12
        "............#SLLSSSL#BBBBBBBBBBBB#..............",  // 13
        "...........#SLLSSS#BBBBBBSSBBBBBBB#.............",  // 14
        "...........#SLLFF#BBBBBBB##BB###BBB#............",  // 15
        "...........#SLLF#BBBBBBB#FFFF###FFFF#...........",  // 16
        "...........#SLL#BBBBBBB#FFF#RR##FFFFF#..........",  // 17
        "...........#LLB#BBBBBBB#FFF#RWKK#FFFFF#.........",  // 18
        "...........#LBB#BBBBBBB#FFF#YKKK#FFFFFF#........",  // 19
        "...........#LBB#BBBBBBB#FFF#YKKW#FFFFFF#OO#.....",  // 20
        "...........#SLB#BBBBBBB#FFF#ROOO#FFFFF#OOOO#....",  // 21
        "...........#SLB##BBBBBBB#FFF####FFFFFF#MMO#.....",  // 22
        "............#BBB##BBBBBBB#FFFFFFFFFFF#MMM#......",  // 23
        "............#BBBBB##BBBBBB#CCCCCCCCCCC#.........",  // 24
        ".............#BBBBB##BBBBBB#CCCCCCCCCCC#........",  // 25
        ".............#LBBBBB##BBBBBB#CCCCCCCCCCC#.......",  // 26
        ".............#LLBBBBB#BBBBBB#CCCSCCCCCCC#.......",  // 27
        ".............#SLLBBBB##BBBBB#CCCCSSCCCCC#.......",  // 28
        ".............#SSLLBBB#DBBBBB#CCCCCCCCCCC#.......",  // 29
        ".............#DSSLLBB#DDBBBB#CCCCCCCCCCC#.......",  // 30
        "..............#DDSSLB#DDSBBB#CCSCCCCCCC#........",  // 31
        "..............#DDDSSL#DDSSBB#CCSCCCCCCC#........",  // 32
        "...............#DDDSSL#DDSSB#CCCCCCCCCC#........",  // 33
        "................#DDDSLL#DSSB#CCCCSCCCC#.........",  // 34
        ".................#DDDSLL#SBB#CCCCSCCCC#.........",  // 35
        "..................#DDDSSL#BB#CCCCCCCC#..........",  // 36
        "...................#DDDSSL#B#CCCCCCC#...........",  // 37
        "....................##DDDSSL#CCCCCC#............",  // 38
        "......................##DDSS#LLSS##.............",  // 39
        "........................####SSSS##..............",  // 40
        ".........................##.....##..............",  // 41: Far leg back (X=25-26), Near leg forward (X=32-33)
        "........................#MM#...#OO#.............",  // 42: Far shaded, Near bright
        ".......................#MMMM#.#OOOO#............",  // 43
        "......................#DD##DD#MM##MM#...........",  // 44: Floor contact on Y=44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    // Frame 1: Passing A (Near leg supporting at Y=44, Far leg LIFTED at Y=41..43, wing tip lift)
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
        ".................#L##...........................",  // 08
        "................#SLL#...........................",  // 09
        "...............#SLL##...#######.................",  // 10
        "..............##SLLL##SBBBBBBBBS#...............",  // 11
        ".............#LBSSLLL#BBBBBBBBBB#...............",  // 12
        "............#SLLSSSL#BBBBBBBBBBBB#..............",  // 13
        "...........#SLLSSS#BBBBBBSSBBBBBBB#.............",  // 14
        "...........#SLLFF#BBBBBBB##BB###BBB#............",  // 15
        "...........#SLLF#BBBBBBB#FFFF###FFFF#...........",  // 16
        "...........#SLL#BBBBBBB#FFF#RR##FFFFF#..........",  // 17
        "...........#LLB#BBBBBBB#FFF#RWKK#FFFFF#.........",  // 18
        "...........#LBB#BBBBBBB#FFF#YKKK#FFFFFF#........",  // 19
        "...........#LBB#BBBBBBB#FFF#YKKW#FFFFFF#OO#.....",  // 20
        "...........#SLB#BBBBBBB#FFF#ROOO#FFFFF#OOOO#....",  // 21
        "...........#SLB##BBBBBBB#FFF####FFFFFF#MMO#.....",  // 22
        "............#BBB##BBBBBBB#FFFFFFFFFFF#MMM#......",  // 23
        "............#BBBBB##BBBBBB#CCCCCCCCCCC#.........",  // 24
        ".............#BBBBB##BBBBBB#CCCCCCCCCCC#........",  // 25
        ".............#LBBBBB##BBBBBB#CCCCCCCCCCC#.......",  // 26
        ".............#LLBBBBB#BBBBBB#CCCSCCCCCCC#.......",  // 27
        ".............#SLLBBBB##BBBBB#CCCCSSCCCCC#.......",  // 28
        ".............#SSLLBBB#DBBBBB#CCCCCCCCCCC#.......",  // 29
        ".............#DSSLLBB#DDBBBB#CCCCCCCCCCC#.......",  // 30
        "..............#DDSSLB#DDSBBB#CCSCCCCCCC#........",  // 31
        "..............#DDDSSL#DDSSBB#CCSCCCCCCC#........",  // 32
        "...............#DDDSSL#DDSSB#CCCCCCCCCC#........",  // 33
        "................#DDDSLL#DSSB#CCCCSCCCC#.........",  // 34
        ".................#DDDSLL#SBB#CCCCSCCCC#.........",  // 35
        "..................#DDDSSL#BB#CCCCCCCC#..........",  // 36
        "...................#DDDSSL#B#CCCCCCC#...........",  // 37
        "....................##DDDSSL#CCCCCC#............",  // 38
        "......................##DDSS#LLSS##.............",  // 39
        "........................####SSSS##..............",  // 40
        "............................#MM#..##............",  // 41: Far foot LIFTED (Y=41), Near leg supporting
        "...........................#MMMM#.#OO#..........",  // 42: Far foot lifted (Y=42)
        "..........................#DD##DD#OOOO#.........",  // 43: Far claws lifted (Y=43)
        ".................................#MM##MM#.......",  // 44: Near leg ON FLOOR Y=44, Far leg IN AIR!
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
        "..................##............................",  // 07
        ".................#L##...........................",  // 08
        "................#SLL#...........................",  // 09
        "...............#SLL##...#######.................",  // 10
        "..............##SLLL##SBBBBBBBBS#...............",  // 11
        ".............#LBSSLLL#BBBBBBBBBB#...............",  // 12
        "............#SLLSSSL#BBBBBBBBBBBB#..............",  // 13
        "...........#SLLSSS#BBBBBBSSBBBBBBB#.............",  // 14
        "...........#SLLFF#BBBBBBB##BB###BBB#............",  // 15
        "...........#SLLF#BBBBBBB#FFFF###FFFF#...........",  // 16
        "...........#SLL#BBBBBBB#FFF#RR##FFFFF#..........",  // 17
        "...........#LLB#BBBBBBB#FFF#RWKK#FFFFF#.........",  // 18
        "...........#LBB#BBBBBBB#FFF#YKKK#FFFFFF#........",  // 19
        "...........#LBB#BBBBBBB#FFF#YKKW#FFFFFF#OO#.....",  // 20
        "...........#SLB#BBBBBBB#FFF#ROOO#FFFFF#OOOO#....",  // 21
        "...........#SLB##BBBBBBB#FFF####FFFFFF#MMO#.....",  // 22
        "............#BBB##BBBBBBB#FFFFFFFFFFF#MMM#......",  // 23
        "............#BBBBB##BBBBBB#CCCCCCCCCCC#.........",  // 24
        ".............#BBBBB##BBBBBB#CCCCCCCCCCC#........",  // 25
        ".............#LBBBBB##BBBBBB#CCCCCCCCCCC#.......",  // 26
        ".............#LLBBBBB#BBBBBB#CCCSCCCCCCC#.......",  // 27
        ".............#SLLBBBB##BBBBB#CCCCSSCCCCC#.......",  // 28
        ".............#SSLLBBB#DBBBBB#CCCCCCCCCCC#.......",  // 29
        ".............#DSSLLBB#DDBBBB#CCCCCCCCCCC#.......",  // 30
        "..............#DDSSLB#DDSBBB#CCSCCCCCCC#........",  // 31
        "..............#DDDSSL#DDSSBB#CCSCCCCCCC#........",  // 32
        "...............#DDDSSL#DDSSB#CCCCCCCCCC#........",  // 33
        "................#DDDSLL#DSSB#CCCCSCCCC#.........",  // 34
        ".................#DDDSLL#SBB#CCCCSCCCC#.........",  // 35
        "..................#DDDSSL#BB#CCCCCCCC#..........",  // 36
        "...................#DDDSSL#B#CCCCCCC#...........",  // 37
        "....................##DDDSSL#CCCCCC#............",  // 38
        "......................##DDSS#LLSS##.............",  // 39
        "........................####SSSS##..............",  // 40
        ".........................##.....##..............",  // 41: Near leg back (X=25-26), Far leg forward (X=32-33)
        "........................#OO#...#MM#.............",  // 42: Near foot back, Far foot forward
        ".......................#OOOO#.#MMMM#............",  // 43
        "......................#MM##MM#DD##DD#...........",  // 44: Both on floor Y=44
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
        "..................##............................",  // 07
        ".................#L##...........................",  // 08
        "................#SLL#...........................",  // 09
        "...............#SLL##...#######.................",  // 10
        "..............##SLLL##SBBBBBBBBS#...............",  // 11
        ".............#LBSSLLL#BBBBBBBBBB#...............",  // 12
        "............#SLLSSSL#BBBBBBBBBBBB#..............",  // 13
        "...........#SLLSSS#BBBBBBSSBBBBBBB#.............",  // 14
        "...........#SLLFF#BBBBBBB##BB###BBB#............",  // 15
        "...........#SLLF#BBBBBBB#FFFF###FFFF#...........",  // 16
        "...........#SLL#BBBBBBB#FFF#RR##FFFFF#..........",  // 17
        "...........#LLB#BBBBBBB#FFF#RWKK#FFFFF#.........",  // 18
        "...........#LBB#BBBBBBB#FFF#YKKK#FFFFFF#........",  // 19
        "...........#LBB#BBBBBBB#FFF#YKKW#FFFFFF#OO#.....",  // 20
        "...........#SLB#BBBBBBB#FFF#ROOO#FFFFF#OOOO#....",  // 21
        "...........#SLB##BBBBBBB#FFF####FFFFFF#MMO#.....",  // 22
        "............#BBB##BBBBBBB#FFFFFFFFFFF#MMM#......",  // 23
        "............#BBBBB##BBBBBB#CCCCCCCCCCC#.........",  // 24
        ".............#BBBBB##BBBBBB#CCCCCCCCCCC#........",  // 25
        ".............#LBBBBB##BBBBBB#CCCCCCCCCCC#.......",  // 26
        ".............#LLBBBBB#BBBBBB#CCCSCCCCCCC#.......",  // 27
        ".............#SLLBBBB##BBBBB#CCCCSSCCCCC#.......",  // 28
        ".............#SSLLBBB#DBBBBB#CCCCCCCCCCC#.......",  // 29
        ".............#DSSLLBB#DDBBBB#CCCCCCCCCCC#.......",  // 30
        "..............#DDSSLB#DDSBBB#CCSCCCCCCC#........",  // 31
        "..............#DDDSSL#DDSSBB#CCSCCCCCCC#........",  // 32
        "...............#DDDSSL#DDSSB#CCCCCCCCCC#........",  // 33
        "................#DDDSLL#DSSB#CCCCSCCCC#.........",  // 34
        ".................#DDDSLL#SBB#CCCCSCCCC#.........",  // 35
        "..................#DDDSSL#BB#CCCCCCCC#..........",  // 36
        "...................#DDDSSL#B#CCCCCCC#...........",  // 37
        "....................##DDDSSL#CCCCCC#............",  // 38
        "......................##DDSS#LLSS##.............",  // 39
        "........................####SSSS##..............",  // 40
        "............................#OO#..##............",  // 41: Near foot LIFTED (Y=41), Far leg supporting
        "...........................#OOOO#.#MM#..........",  // 42: Near foot lifted (Y=42)
        "..........................#MM##MM#MMMM#.........",  // 43: Near claws lifted (Y=43)
        ".................................#DD##DD#.......",  // 44: Far leg ON FLOOR Y=44, Near leg IN AIR!
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
        Console.WriteLine("Generating Slender Clean Profile WALK Animation (4 frames)...");

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

            string stripPath = Path.Combine(baseDir, @"assets\preview_narrow_profile_walk_8x.png");
            strip.Save(stripPath, ImageFormat.Png);
            Console.WriteLine("Saved 8x Slender Profile WALK strip preview to: {0}", stripPath);
        }

        // Clean up
        foreach (var bmp in bitmaps)
        {
            bmp.Dispose();
        }

        Console.WriteLine("[OK] Slender Clean Profile WALK Animation Set generated successfully!");
    }
}
