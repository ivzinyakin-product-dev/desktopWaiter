using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class WalkSpriteGenerator
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

    // Frame 0: Contact A (Left step forward 1px, Right step back, both feet touching Y=44)
    static readonly string[] Frame0 = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "..........##........................##..........",  // 07
        "..........#L##....................##L#..........",  // 08
        "..........#SLL#..................#LLS#..........",  // 09
        "...........#SLL##..##########..##LLS#...........",  // 10
        ".........####SLLL##SBBBBBBBBS##LLLS####.........",  // 11
        ".........#LBSSSLLLL##BBBBBB##LLLLSSSBL#.........",  // 12
        "..........#SLLSSSSSLB#BBBB#BLSSSSSLLS#..........",  // 13
        "...........########SBBSBBSBBS########...........",  // 14
        "...........#SLLFFFF##BBSSBB##FFFFLLS#...........",  // 15
        "..........#SLLFF###FF#BBBB#FF###FFLLS#..........",  // 16
        ".........#SLLFF##RR#FFLBBLFF#RR##FFLLS#.........",  // 17
        "........#SLLFFF#RWKK#FFLLFF#WKKR#FFFLLS#........",  // 18
        "........#LLBFFF#YKKK#FFFFFF#KKKY#FFFBLL#........",  // 19
        "........#LBBFFF#YKKW#FFMMFF#KKWY#FFFBBL#........",  // 20
        "........#LBBFFF#ROOO#FMOOMF#OOOR#FFFBBL#........",  // 21
        "........#SLBBFFF####FFMOOMFF####FFFBBLS#........",  // 22
        "........#SLBBBFFFFFFFB#MM#BFFFFFFFBBBLS#........",  // 23
        "........###LBBBFFFFBBCC##CCBBFFFFBBBL###........",  // 24
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
        ".................##.........##..................",  // 41: Left ankle forward 1px (X=17-18)
        "................#OO#.......#OO#.................",  // 42: Left foot forward 1px
        "...............#OOOO#.....#OOOO#................",  // 43: Left foot forward 1px
        "..............#MM##MM#...#MM##MM#...............",  // 44: Both on floor Y=44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    // Frame 1: Passing A (Left foot firmly planted at Y=44, Right foot LIFTED at Y=41..43, air under right foot on Y=44)
    // Body has a subtle 1px waddle lean to the left, right wing flutters up 1px
    static readonly string[] Frame1 = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        ".........##.........................##..........",  // 07: Ear tufts tilt 1px left
        ".........#L##.....................##L#..........",  // 08
        ".........#SLL#...................#LLS#..........",  // 09
        "..........#SLL##..##########...##LLS#...........",  // 10
        "........####SLLL##SBBBBBBBBS##LLLS####..........",  // 11
        "........#LBSSSLLLL##BBBBBB##LLLLSSSBL#..........",  // 12
        ".........#SLLSSSSSLB#BBBB#BLSSSSSLLS#...........",  // 13
        "..........########SBBSBBSBBS########............",  // 14
        "..........#SLLFFFF##BBSSBB##FFFFLLS#............",  // 15
        ".........#SLLFF###FF#BBBB#FF###FFLLS#...........",  // 16
        "........#SLLFF##RR#FFLBBLFF#RR##FFLLS#..........",  // 17
        ".......#SLLFFF#RWKK#FFLLFF#WKKR#FFFLLS#.........",  // 18
        ".......#LLBFFF#YKKK#FFFFFF#KKKY#FFFBLL#.........",  // 19
        ".......#LBBFFF#YKKW#FFMMFF#KKWY#FFFBBL#.........",  // 20
        ".......#LBBFFF#ROOO#FMOOMF#OOOR#FFFBBL#.........",  // 21
        ".......#SLBBFFF####FFMOOMFF####FFFBBLS#.........",  // 22
        ".......#SLBBBFFFFFFFB#MM#BFFFFFFFBBBLS#.........",  // 23
        ".......###LBBBFFFFBBCC##CCBBFFFFBBBL###.........",  // 24
        "......#BBB#LBBBBBBCCCCCCCCCCBBBBBBL#BBB#........",  // 25
        ".....#BBBBB#BBBCCCCCCCCCCCCCCCCBBB#BBBBB#.......",  // 26
        ".....#BBBBBB#BBCSCCLCCCCCCLCCSCBB#BBBBBB#.......",  // 27
        "....#SB#BBBB#BCCCSSCCBCCBCCSSCCCB#BBBB#BS#......",  // 28
        "....##L#B#BB#BCCCCCCCCBBCCCCCCCCB#BB#B#L##......",  // 29
        "....#D#SL#BL#BCCCCCCCCCCCCCCCCCCB#LB#LS#D#......",  // 30
        "....#DSS#BL#BCCSCCCSCCCCCCSCCCSCCB#LB#SSD#......",  // 31
        "....#DBSBB##BCCCSCCCSCCCCSCCCSCCCB##BBSBD#......",  // 32
        "....##BSBL#BBBCCCCCCCCCCCCCCCCCCBBB#LBSB##......",  // 33
        ".....#B#L#BBBBCCCCSCCCCCCCCSCCCCBBBB#L#B#.......",  // 34
        ".....#L##SLBBBCCCCCSCCCCCCSCCCCCBBBLS##L#.......",  // 35
        ".....##D##SLBBBCCCCCCCSSCCCCCCCBBBLS##D##.......",  // 36
        ".......#D##SLLBBBCCCCCCCCCCCCBBBLLS##D#.........",  // 37
        "........#D##SSLBBBBBBBBBBBBBBBBLSS##D#..........",  // 38
        ".........##D###SSLLBBBBBBBBLLSS###D##...........",  // 39
        "...........####D###SSSSSSSS###D####.............",  // 40
        ".................##.........#OO#................",  // 41: Left ankle + Right foot lifted (Y=41)
        "................#OO#.......#OOOO#...............",  // 42: Left foot + Right foot lifted (Y=42)
        "...............#OOOO#.....#MM##MM#..............",  // 43: Left foot + Right claws lifted (Y=43)
        "..............#MM##MM#..........................",  // 44: Left foot ON FLOOR Y=44, Right foot IN AIR!
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    // Frame 2: Contact B (Right step forward 1px, Left step back, both feet touching Y=44)
    static readonly string[] Frame2 = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "..........##........................##..........",  // 07
        "..........#L##....................##L#..........",  // 08
        "..........#SLL#..................#LLS#..........",  // 09
        "...........#SLL##..##########..##LLS#...........",  // 10
        ".........####SLLL##SBBBBBBBBS##LLLS####.........",  // 11
        ".........#LBSSSLLLL##BBBBBB##LLLLSSSBL#.........",  // 12
        "..........#SLLSSSSSLB#BBBB#BLSSSSSLLS#..........",  // 13
        "...........########SBBSBBSBBS########...........",  // 14
        "...........#SLLFFFF##BBSSBB##FFFFLLS#...........",  // 15
        "..........#SLLFF###FF#BBBB#FF###FFLLS#..........",  // 16
        ".........#SLLFF##RR#FFLBBLFF#RR##FFLLS#.........",  // 17
        "........#SLLFFF#RWKK#FFLLFF#WKKR#FFFLLS#........",  // 18
        "........#LLBFFF#YKKK#FFFFFF#KKKY#FFFBLL#........",  // 19
        "........#LBBFFF#YKKW#FFMMFF#KKWY#FFFBBL#........",  // 20
        "........#LBBFFF#ROOO#FMOOMF#OOOR#FFFBBL#........",  // 21
        "........#SLBBFFF####FFMOOMFF####FFFBBLS#........",  // 22
        "........#SLBBBFFFFFFFB#MM#BFFFFFFFBBBLS#........",  // 23
        "........###LBBBFFFFBBCC##CCBBFFFFBBBL###........",  // 24
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
        "..................##.........##.................",  // 41: Right ankle forward 1px (X=29-30)
        ".................#OO#.......#OO#................",  // 42: Right foot forward 1px
        "................#OOOO#.....#OOOO#...............",  // 43: Right foot forward 1px
        "...............#MM##MM#...#MM##MM#..............",  // 44: Both on floor Y=44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    // Frame 3: Passing B (Right foot firmly planted at Y=44, Left foot LIFTED at Y=41..43, air under left foot on Y=44)
    // Body has a subtle 1px waddle lean to the right, left wing flutters up 1px
    static readonly string[] Frame3 = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "..........##.........................##.........",  // 07: Ear tufts tilt 1px right
        "..........#L##.....................##L#.........",  // 08
        "..........#SLL#...................#LLS#.........",  // 09
        "...........#SLL##...##########..##LLS#..........",  // 10
        "..........####SLLL##SBBBBBBBBS##LLLS####........",  // 11
        "..........#LBSSSLLLL##BBBBBB##LLLLSSSBL#........",  // 12
        "...........#SLLSSSSSLB#BBBB#BLSSSSSLLS#.........",  // 13
        "............########SBBSBBSBBS########..........",  // 14
        "............#SLLFFFF##BBSSBB##FFFFLLS#..........",  // 15
        "...........#SLLFF###FF#BBBB#FF###FFLLS#.........",  // 16
        "..........#SLLFF##RR#FFLBBLFF#RR##FFLLS#........",  // 17
        ".........#SLLFFF#RWKK#FFLLFF#WKKR#FFFLLS#.......",  // 18
        ".........#LLBFFF#YKKK#FFFFFF#KKKY#FFFBLL#.......",  // 19
        ".........#LBBFFF#YKKW#FFMMFF#KKWY#FFFBBL#.......",  // 20
        ".........#LBBFFF#ROOO#FMOOMF#OOOR#FFFBBL#.......",  // 21
        ".........#SLBBFFF####FFMOOMFF####FFFBBLS#.......",  // 22
        ".........#SLBBBFFFFFFFB#MM#BFFFFFFFBBBLS#.......",  // 23
        ".........###LBBBFFFFBBCC##CCBBFFFFBBBL###.......",  // 24
        "........#BBB#LBBBBBBCCCCCCCCCCBBBBBBL#BBB#......",  // 25
        ".......#BBBBB#BBBCCCCCCCCCCCCCCCCBBB#BBBBB#.....",  // 26
        ".......#BBBBBB#BBCSCCLCCCCCCLCCSCBB#BBBBBB#.....",  // 27
        "......#SB#BBBB#BCCCSSCCBCCBCCSSCCCB#BBBB#BS#....",  // 28
        "......##L#B#BB#BCCCCCCCCBBCCCCCCCCB#BB#B#L##....",  // 29
        "......#D#SL#BL#BCCCCCCCCCCCCCCCCCCB#LB#LS#D#....",  // 30
        "......#DSS#BL#BCCSCCCSCCCCCCSCCCSCCB#LB#SSD#....",  // 31
        "......#DBSBB##BCCCSCCCSCCCCSCCCSCCCB##BBSBD#....",  // 32
        "......##BSBL#BBBCCCCCCCCCCCCCCCCCCBBB#LBSB##....",  // 33
        ".......#B#L#BBBBCCCCSCCCCCCCCSCCCCBBBB#L#B#.....",  // 34
        ".......#L##SLBBBCCCCCSCCCCCCSCCCCCBBBLS##L#.....",  // 35
        ".......##D##SLBBBCCCCCCCSSCCCCCCCBBBLS##D##.....",  // 36
        ".........#D##SLLBBBCCCCCCCCCCCCBBBLLS##D#.......",  // 37
        "..........#D##SSLBBBBBBBBBBBBBBBBLSS##D#........",  // 38
        "...........##D###SSLLBBBBBBBBLLSS###D##.........",  // 39
        ".............####D###SSSSSSSS###D####...........",  // 40
        "................#OO#.........##.................",  // 41: Left foot lifted (Y=41) + Right ankle
        "...............#OOOO#.......#OO#................",  // 42: Left foot lifted (Y=42) + Right foot
        "..............#MM##MM#.....#OOOO#...............",  // 43: Left claws lifted (Y=43) + Right foot
        "..........................#MM##MM#..............",  // 44: Left foot IN AIR!, Right foot ON FLOOR Y=44
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
        Console.WriteLine("Generating complete WALK Animation Set (4 frames)...");

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

            string stripPath = Path.Combine(baseDir, @"assets\preview_walk_strip_8x.png");
            strip.Save(stripPath, ImageFormat.Png);
            Console.WriteLine("Saved 8x WALK strip preview to: {0}", stripPath);
        }

        // Clean up
        foreach (var bmp in bitmaps)
        {
            bmp.Dispose();
        }

        Console.WriteLine("[OK] WALK Animation Set generated successfully!");
    }
}
