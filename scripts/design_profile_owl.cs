using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class DesignProfileOwl
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

    // Profile base frame (Facing Right, 90 degrees)
    static readonly string[] Grid = new string[]
    {
        "................................................",  // 00
        "................................................",  // 01
        "................................................",  // 02
        "................................................",  // 03
        "................................................",  // 04
        "................................................",  // 05
        "................................................",  // 06
        ".................##.............................",  // 07: Ear tuft tip
        "................#L##............................",  // 08
        "................#SLL#...........................",  // 09
        ".................#SLL##...#######...............",  // 10: Head crown arch
        "...............###SLLL##SBBBBBBBBS#.............",  // 11
        "..............#LBSSSLLLL#BBBBBBBBBB#............",  // 12
        ".............#SLLSSSSSLB#BBBBBBBBBBB#...........",  // 13
        "............########SBBSBBBBBBBBBBBBB#..........",  // 14
        "...........#SLLFFFF##BBSSBBBBFFFFFFFF#..........",  // 15: Brow & face disc
        "..........#SLLFFFF####BB##FFFF###FFFFF#.........",  // 16: Cream cushion over eye
        ".........#SLLFFFFF#RR##FFFFFF#RR##FFFF#.........",  // 17
        "........#SLLFFFFFF#RWKK#FFFF#RWKK#FFFF#.........",  // 18
        "........#LLBFFFFFF#YKKK#FFFF#YKKK#FFF#MM#.......",  // 19
        "........#LBBFFFFFF#YKKW#FFFF#YKKW#FF#MOOO#......",  // 20
        "........#LBBFFFFFF#ROOO#FFFF#ROOO#FF#MOOOO#.....",  // 21
        "........#SLBBFFFFF####FFFFFFF####FFFF#MM##......",  // 22
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
        "...................##........##.................",  // 41
        "..................#MM#......#OO#................",  // 42
        ".................#MMMM#....#OOOO#...............",  // 43
        "................#DD##DD#..#MM##MM#..............",  // 44
        "................................................",  // 45
        "................................................",  // 46
        "................................................"   // 47
    };

    static void Main()
    {
        Console.WriteLine("Designing Profile Owl...");
    }
}
