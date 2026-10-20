using System;
using System.Drawing;
using System.Drawing.Imaging;

class CompareVisuals
{
    public static void Main()
    {
        string refPath = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\.user_uploaded\media_1792300480076_e208efda.png";
        string ourPath = @"assets\preview_profile_grid_8x.png";

        using (Bitmap refBmp = new Bitmap(refPath))
        using (Bitmap ourBmp = new Bitmap(ourPath))
        {
            // Create a side-by-side comparison with labels
            int cardW = 420;
            int cardH = 500;
            int totalW = cardW * 2 + 40;
            int totalH = cardH + 60;

            Bitmap comp = new Bitmap(totalW, totalH, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(comp))
            {
                g.Clear(Color.FromArgb(255, 15, 18, 24)); // dark background

                // Title Font
                Font titleFont = new Font("Segoe UI", 14, FontStyle.Bold);
                Font labelFont = new Font("Segoe UI", 11, FontStyle.Bold);
                Brush goldBrush = new SolidBrush(Color.FromArgb(255, 229, 169, 60));
                Brush cyanBrush = new SolidBrush(Color.FromArgb(255, 0, 229, 255));
                Brush whiteBrush = new SolidBrush(Color.FromArgb(255, 226, 232, 240));

                g.DrawString("СРАВНЕНИЕ: ЭТАЛОННЫЙ РЕФЕРЕНС vs ПРЕДЫДУЩАЯ МАТРИЦА", titleFont, goldBrush, 20, 16);

                // Left card: Reference
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 24, 30, 41)), 20, 60, cardW, cardH);
                g.DrawRectangle(new Pen(Color.FromArgb(255, 42, 52, 71)), 20, 60, cardW, cardH);
                g.DrawString("1. Эталонный референс (Целевой)", labelFont, cyanBrush, 36, 75);
                g.DrawImage(refBmp, new Rectangle(36, 110, 388, 388));

                // Right card: Our previous matrix
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 24, 30, 41)), 20 + cardW + 20, 60, cardW, cardH);
                g.DrawRectangle(new Pen(Color.FromArgb(255, 42, 52, 71)), 20 + cardW + 20, 60, cardW, cardH);
                g.DrawString("2. Результат из предоставленной сетки", labelFont, goldBrush, 36 + cardW + 20, 75);
                g.DrawImage(ourBmp, new Rectangle(36 + cardW + 20, 110, 388, 388));
            }

            string outPath = @"assets\comparison_reference_vs_matrix.png";
            string brainPath = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\comparison_reference_vs_matrix.png";
            comp.Save(outPath, ImageFormat.Png);
            comp.Save(brainPath, ImageFormat.Png);
            Console.WriteLine("Saved comparison: " + outPath);
        }
    }
}
