using System;
using System.Drawing;
using System.Drawing.Imaging;

class ThreeWayComparison
{
    public static void Main()
    {
        string refPath = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\.user_uploaded\media_1792300480076_e208efda.png";
        string oldPath = @"assets\preview_profile_grid_8x.png";
        string newPath = @"assets\preview_exact_ref_refined_8x.png";

        using (Bitmap refBmp = new Bitmap(refPath))
        using (Bitmap oldBmp = new Bitmap(oldPath))
        using (Bitmap newBmp = new Bitmap(newPath))
        {
            int cardW = 340;
            int cardH = 430;
            int totalW = cardW * 3 + 60;
            int totalH = cardH + 70;

            Bitmap comp = new Bitmap(totalW, totalH, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(comp))
            {
                g.Clear(Color.FromArgb(255, 15, 18, 24));

                Font titleFont = new Font("Segoe UI", 13, FontStyle.Bold);
                Font labelFont = new Font("Segoe UI", 10, FontStyle.Bold);
                Brush goldBrush = new SolidBrush(Color.FromArgb(255, 229, 169, 60));
                Brush cyanBrush = new SolidBrush(Color.FromArgb(255, 0, 229, 255));
                Brush redBrush = new SolidBrush(Color.FromArgb(255, 239, 68, 68));
                Brush greenBrush = new SolidBrush(Color.FromArgb(255, 34, 197, 94));
                Brush cardBg = new SolidBrush(Color.FromArgb(255, 24, 30, 41));
                Pen borderPen = new Pen(Color.FromArgb(255, 42, 52, 71));

                g.DrawString("ДЕТАЛЬНОЕ СРАВНЕНИЕ МОРДОЧКИ И СИЛУЭТА В ПРОФИЛЬ", titleFont, goldBrush, 20, 16);

                // 1. Reference
                int x1 = 20;
                g.FillRectangle(cardBg, x1, 55, cardW, cardH);
                g.DrawRectangle(borderPen, x1, 55, cardW, cardH);
                g.DrawString("1. Эталонный референс (Цель)", labelFont, cyanBrush, x1 + 16, 70);
                g.DrawImage(refBmp, new Rectangle(x1 + 16, 100, cardW - 32, cardW - 32));

                // 2. Old Matrix
                int x2 = x1 + cardW + 10;
                g.FillRectangle(cardBg, x2, 55, cardW, cardH);
                g.DrawRectangle(borderPen, x2, 55, cardW, cardH);
                g.DrawString("2. Предыдущая сетка (Ошибка)", labelFont, redBrush, x2 + 16, 70);
                g.DrawImage(oldBmp, new Rectangle(x2 + 16, 100, cardW - 32, cardW - 32));

                // 3. Perfect 48x48
                int x3 = x2 + cardW + 10;
                g.FillRectangle(cardBg, x3, 55, cardW, cardH);
                g.DrawRectangle(borderPen, x3, 55, cardW, cardH);
                g.DrawString("3. Исправленный 48×48 (Идеал)", labelFont, greenBrush, x3 + 16, 70);
                g.DrawImage(newBmp, new Rectangle(x3 + 16, 100, cardW - 32, cardW - 32));
            }

            string outPath = @"assets\preview_3way_comparison.png";
            string brainPath = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\preview_3way_comparison.png";
            comp.Save(outPath, ImageFormat.Png);
            comp.Save(brainPath, ImageFormat.Png);
            Console.WriteLine("Saved: " + outPath);
        }
    }
}
