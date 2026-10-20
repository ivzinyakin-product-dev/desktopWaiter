using System;
using System.Drawing;
using System.Collections.Generic;

class GridDetector
{
    public static void Main()
    {
        string refPath = @"C:\Users\ZEPHYRUS\.gemini\antigravity\brain\5833e5db-752d-4247-9509-16bb4534a9be\.user_uploaded\media_1792300480076_e208efda.png";
        using (Bitmap src = new Bitmap(refPath))
        {
            int[] rows = new int[] { 250, 300, 350, 400, 450, 500, 550, 600, 650, 700 };
            Dictionary<int, int> runCounts = new Dictionary<int, int>();

            foreach (int y in rows)
            {
                int currentRun = 1;
                for (int x = 1; x < src.Width; x++)
                {
                    Color c1 = src.GetPixel(x - 1, y);
                    Color c2 = src.GetPixel(x, y);
                    int diff = Math.Abs(c1.R - c2.R) + Math.Abs(c1.G - c2.G) + Math.Abs(c1.B - c2.B);
                    if (diff < 20)
                    {
                        currentRun++;
                    }
                    else
                    {
                        if (currentRun >= 15 && currentRun <= 80)
                        {
                            if (!runCounts.ContainsKey(currentRun)) runCounts[currentRun] = 0;
                            runCounts[currentRun]++;
                        }
                        currentRun = 1;
                    }
                }
            }

            Console.WriteLine("Dominant run lengths:");
            var sorted = new List<KeyValuePair<int, int>>(runCounts);
            sorted.Sort((a, b) => b.Value.CompareTo(a.Value));
            for (int i = 0; i < Math.Min(15, sorted.Count); i++)
            {
                Console.WriteLine(string.Format("Length {0} px: count {1}", sorted[i].Key, sorted[i].Value));
            }
        }
    }
}
