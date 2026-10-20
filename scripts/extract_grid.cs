using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

class ExtractGrid
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

    static int ColorDist(Color c1, Color c2)
    {
        return Math.Abs(c1.R - c2.R) + Math.Abs(c1.G - c2.G) + Math.Abs(c1.B - c2.B);
    }

    static char MatchChar(Color c)
    {
        // Check transparent or background white
        if (c.A < 128 || (c.R > 240 && c.G > 240 && c.B > 240))
            return '.';

        int minDist = int.MaxValue;
        char best = '?';
        foreach (var kvp in Palette)
        {
            if (kvp.Key == '.') continue;
            int dist = ColorDist(c, kvp.Value);
            if (dist < minDist)
            {
                minDist = dist;
                best = kvp.Key;
            }
        }
        return best;
    }

    static void Main()
    {
        using (Bitmap bmp = new Bitmap("assets/test_48_nn.png"))
        {
            List<string> lines = new List<string>();
            for (int y = 0; y < bmp.Height; y++)
            {
                char[] row = new char[bmp.Width];
                for (int x = 0; x < bmp.Width; x++)
                {
                    row[x] = MatchChar(bmp.GetPixel(x, y));
                }
                lines.Add(string.Format("        \"{0}\",  // {1:D2}", new string(row), y));
            }
            File.WriteAllLines("assets/extracted_grid.txt", lines.ToArray());
            Console.WriteLine("Extracted {0} rows to assets/extracted_grid.txt", lines.Count);
        }
    }
}
