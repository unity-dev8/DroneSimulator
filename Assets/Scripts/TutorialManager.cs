using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("Task Setup")]
    [TextArea]
    public string[] tutorialTasks;   // Set task list in Inspector
    private int currentTaskIndex = 0;

    [Header("UI Reference")]
    public TextMeshProUGUI taskText;  // Assign TMP text in Inspector
    public GameObject tutorialUI;     // UI Image container

    [Header("References")]
    public DRONECONT droneController;
    public FSJoystickInput joystickInput;

    private bool taskInProgress = false;

    private LandingPad landingPad;

    private bool hasEnteredGimbalMode = false;  // Flag for Gimbal Mode task
    private bool hasTakenPhoto = false;  // Flag for Photo Mode task
    private bool hasDrawnSquare = false;  // Flag for Square Draw task
    private bool checkpointObjectiveCompleted = false; // Flag for Checkpoint Task

    private void Start()
    {
        StartCoroutine(AssignDroneReferences());
        landingPad = GameObject.Find("Landing_Pad").GetComponent<LandingPad>();
    }

    private IEnumerator AssignDroneReferences()
    {
        // Wait one frame to let DRONECONT.Start() set the tag
        yield return null;

        GameObject droneObj = GameObject.FindGameObjectWithTag("Player");
        if (droneObj != null)
        {
            droneController = droneObj.GetComponent<DRONECONT>();
            joystickInput = droneObj.GetComponent<FSJoystickInput>();
        }
        else
        {
            Debug.LogError("❌ No Drone with tag 'Drone' found in the scene!");
        }

        if (tutorialTasks.Length > 0 && taskText != null)
        {
            taskText.text = tutorialTasks[0];
        }
    }

    private void Update()
    {
        if (droneController == null) return; // ✅ Prevent null issues
        if (currentTaskIndex >= tutorialTasks.Length || taskInProgress) return;

        // Check for keypresses for photo mode and gimbal mode
        CheckGimbalMode();
        CheckPhotoMode();
        CheckSquareDraw();

        // Task checks
        switch (currentTaskIndex)
        {
            case 0: // Arm the drone
                if (droneController.startupDone)
                    StartCoroutine(CompleteTask());
                break;

            case 1: // Ascend
                if (droneController.finalVertical > 0.1f)
                    StartCoroutine(CompleteTask());
                break;

            case 2: // Descend
                if (droneController.finalVertical < -0.1f)
                    StartCoroutine(CompleteTask());
                break;

            case 3: // Pitch
                if (Mathf.Abs(droneController.finalHorizontalZ) > 0.1f)
                    StartCoroutine(CompleteTask());
                break;

            case 4: // Roll
                if (Mathf.Abs(droneController.finalHorizontalX) > 0.1f)
                    StartCoroutine(CompleteTask());
                break;

            case 5: // Yaw
                if (Mathf.Abs(droneController.finalYaw) > 0.1f)
                    StartCoroutine(CompleteTask());
                break;

            case 6: // Land (grounded again)
                if (droneController.inGround && landingPad.isLanding == true)
                    StartCoroutine(CompleteTask());
                break;

            case 7: // Disarm (turn off)
                if (!droneController.startupDone)
                    StartCoroutine(CompleteTask());
                break;

            case 8: // Gimbal Mode (press 7)
                if (hasEnteredGimbalMode)
                    StartCoroutine(CompleteTask());
                break;

            case 9: // Photo Mode (press P)
                if (hasTakenPhoto)
                    StartCoroutine(CompleteTask());
                break;

            case 10: // Checkpoint Objective (Player passes through a checkpoint)
                if (checkpointObjectiveCompleted)
                    StartCoroutine(CompleteTask());
                break;
        }
    }

    // ------------------ Check for Key Presses ------------------

    private void CheckGimbalMode()
    {
        if (currentTaskIndex == 8 && !hasEnteredGimbalMode)
        {
            if (Input.GetKeyDown(KeyCode.Alpha7))  // When '7' is pressed
            {
                hasEnteredGimbalMode = true;
                Debug.Log("Gimbal Mode Entered!");
            }
        }
    }

    private void CheckPhotoMode()
    {
        if (currentTaskIndex == 9 && !hasTakenPhoto)
        {
            if (Input.GetKeyDown(KeyCode.P))  // When 'P' is pressed
            {
                hasTakenPhoto = true;
                Debug.Log("Photo Mode Activated!");
            }
        }
    }

    private void CheckSquareDraw()
    {
        if (currentTaskIndex == 10 && !hasDrawnSquare)
        {
            // Assuming you have a function for detecting if the square is drawn
            if (droneController.HasDrawnSquare)
            {
                hasDrawnSquare = true;
                Debug.Log("Square Drawn!");
                StartCoroutine(CompleteTask());
            }
        }
    }

    // ------------------- Checkpoint Task -------------------

    public void CheckpointReached()
    {
        if (currentTaskIndex == 10 && !checkpointObjectiveCompleted)
        {
            checkpointObjectiveCompleted = true;
            Debug.Log("Checkpoint Objective Completed!");
            StartCoroutine(CompleteTask());
        }
    }

    // ------------------- Objective Management -------------------

    private IEnumerator CompleteTask()
    {
        taskInProgress = true;
        yield return new WaitForSeconds(2.0f);  // Optional delay for task completion

        currentTaskIndex++;

        if (currentTaskIndex < tutorialTasks.Length)
        {
            taskText.text = tutorialTasks[currentTaskIndex];
        }
        else
        {
            Debug.Log("Tutorial Completed!");
            if (tutorialUI != null)
                Destroy(tutorialUI);  // Hide tutorial UI after completion
        }

        taskInProgress = false;
    }
}
