using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class LowResAssetValidator
{
    static int passed = 0;
    static int failed = 0;
    static int warnings = 0;

    static void Assert(bool condition, string testName, string details = "")
    {
        if (condition)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[PASS] ");
            Console.ResetColor();
            Console.WriteLine(testName);
            passed++;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[FAIL] ");
            Console.ResetColor();
            Console.WriteLine("{0} - {1}", testName, details);
            failed++;
        }
    }

    static void Warn(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[WARN] ");
        Console.ResetColor();
        Console.WriteLine(message);
        warnings++;
    }

    static void Main(string[] args)
    {
        Console.WriteLine("==========================================================");
        Console.WriteLine("   LOW-RES PIXEL ART ASSET VERIFICATION SUITE            ");
        Console.WriteLine("==========================================================");

        string baseDir = Directory.Exists("assets") ? Directory.GetCurrentDirectory() : AppDomain.CurrentDomain.BaseDirectory;
        if (!Directory.Exists(Path.Combine(baseDir, "assets")) && Directory.Exists(Path.Combine(baseDir, @"..\assets")))
        {
            baseDir = Path.GetFullPath(Path.Combine(baseDir, ".."));
        }

        string spritesDir = Path.Combine(baseDir, @"assets\sprites");
        string[] requiredStates = new string[] { "idle", "walk", "sleep", "fly" };

        int totalFoundFrames = 0;
        int standardWidth = -1;
        int standardHeight = -1;

        foreach (string state in requiredStates)
        {
            string statePath = Path.Combine(spritesDir, state);
            Assert(Directory.Exists(statePath), string.Format("Directory exists: assets/sprites/{0}", state));

            if (!Directory.Exists(statePath)) continue;

            string[] frameFiles = Directory.GetFiles(statePath, "*.png");
            if (frameFiles.Length == 0)
            {
                Warn(string.Format("State '{0}' has 0 frames (awaiting sprite generation).", state));
                continue;
            }

            totalFoundFrames += frameFiles.Length;

            for (int i = 0; i < frameFiles.Length; i++)
            {
                string f = frameFiles[i];
                using (Bitmap bmp = new Bitmap(f))
                {
                    if (standardWidth == -1)
                    {
                        standardWidth = bmp.Width;
                        standardHeight = bmp.Height;
                        Console.WriteLine(string.Format("--> Detected target resolution: {0}x{1} px", standardWidth, standardHeight));
                    }

                    Assert(bmp.Width == standardWidth && bmp.Height == standardHeight,
                        string.Format("Frame Resolution Uniformity: {0}/{1}", state, Path.GetFileName(f)),
                        string.Format("Got {0}x{1}, expected {2}x{3}", bmp.Width, bmp.Height, standardWidth, standardHeight));

                    // Verify transparency
                    bool hasAlpha = Image.IsAlphaPixelFormat(bmp.PixelFormat);
                    Assert(hasAlpha, string.Format("Alpha Channel: {0}/{1}", state, Path.GetFileName(f)));
                }
            }
        }

        Console.WriteLine("\n----------------------------------------------------------");
        if (totalFoundFrames == 0)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[INFO] Asset directory structure is properly configured.");
            Console.WriteLine("       No frames found yet (awaiting low-res sprite generation).");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine(string.Format("Total Frames Verified: {0}", totalFoundFrames));
            Console.WriteLine(string.Format("Passed: {0} | Failed: {1} | Warnings: {2}", passed, failed, warnings));
        }
        Console.WriteLine("==========================================================");
    }
}
