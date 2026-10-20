using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class VerifyFlightGrids
{
    static readonly Dictionary<char, Color> Palette = new Dictionary<char, Color>()
    {
        { '.', Color.FromArgb(0, 0, 0, 0) },            // Transparent
        { '#', ColorTranslator.FromHtml("#1A0F0A") },   // Outline
        { 'K', ColorTranslator.FromHtml("#120906") },   // Dark Pupil
        { 'D', ColorTranslator.FromHtml("#361F12") },   // Deep Plumage Shadow
        { 'S', ColorTranslator.FromHtml("#5A3820") },   // Feather Shadow
        { 'B', ColorTranslator.FromHtml("#7D5330") },   // Base Brown Body
        { 'L', ColorTranslator.FromHtml("#966840") },   // Light Brown Highlights
        { 'C', ColorTranslator.FromHtml("#D6BA98") },   // Beige Chest & Belly
        { 'F', ColorTranslator.FromHtml("#EAD8BC") },   // Light Cream Facial Disc
        { 'W', ColorTranslator.FromHtml("#F5ECE0") },   // Glint / Eye Highlight
        { 'Y', ColorTranslator.FromHtml("#FFA726") },   // Yellow-Amber Upper Iris
        { 'O', ColorTranslator.FromHtml("#F59E0B") },   // Orange Beak & Feet
        { 'R', ColorTranslator.FromHtml("#E65100") },   // Rich Orange Lower Iris
        { 'M', ColorTranslator.FromHtml("#B45309") }    // Beak & Talon Shadow
    };

    static readonly string[] Frame1_Up = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "...###............##............................",  // 07
        "...#SB#...........#B#.............##............",  // 08
        "...#SBB#..........#SB#...........#B#............",  // 09
        "....#SBB#..........#SB##.######..#SB##..........",  // 10
        "..###SSBB#.......####SBB#SSSSSS##SSSB#......##..",  // 11
        "..#BS#SSSB##.....#SBSSSBBBSSSBBBS#DBB#.....#B#..",  // 12
        "..#SBS#SS#BB##....##BBSSBBBBSSBBBBSBS#....#BS#..",  // 13
        "...#SBS#S#SBBB#....#########BSBBBSBS#....#LSD#..",  // 14
        "...#SSBS#SSSBLL#..#SSSLLFFFF#BSBSBS##...#LBD#...",  // 15
        "..####DBS##SSBL#.##SSLLFF###F#BSBB#C#..#BBS#B#..",  // 16
        "..#BBS#DS#BSSSLS##SSLLFFKMMMKFLBBKMKL##BBS#BD#..",  // 17
        "..#SBBBSS#BBSSLL##SSLFF#MOWKK#FLFKWMF#SS##BS#...",  // 18
        "...#SBBBBB##BSBLS#SSBFF#OOKKK#FFFKKOF#SDD#S##...",  // 19
        "....###SS#BBSSSLL#SSBFF#OYKKO#FMMKKYF#DD#S#B#...",  // 20
        "....#BB##S#BBBSBLL#SBFFFKYYYKFMOYKOKF#D#SSB#....",  // 21
        "....#SBBBBS##BSSBLS#BBFFF###FFMMO##FL#SS###.....",  // 22
        ".....##SS#SS#BBSSBBS#BBFFFFFFL#MM#FCS#S#SB#.....",  // 23
        ".......##LBSS#BBSSBBSSBBCFFFBCC##LCS#S#SS#......",  // 24
        "......#BBS#BBS#BBSSSBBSSBBBBCCCCLL##S#SB#.......",  // 25
        "......####BBSSS##BSSBBBBCLLLLLLLLCS##S##........",  // 26
        "........#BBS#BSSS#SBBBLCCCCCCCCCCCL#DS#.........",  // 27
        "........####BSBS#SBBBLCCBCBCBCBCBCS###..........",  // 28
        "...........#SS##SBBBBCCCCBCCCBCCCBC##...........",  // 29
        "...........####SSBBBLCCCCCCCCCCCCCC#............",  // 30
        "..............#SSBBBCCLCLCLCLCLCBCL#............",  // 31
        "..............#SSBBBCCCLCCCLCCCBCC#.............",  // 32
        "..............#SSSBBCCCCCCCCCCCCCL#.............",  // 33
        ".............##DSSBBBCCCLCCLCLCLC#..............",  // 34
        "............#BS#SSSBBLCCCCCCBCCCB#..............",  // 35
        "...........#BS###SSSSLLLCCCCCCLB#...............",  // 36
        "..........#BS#S###DS###LLLLLLB##................",  // 37
        ".........#BS#B#S####OMR#LLL###..................",  // 38
        ".........###BS#SS###OMO####MO#..................",  // 39
        "...........###BBS#S#M#M#.#M#M#..................",  // 40
        ".............#BS#SS####...###...................",  // 41
        ".............###.##.............................",  // 42
        "................................................",  // 43
        "................................................",  // 44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    static readonly string[] Frame2_MidUp = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "..................##............................",  // 07
        "..................#B#.............##............",  // 08
        "..................#SB#...........#B#............",  // 09
        ".###...............#SB##.######..#DB##..........",  // 10
        ".#BB##...........####SBB#SSSSSS##DDSB#..........",  // 11
        ".#SSBB##.........#SBSSSBBBSSSBBBS#DBB#..........",  // 12
        ".##SSSBB###.......##BBSSBBBBSSBBBBSBS#......###.",  // 13
        "..###SSSBBB##......#########BSBBBSBS#......#BB#.",  // 14
        "..#BS##SSSBBB##...#SSSBLFFFF#BSSSBS##....##BBS#.",  // 15
        "..#BBSS##DDSBBS#.##SSBBFF###F#BSBB#C#..##SBBS#..",  // 16
        "...#SBBSS#DDSBB#.#SSBBFFKMMMKFLBBKMKL##SBBSS#...",  // 17
        "..#S##BBS#BBSSBB##SSBFF#MRWKK#FBCKWMF#DBBSS###..",  // 18
        "..#BBS####SSBSSBS#SSBFF#OOKKK#FCFKKRF#BSS##SB#..",  // 19
        "...#BBBBSS#BBBSBB#SSBFF#OYKKO#FMMKKYF#D##SSB#...",  // 20
        "...####SBBS#BSSSBB#SBFFFKYYYKFMOYKOKF##S#####...",  // 21
        "....#BBBS##BBBSSSBS#BBFFF###FFMMO##CL#DD#SBB#...",  // 22
        "....#####SB###BBSBBS#BBFFFFFFL#MM#FCS#S#SBS#....",  // 23
        ".....#BSBBSSS#BBSSBSSSBBCFFFBCC##LCS#S#SD##.....",  // 24
        "......#BS##BBS#SBSSSBBSSBBBBCCCCLL##D#DSBB#.....",  // 25
        ".......#DBB##BS##BSSBBBBCLLLLLLLLCS##D##S#......",  // 26
        "........#S#BBS#SSDSBBBLCCCCCCCCCCCL#DDDS#.......",  // 27
        ".........#BBD#BBSSBBBLCCBCBCBCBCBCS##SD#........",  // 28
        "..........###BBS#BBBBCCCCSCCCBCCCSC#D##.........",  // 29
        ".............###SBBBLCCCCCCCCCCCCCC##...........",  // 30
        "..............#SSBBBCCLCLCLCLCLCBCL#............",  // 31
        "..............#SSBBBCCCBCCCLCCCSCC#.............",  // 32
        "..............#SSSBBCCCCCCCCCCCCCL#.............",  // 33
        ".............###SSBBBCCCLCCLCLCLC#..............",  // 34
        "............#BS#SSSBBLCCCCCCBCCCB#..............",  // 35
        "...........#BS###SSSSLLLCCCCCCLB#...............",  // 36
        "..........#BS#S###DS###LLLLLLB##................",  // 37
        ".........#BD#B#S####OMR#LLL###..................",  // 38
        ".........###BS#SS###OMO####MO#..................",  // 39
        "...........###BBS#S#M#M#.#M#M#..................",  // 40
        ".............#BS#SS####...###...................",  // 41
        ".............###.##.............................",  // 42
        "................................................",  // 43
        "................................................",  // 44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    static readonly string[] Frame3_MidDown = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "..................##............................",  // 07
        "..................#B#.............##............",  // 08
        "..................#SB#...........#B#............",  // 09
        "...................#SB##.######..#SB##..........",  // 10
        ".####............####SBB#SSSSSS##SSSB#..........",  // 11
        ".#SSB##..........#SBSSSBBBSSSBBBS#DBB#......##..",  // 12
        ".#DSSBB##.........##BBSSBBBBSSBBBBSBS#.....#B#..",  // 13
        "..#DSSBBB###.......#########BSBBBSBS#.....#BD#..",  // 14
        "...##SSSSBBB##....#SSSLLFFFF#BSBSBS##...##BS#...",  // 15
        ".######SS#SSBB#..##SSLLFF###F#BSBB#C#..#BBS###..",  // 16
        ".#SBBS#####SSBB#.#SSBLFFKMMMKFLBBKMKL##BBS#BD#..",  // 17
        "..#SBBBSS#BSSSBB##SSBFF#MOWKK#FLFKWMF#SS##BD#...",  // 18
        "...#SBBBS#BBSSBB##SSBFF#OOKKK#FFFKKRF#SDD#S##...",  // 19
        "....####BB##BSSBB#SSBFF#OYKKO#FMMKKYF#DD#S#B#...",  // 20
        "...#SBBBSS#BSSSBBS#SBFFFKYYYKFMOYKOKF#D#SSB#....",  // 21
        "....#SBBBBS#BBSSBBS#BBFFF###FFMMO##FL#SS###.....",  // 22
        ".....##SS#SS#BBSSBBS#BBFFFFFFL#MM#FCS#S#SB#.....",  // 23
        ".......##BBSS#BBSSBBSSBBCFFFBCC##LCS#S#DS#......",  // 24
        "......#BBSS#BS#BBSSSBBSSBBBBCCCCLL##S#SB#.......",  // 25
        ".......####BS#S##BSSBBBBCLLLLLLLLCS##D##........",  // 26
        "........#BBS#BSSS#SBBBLCCCCCCCCCCCL#DS#.........",  // 27
        ".........###BSBS#SBBBLCCBCBCBCBCBCS###..........",  // 28
        "............#S##SBBBBCCCCBCCCBCCCSC##...........",  // 29
        ".............##SSBBBLCCCCCCCCCCCCCC#............",  // 30
        "..............#SSBBBCCLCLCLCLCLCBCL#............",  // 31
        "..............#SSBBBCCCBCCCLCCCBCC#.............",  // 32
        "..............#SSSBBCCCCCCCCCCCCCL#.............",  // 33
        ".............##DSSBBBCCCLCCLCLCLC#..............",  // 34
        "............#BS#SSSBBLCCCCCCBCCCB#..............",  // 35
        "...........#BS###SSSSLLLCCCCCCLB#...............",  // 36
        "..........#BS#S###DS###LLLLLLB##................",  // 37
        ".........#BS#B#S####OMR#LLL###..................",  // 38
        ".........###BS#SS###OMO####MO#..................",  // 39
        "...........###BBS#S#M#M#.#M#M#..................",  // 40
        ".............#BS#SS####...###...................",  // 41
        ".............###.##.............................",  // 42
        "................................................",  // 43
        "................................................",  // 44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    static readonly string[] Frame4_Down = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        "..................##............................",  // 07
        "..................#B#.............##............",  // 08
        "..................#SB#...........#B#............",  // 09
        "...................#SB##.######..#SB##..........",  // 10
        ".................####SBB#SSSSSS##DDSB#..........",  // 11
        ".................#SBSSSBBBSSSBBBS#DBB#..........",  // 12
        "..................##BBSSBBBBSSBBBBSBS#..........",  // 13
        "...................#########BSBBBSBS#...........",  // 14
        "..................#SSSLLFFFF#BSBSBS##...........",  // 15
        ".................##SSLLFF###F#BSBB#C#...........",  // 16
        ".................#SSBBFFKMMMKFLBBKMKL#..........",  // 17
        ".................#SSBFF#MOWKK#FLFKWMF#..........",  // 18
        ".................#SSBFF#OOKKK#FFFKKRF#..........",  // 19
        "............######SSBFF#OYKKO#FMMKKYF#..........",  // 20
        "..........##SBBBBS#SBFFFKYYYKFMOYKOKF#..........",  // 21
        ".........#SBBBBBBBD#BBFFF###FFMMO##FL#..........",  // 22
        "........#BBBSSSBSBSS#BBFFFFFFL#MM#FCS##.........",  // 23
        ".......#BBSSSSBBDDSSSSBBCFFFBCC##LCS#BS#........",  // 24
        "......#BBSSBSBB##SSSBBSSBBBBCCCCLL##DDBB#.......",  // 25
        ".....#BBSSSSBB##SBSSBBBBCLLLLLLLLCS##DDBB#......",  // 26
        ".....#BS#BSBB##SB#SBBBLCCCCCCCCCCCL#D#DSB#......",  // 27
        "....#BS#BSBB##SSS#BBBLCCBCBCBCBCBCS##D#DBB#.....",  // 28
        "...#SB#S#BBS#SSB#BBBBCCCCBCCCBCCCSC##DD#DBB#....",  // 29
        "...#B#SB#BS#DBS#SBBBLCCCCCCCCCCCCCC##DS#DSB#....",  // 30
        "..#BS#B#BS#SD##SSBBBCCLCLCLCLCLCBCL#B#SS#SB#....",  // 31
        "..###B#SB#SB#.#SSBBBCCCBCCCLCCCBCC#DB#DB#SSB#...",  // 32
        "...#SB#BS#B#..#SSSBBCCCCCCCCCCCCCL#DB#DSB#SB#...",  // 33
        "...#B#BB###..###SSBBBCCCLCCLCLCLC#.##.#SB#SSB#..",  // 34
        "...##SBS#...#BS#SSSBBLCCCCCCBCCCB#....##SB#SB#..",  // 35
        "....#BS#...#BS###SSSSLLLCCCCCCLB#.......##.#S#..",  // 36
        "....#B#...#BS#S###DS###LLLLLLB##...........###..",  // 37
        "....##...#BS#B#S####OMR#LLL###..................",  // 38
        ".........###BS#SS###OMO####MO#..................",  // 39
        "...........###BBS#S#M#M#.#M#M#..................",  // 40
        ".............#BS#SS####...###...................",  // 41
        ".............###.##.............................",  // 42
        "................................................",  // 43
        "................................................",  // 44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    static Bitmap RenderGrid(string[] grid)
    {
        if (grid.Length != 48) throw new Exception("Rows != 48!");
        Bitmap bmp = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
        for (int y = 0; y < 48; y++)
        {
            if (grid[y].Length != 48) throw new Exception(string.Format("Row {0} length != 48: {1}", y, grid[y].Length));
            for (int x = 0; x < 48; x++)
            {
                char ch = grid[y][x];
                if (!Palette.ContainsKey(ch)) throw new Exception(string.Format("Unknown char '{0}' at ({1},{2})", ch, x, y));
                bmp.SetPixel(x, y, Palette[ch]);
            }
        }
        return bmp;
    }

    static void Main()
    {
        Console.WriteLine("Verifying Flight Grids 48x48...");

        var rawGrids = new Dictionary<string, string[]>()
        {
            { "frame_0.png", Frame1_Up },
            { "frame_1.png", Frame2_MidUp },
            { "frame_2.png", Frame3_MidDown },
            { "frame_3.png", Frame4_Down }
        };

        string outDir = @"assets\sprites\fly";
        Directory.CreateDirectory(outDir);

        var bitmaps = new List<Bitmap>();

        foreach (var kvp in rawGrids)
        {
            Bitmap bmp = RenderGrid(kvp.Value);
            string fullPath = Path.Combine(outDir, kvp.Key);
            bmp.Save(fullPath, ImageFormat.Png);
            bitmaps.Add(bmp);
            Console.WriteLine("Validated & saved: " + fullPath);
        }

        // Render 8x Preview Strip (4 frames side-by-side, 384x384 each -> 1536x384)
        int scale = 8;
        int frameW = 48 * scale;
        int frameH = 48 * scale;
        using (Bitmap strip = new Bitmap(frameW * 4, frameH, PixelFormat.Format32bppArgb))
        using (Graphics g = Graphics.FromImage(strip))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

            for (int i = 0; i < 4; i++)
            {
                g.DrawImage(bitmaps[i], i * frameW, 0, frameW, frameH);
            }

            string stripPath = @"assets\preview_flight_strip_8x.png";
            strip.Save(stripPath, ImageFormat.Png);
            string brainStrip = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\preview_flight_strip_8x.png";
            strip.Save(brainStrip, ImageFormat.Png);
            Console.WriteLine("Saved 8x strip to: " + stripPath);
        }

        Console.WriteLine("[OK] All 4 Flight frames validated 100%!");
    }
}
