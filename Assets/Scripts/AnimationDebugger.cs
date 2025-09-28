using UnityEngine;

public class AnimationDebugger : MonoBehaviour
{
    [Header("--- DRAG THE OBJECTS HERE ---")]
    [Tooltip("Drag the PlayerShip GameObject here.")]
    [SerializeField] private PlayerController playerController;

    [Tooltip("Drag the ThrusterFire GameObject here.")]
    [SerializeField] private Animator thrusterAnimator;

    void Update()
    {
        // --- Test 1: Are our references working? ---
        if (playerController == null)
        {
            Debug.LogError("!!! CRITICAL ERROR: PlayerController reference is MISSING on the debugger!");
            return;
        }
        if (thrusterAnimator == null)
        {
            Debug.LogError("!!! CRITICAL ERROR: ThrusterFire Animator reference is MISSING on the debugger!");
            return;
        }

        // --- Test 2: What command is the PlayerController sending? ---
        // We will manually re-create the logic to be 100% sure.
        int expectedState = 0;
        if (playerController.IsProvidingInput && playerController.CurrentFuel > 0)
        {
            // Check for boost specifically. We need to access the input actions.
            // This is a simplified check for debugging.
            bool isBoosting = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftShift);
            if (isBoosting)
            {
                expectedState = 2; // Boost
            }
            else
            {
                expectedState = 1; // Thrust
            }
        }

        // --- Test 3: What is the Animator's CURRENT state? ---
        // This asks the engine: "What integer value is currently stored in your 'ThrustState' parameter?"
        int actualState = thrusterAnimator.GetInteger("ThrustState");

        // --- Test 4: Which animation clip is ACTUALLY playing? ---
        // This is the ultimate proof.
        string currentClipName = "None";
        if (thrusterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Thruster_Idle"))
        {
            currentClipName = "IDLE";
        }
        else if (thrusterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Thruster_Thrust"))
        {
            currentClipName = "THRUST";
        }
        else if (thrusterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Thruster_Boost"))
        {
            currentClipName = "BOOST";
        }

        // --- FINAL REPORT ---
        Debug.Log($"--- FRAME REPORT --- \n" +
                  $"PlayerController wants state: <color=yellow>{expectedState}</color>\n" +
                  $"Animator parameter 'ThrustState' is currently: <color=orange>{actualState}</color>\n" +
                  $"Animator is ACTUALLY playing: <color=cyan>{currentClipName}</color>");
    }
}