using System;
using System.IO;
using System.Drawing;

class MeasureAnatomy
{
    static void Main()
    {
        string[] states = new string[] { "idle", "walk", "sleep", "fly" };
        foreach (string st in states)
        {
            string f = Path.Combine(@"assets\sprites", st, "frame_0.png");
            using (Bitmap bmp = new Bitmap(f))
            {
                // Measure facial disc (F/Cream pixels), eye pupil/iris (Y, R, K, W), beak (O, M)
                int creamCount = 0;
                int irisCount = 0;
                int pupilCount = 0;
                int beakCount = 0;

                int eyeMinX = 999, eyeMaxX = -1, eyeMinY = 999, eyeMaxY = -1;
                int beakMinX = 999, beakMaxX = -1, beakMinY = 999, beakMaxY = -1;
                int faceMinX = 999, faceMaxX = -1, faceMinY = 999, faceMaxY = -1;

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color c = bmp.GetPixel(x, y);
                        string hex = ColorTranslator.ToHtml(c);

                        // Face disc: #EAD8BC (F)
                        if (hex == "#EAD8BC")
                        {
                            creamCount++;
                            if (x < faceMinX) faceMinX = x;
                            if (x > faceMaxX) faceMaxX = x;
                            if (y < faceMinY) faceMinY = y;
                            if (y > faceMaxY) faceMaxY = y;
                        }
                        // Eye Iris & glint: #FFA726 (Y), #E65100 (R), #F5ECE0 (W) in eye region
                        if (hex == "#FFA726" || hex == "#E65100" || (hex == "#F5ECE0" && y > 10 && y < 25))
                        {
                            irisCount++;
                            if (x < eyeMinX) eyeMinX = x;
                            if (x > eyeMaxX) eyeMaxX = x;
                            if (y < eyeMinY) eyeMinY = y;
                            if (y > eyeMaxY) eyeMaxY = y;
                        }
                        // Pupil: #120906 (K)
                        if (hex == "#120906") pupilCount++;

                        // Beak: #F59E0B (O), #B45309 (M) in row 15..26
                        if ((hex == "#F59E0B" || hex == "#B45309") && y >= 17 && y <= 26)
                        {
                            beakCount++;
                            if (x < beakMinX) beakMinX = x;
                            if (x > beakMaxX) beakMaxX = x;
                            if (y < beakMinY) beakMinY = y;
                            if (y > beakMaxY) beakMaxY = y;
                        }
                    }
                }

                Console.WriteLine("==================================================");
                Console.WriteLine("State: {0}", st.ToUpper());
                Console.WriteLine("  Facial Disc: X=[{0}..{1}] (W={2}), Y=[{3}..{4}] (H={5}) | Count={6}",
                    faceMinX, faceMaxX, faceMaxX - faceMinX + 1, faceMinY, faceMaxY, faceMaxY - faceMinY + 1, creamCount);
                Console.WriteLine("  Eye Area:    X=[{0}..{1}] (W={2}), Y=[{3}..{4}] (H={5}) | Iris={6}, Pupil={7}",
                    eyeMinX, eyeMaxX, eyeMaxX - eyeMinX + 1, eyeMinY, eyeMaxY, eyeMaxY - eyeMinY + 1, irisCount, pupilCount);
                Console.WriteLine("  Beak:        X=[{0}..{1}] (W={2}), Y=[{3}..{4}] (H={5}) | Count={6}",
                    beakMinX, beakMaxX, beakMaxX - beakMinX + 1, beakMinY, beakMaxY, beakMaxY - beakMinY + 1, beakCount);
            }
        }
    }
}
