using System;
using System.IO;
using System.Drawing;

class DetailAnatomy
{
    static void Main()
    {
        string[] states = new string[] { "idle", "walk", "sleep", "fly" };
        foreach (string st in states)
        {
            string f = Path.Combine(@"assets\sprites", st, "frame_0.png");
            using (Bitmap bmp = new Bitmap(f))
            {
                // Measure eye center (iris/pupil)
                int eyeSumX = 0, eyeSumY = 0, eyePts = 0;
                int beakSumX = 0, beakSumY = 0, beakPts = 0;
                int bodySumX = 0, bodySumY = 0, bodyPts = 0;

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color c = bmp.GetPixel(x, y);
                        if (c.A > 0)
                        {
                            // Exclude Zzz in sleep (rows < 10, cols > 25)
                            if (st == "sleep" && y < 10 && x > 25) continue;

                            bodySumX += x; bodySumY += y; bodyPts++;

                            string hex = ColorTranslator.ToHtml(c);
                            if (hex == "#FFA726" || hex == "#E65100" || hex == "#120906")
                            {
                                if (y >= 12 && y <= 22)
                                {
                                    eyeSumX += x; eyeSumY += y; eyePts++;
                                }
                            }
                            if ((hex == "#F59E0B" || hex == "#B45309") && y >= 17 && y <= 25 && x >= 15 && x <= 40)
                            {
                                beakSumX += x; beakSumY += y; beakPts++;
                            }
                        }
                    }
                }

                double eyeAvgX = eyePts > 0 ? (double)eyeSumX / eyePts : -1;
                double eyeAvgY = eyePts > 0 ? (double)eyeSumY / eyePts : -1;
                double beakAvgX = beakPts > 0 ? (double)beakSumX / beakPts : -1;
                double beakAvgY = beakPts > 0 ? (double)beakSumY / beakPts : -1;
                double bodyAvgX = bodyPts > 0 ? (double)bodySumX / bodyPts : -1;
                double bodyAvgY = bodyPts > 0 ? (double)bodySumY / bodyPts : -1;

                Console.WriteLine("{0,-6} | Pixels: {1,4} | Eye Center: ({2:F1}, {3:F1}) | Beak Center: ({4:F1}, {5:F1}) | Body Center: ({6:F1}, {7:F1})",
                    st.ToUpper(), bodyPts, eyeAvgX, eyeAvgY, beakAvgX, beakAvgY, bodyAvgX, bodyAvgY);
            }
        }
    }
}
