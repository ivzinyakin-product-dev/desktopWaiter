using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class IdleSpriteGenerator
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

    // Frame 0: Approved Master Frame (Neutral)
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
        "..................##........##..................",  // 41
        ".................#OO#......#OO#.................",  // 42
        "................#OOOO#....#OOOO#................",  // 43
        "...............#MM##MM#..#MM##MM#...............",  // 44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    // Frame 1: Inhale Start (Fluff chest expands 1px wider on rows 27-33)
    static readonly string[] Frame1 = new string[]
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
        "......#BBBBB#BBCCSCCLCCCCCCLCCSCCBB#BBBBB#......",  // 27: Chest fluff +1px wider
        ".....#SB#BBB#BBCCCSSCCBCCBCCSSCCCBB#BBB#BS#.....",  // 28: Chest fluff +1px wider
        ".....##L#B#B#BBCCCCCCCCBBCCCCCCCCBB#B#B#L##.....",  // 29: Chest fluff +1px wider
        ".....#D#SL#B#BBCCCCCCCCCCCCCCCCCCBB#B#LS#D#.....",  // 30: Chest fluff +1px wider
        ".....#DSS#B#BBCCSCCCSCCCCCCSCCCSCCBB#B#SSD#.....",  // 31: Chest fluff +1px wider
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

    // Frame 2: Inhale Peak (Torso & Head rise +1px, ankle stretches 1px, feet anchored at Y=44)
    static readonly string[] Frame2 = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "..........##........................##..........",  // 06: Ear tufts peak +1px
        "..........#L##....................##L#..........",  // 07
        "..........#SLL#..................#LLS#..........",  // 08
        "...........#SLL##..##########..##LLS#...........",  // 09
        ".........####SLLL##SBBBBBBBBS##LLLS####.........",  // 10
        ".........#LBSSSLLLL##BBBBBB##LLLLSSSBL#.........",  // 11
        "..........#SLLSSSSSLB#BBBB#BLSSSSSLLS#..........",  // 12
        "...........########SBBSBBSBBS########...........",  // 13
        "...........#SLLFFFF##BBSSBB##FFFFLLS#...........",  // 14
        "..........#SLLFF###FF#BBBB#FF###FFLLS#..........",  // 15
        ".........#SLLFF##RR#FFLBBLFF#RR##FFLLS#.........",  // 16
        "........#SLLFFF#RWKK#FFLLFF#WKKR#FFFLLS#........",  // 17
        "........#LLBFFF#YKKK#FFFFFF#KKKY#FFFBLL#........",  // 18
        "........#LBBFFF#YKKW#FFMMFF#KKWY#FFFBBL#........",  // 19
        "........#LBBFFF#ROOO#FMOOMF#OOOR#FFFBBL#........",  // 20
        "........#SLBBFFF####FFMOOMFF####FFFBBLS#........",  // 21
        "........#SLBBBFFFFFFFB#MM#BFFFFFFFBBBLS#........",  // 22
        "........###LBBBFFFFBBCC##CCBBFFFFBBBL###........",  // 23
        ".......#BBB#LBBBBBBCCCCCCCCCCBBBBBBL#BBB#.......",  // 24
        "......#BBBBB#BBBCCCCCCCCCCCCCCCCBBB#BBBBB#......",  // 25
        "......#BBBBB#BBCCSCCLCCCCCCLCCSCCBB#BBBBB#......",  // 26
        ".....#SB#BBB#BBCCCSSCCBCCBCCSSCCCBB#BBB#BS#.....",  // 27
        ".....##L#B#B#BBCCCCCCCCBBCCCCCCCCBB#B#B#L##.....",  // 28
        ".....#D#SL#B#BBCCCCCCCCCCCCCCCCCCBB#B#LS#D#.....",  // 29
        ".....#DSS#B#BBCCSCCCSCCCCCCSCCCSCCBB#B#SSD#.....",  // 30
        ".....#DBSBB##BCCCSCCCSCCCCSCCCSCCCB##BBSBD#.....",  // 31
        ".....##BSBL#BBBCCCCCCCCCCCCCCCCCCBBB#LBSB##.....",  // 32
        "......#B#L#BBBBCCCCSCCCCCCCCSCCCCBBBB#L#B#......",  // 33
        "......#L##SLBBBCCCCCSCCCCCCSCCCCCBBBLS##L#......",  // 34
        "......##D##SLBBBCCCCCCCSSCCCCCCCBBBLS##D##......",  // 35
        "........#D##SLLBBBCCCCCCCCCCCCBBBLLS##D#........",  // 36
        ".........#D##SSLBBBBBBBBBBBBBBBBLSS##D#.........",  // 37
        "..........##D###SSLLBBBBBBBBLLSS###D##..........",  // 38
        "............####D###SSSSSSSS###D####............",  // 39
        "..................##........##..................",  // 40: Stretched ankle
        "..................##........##..................",  // 41: Ankle connection
        ".................#OO#......#OO#.................",  // 42: Foot upper
        "................#OOOO#....#OOOO#................",  // 43: Foot middle
        "...............#MM##MM#..#MM##MM#...............",  // 44: Baseline contact strictly at Y=44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    // Frame 3: Exhale Transition (Body settles back to Y=07..40, chest relaxes halfway)
    static readonly string[] Frame3 = new string[]
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
        "......#BBBBBB#BBCSCCLCCCCCCLCCSCBB#BBBBBB#......",  // 27: Relaxing back
        ".....#SB#BBB#BBCCCSSCCBCCBCCSSCCCBB#BBB#BS#.....",  // 28: Chest soft exhale
        ".....##L#B#BB#BCCCCCCCCBBCCCCCCCCB#BB#B#L##.....",  // 29
        ".....#D#SL#B#BBCCCCCCCCCCCCCCCCCCBB#B#LS#D#.....",  // 30
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

    // Idle Blink: Sweet peaceful closed eyes (seam at row 20 with soft shadow #SS#)
    static readonly string[] BlinkFrame = new string[]
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
        ".........#SLLFFFFFFFFFLBBLFFFFFFFFFLLS#.........",  // 17: Closed upper eyelid cream
        "........#SLLFFFFFFFFFFFLLFFFFFFFFFFFLLS#........",  // 18: Cream lid
        "........#LLBFFFFFFFFFFFFFFFFFFFFFFFFBLL#........",  // 19: Cream lid
        "........#LBBFFF##SS##FFMMFF##SS##FFFBBL#........",  // 20: Sweet closed eye crease (#SS#)
        "........#LBBFFFFFFFFFFMOOMFFFFFFFFFFBBL#........",  // 21: Lower lid cream
        "........#SLBBFFF####FFMOOMFF####FFFBBLS#........",  // 22: Socket lower outline
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
        "..................##........##..................",  // 41
        ".................#OO#......#OO#.................",  // 42
        "................#OOOO#....#OOOO#................",  // 43
        "...............#MM##MM#..#MM##MM#...............",  // 44
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
        Console.WriteLine("Generating complete IDLE Animation Set (4 frames + 1 blink)...");

        string baseDir = Directory.Exists("assets") ? Directory.GetCurrentDirectory() : AppDomain.CurrentDomain.BaseDirectory;
        string outDir = Path.Combine(baseDir, @"assets\sprites\idle");
        Directory.CreateDirectory(outDir);

        var frames = new Dictionary<string, string[]>()
        {
            { "frame_0.png", Frame0 },
            { "frame_1.png", Frame1 },
            { "frame_2.png", Frame2 },
            { "frame_3.png", Frame3 },
            { "idle_blink.png", BlinkFrame }
        };

        var bitmaps = new List<Bitmap>();
        var names = new List<string>();

        foreach (var kvp in frames)
        {
            Bitmap bmp = RenderGrid(kvp.Value);
            string fullPath = Path.Combine(outDir, kvp.Key);
            bmp.Save(fullPath, ImageFormat.Png);
            Console.WriteLine("Saved: {0}", fullPath);

            bitmaps.Add(bmp);
            names.Add(kvp.Key);
        }

        // Render combined 8x preview strip (5 frames side-by-side, 384x384 each -> 1920x384)
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

            string stripPath = Path.Combine(baseDir, @"assets\preview_idle_strip_8x.png");
            strip.Save(stripPath, ImageFormat.Png);
            Console.WriteLine("Saved 8x IDLE strip preview to: {0}", stripPath);
        }

        // Clean up
        foreach (var bmp in bitmaps)
        {
            bmp.Dispose();
        }

        Console.WriteLine("[OK] IDLE Animation Set generated successfully!");
    }
}
