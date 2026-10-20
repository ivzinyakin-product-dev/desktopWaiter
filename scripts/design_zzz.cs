using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

class TestZzz
{
    static void Main()
    {
        int scale = 8;
        int w = 200, h = 100;
        using (Bitmap bmp = new Bitmap(w, h))
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.FromArgb(40, 44, 52)); // dark gray preview bg
            
            // Draw different Z designs
            Action<int, int, string[]> draw = (x0, y0, pat) => {
                for (int y = 0; y < pat.Length; y++) {
                    for (int x = 0; x < pat[y].Length; x++) {
                        char c = pat[y][x];
                        Color col = Color.Transparent;
                        if (c == '#') col = ColorTranslator.FromHtml("#1A0F0A");
                        else if (c == 'W') col = ColorTranslator.FromHtml("#F5ECE0");
                        else if (c == 'F') col = ColorTranslator.FromHtml("#EAD8BC");
                        else if (c == 'C') col = ColorTranslator.FromHtml("#D6BA98");
                        if (col.A > 0) {
                            using (SolidBrush b = new SolidBrush(col)) {
                                g.FillRectangle(b, (x0 + x) * scale, (y0 + y) * scale, scale, scale);
                            }
                        }
                    }
                }
            };

            // Option 1: Tiny 3x3 z (pure F/W)
            draw(2, 2, new string[] {
                "WWW",
                "..W",
                "WWW"
            });

            // Option 2: Tiny 3x4 z with dark base
            draw(8, 2, new string[] {
                "WWW",
                "..W",
                ".W.",
                "WWW"
            });

            // Option 3: Tiny 4x4 z with outline
            draw(14, 2, new string[] {
                "####",
                "#WW#",
                ".#W#",
                "####"
            });

            // Option 4: Crisp outlined 5x5 Z
            draw(2, 8, new string[] {
                "#####",
                "#WWW#",
                "..#W#",
                ".#W#.",
                "#WWW#",
                "#####"
            });

            // Option 5: Beautiful 6x6 Z
            draw(10, 8, new string[] {
                ".####.",
                "#WWWW#",
                "...#W#",
                "..#W#.",
                ".#W#..",
                "#WWWW#",
                ".####."
            });

            bmp.Save(@"assets\test_zzz_options.png", ImageFormat.Png);
            Console.WriteLine("Saved assets/test_zzz_options.png");
        }
    }
}
