using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

class UpdateProfileSprites
{
    public static void Main()
    {
        string refinedPath = @"assets\preview_exact_ref_refined_8x.png";
        using (Bitmap p8x = new Bitmap(refinedPath))
        {
            // Downsample 8x to native 48x48
            Bitmap bmpRight = new Bitmap(48, 48, PixelFormat.Format32bppArgb);
            Bitmap bmpLeft = new Bitmap(48, 48, PixelFormat.Format32bppArgb);

            for (int y = 0; y < 48; y++)
            {
                for (int x = 0; x < 48; x++)
                {
                    Color c = p8x.GetPixel(x * 8 + 4, y * 8 + 4);
                    bmpRight.SetPixel(x, y, c);
                    bmpLeft.SetPixel(47 - x, y, c);
                }
            }

            string profileDir = @"assets\sprites\walk_profile";
            string fileRight = Path.Combine(profileDir, "owl_profile_right.png");
            string fileLeft = Path.Combine(profileDir, "owl_profile_left.png");
            string fileFrame0 = Path.Combine(profileDir, "frame_0.png");

            bmpRight.Save(fileRight, ImageFormat.Png);
            bmpLeft.Save(fileLeft, ImageFormat.Png);
            bmpRight.Save(fileFrame0, ImageFormat.Png);

            // Create side-by-side 8x
            Bitmap pLeft8x = new Bitmap(384, 384, PixelFormat.Format32bppArgb);
            for (int y = 0; y < 48; y++)
                for (int x = 0; x < 48; x++)
                {
                    Color col = bmpLeft.GetPixel(x, y);
                    for (int dy = 0; dy < 8; dy++)
                        for (int dx = 0; dx < 8; dx++)
                            pLeft8x.SetPixel(x * 8 + dx, y * 8 + dy, col);
                }

            int spacing = 32;
            Bitmap sideBySide = new Bitmap(384 * 2 + spacing, 384, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(sideBySide))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(pLeft8x, 0, 0);
                g.DrawImage(p8x, 384 + spacing, 0);
            }

            string previewLR = @"assets\preview_profile_left_right_8x.png";
            string brainLR = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\preview_profile_left_right_8x.png";
            string previewGrid = @"assets\preview_profile_grid_8x.png";
            string brainGrid = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\preview_profile_grid_8x.png";

            sideBySide.Save(previewLR, ImageFormat.Png);
            sideBySide.Save(brainLR, ImageFormat.Png);
            p8x.Save(previewGrid, ImageFormat.Png);
            p8x.Save(brainGrid, ImageFormat.Png);

            Console.WriteLine("Profile sprites and previews updated successfully!");
        }
    }
}
