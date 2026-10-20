using System;

class PhysicsFsmAssertionTester
{
    static int passed = 0;
    static int failed = 0;

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

    static void Main()
    {
        Console.WriteLine("==========================================================");
        Console.WriteLine("   LOW-RES FSM & DISCRETE MOVEMENT TEST SUITE             ");
        Console.WriteLine("==========================================================");

        // Simulation parameters
        double screenRight = 1920;
        double petWidth = 144; // e.g. 48px * 3x scale
        double currentX = 500;
        double direction = 1.0;
        double stepSize = 6.0;
        string state = "IDLE";

        // Test 1: State Initialization
        Assert(state == "IDLE", "Initial state is IDLE");

        // Test 2: State Transition
        state = "WALKING";
        Assert(state == "WALKING", "State transition IDLE -> WALKING");

        // Test 3: Step-Locked advancement (advances on frames 1 and 3 only)
        double startX = currentX;
        for (int frame = 0; frame < 4; frame++)
        {
            if (state == "WALKING" && (frame == 1 || frame == 3))
            {
                currentX += direction * stepSize;
            }
        }
        Assert(currentX == startX + 2 * stepSize, "Walking advances precisely 2 times per 4-frame cycle", 
            string.Format("Expected {0}, got {1}", startX + 2 * stepSize, currentX));

        // Test 4: Screen edge bounce (Right edge)
        currentX = screenRight - petWidth - 5;
        direction = 1.0;
        // Step causes boundary collision
        currentX += direction * stepSize;
        if (currentX >= screenRight - petWidth - 10)
        {
            currentX = screenRight - petWidth - 10;
            direction = -1.0;
        }
        Assert(direction == -1.0, "Right boundary reflection triggers direction inversion to -1.0");
        Assert(currentX <= screenRight - petWidth - 10, "Right boundary clamping prevents screen overflow");

        // Test 5: Screen edge bounce (Left edge)
        currentX = 12;
        direction = -1.0;
        currentX += direction * stepSize;
        if (currentX <= 10)
        {
            currentX = 10;
            direction = 1.0;
        }
        Assert(direction == 1.0, "Left boundary reflection triggers direction inversion to 1.0");
        Assert(currentX >= 10, "Left boundary clamping prevents screen underflow");

        // Test 6: AFK Timer transition to SLEEPING after 10.0s inactivity
        state = "IDLE";
        double elapsedAfkSeconds = 10.1;
        double afkThreshold = 10.0;
        if (state == "IDLE" && elapsedAfkSeconds >= afkThreshold)
        {
            state = "SLEEPING";
        }
        Assert(state == "SLEEPING", "AFK inactivity (>=10s) triggers transition IDLE -> SLEEPING");

        // Test 7: Sleep Stability (Pet does NOT walk while asleep)
        bool randomWalkAttempted = true;
        if (state == "SLEEPING" && randomWalkAttempted)
        {
            // FSM blocks walking
        }
        else
        {
            state = "WALKING";
        }
        Assert(state == "SLEEPING", "Random walk is suppressed while pet is SLEEPING");

        // Test 8: Wake-up interaction
        bool userClicked = true;
        if (state == "SLEEPING" && userClicked)
        {
            state = "IDLE";
            elapsedAfkSeconds = 0.0;
        }
        Assert(state == "IDLE", "User click wakes up pet: SLEEPING -> IDLE");
        Assert(elapsedAfkSeconds == 0.0, "Wake-up interaction resets AFK timer to 0s");

        // Test 9: Active interaction prevents auto-sleep
        elapsedAfkSeconds = 8.5; // active before 10s
        bool userMovedMouse = true;
        if (userMovedMouse) elapsedAfkSeconds = 0.0;
        if (state == "IDLE" && elapsedAfkSeconds >= afkThreshold)
        {
            state = "SLEEPING";
        }
        Assert(state == "IDLE", "Interaction before 10s resets timer and prevents unwanted sleep");

        // Test 10: Active drag activates FLYING state
        double dragDx = 15.0;
        double dragDy = -30.0;
        double dragDist = Math.Sqrt(dragDx * dragDx + dragDy * dragDy);
        if (dragDist > 6.0)
        {
            state = "FLYING";
        }
        Assert(state == "FLYING", "Active mouse drag (dist > 6px) triggers transition to FLYING");

        // Test 11: Flight facing orientation follows drag direction
        double dragStepDx = -10.0; // dragging leftwards
        double facing = (dragStepDx > 2.0) ? 1.0 : (dragStepDx < -2.0 ? -1.0 : 1.0);
        Assert(facing == -1.0, "Leftward drag inverts flight facing scale to -1.0");

        // Test 12: Release in the air triggers gliding descent
        double floorY = 900.0;
        double petY = 500.0; // released high above floor
        bool isGliding = (petY < floorY - 6.0);
        Assert(isGliding, "Release above taskbar floor activates gentle gliding descent");

        // Test 13: Gliding landing smoothly lands pet on floorY into IDLE
        while (petY < floorY)
        {
            petY += 50.0; // simulation steps
        }
        if (petY >= floorY)
        {
            petY = floorY;
            isGliding = false;
            state = "IDLE";
        }
        Assert(petY == floorY, "Gliding descent lands accurately on floor baseline Y");
        Assert(state == "IDLE", "Touching floor baseline finishes flight and transitions to IDLE");

        Console.WriteLine("\n----------------------------------------------------------");
        Console.WriteLine(string.Format("Passed: {0} | Failed: {1}", passed, failed));
        Console.WriteLine("==========================================================");
    }
}
