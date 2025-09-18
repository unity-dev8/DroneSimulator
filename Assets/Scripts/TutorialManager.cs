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

    [Header("Checkpoint References")]
    public GameObject checkpointsParent; // Parent GameObject holding checkpoints
    private GameObject trackCheckpoint;
    private GameObject squareCheckpoint;

    private bool taskInProgress = false;

    private LandingPad landingPad;

    private bool hasEnteredGimbalMode = false;  // Flag for Gimbal Mode task
    private bool checkpointObjectiveCompleted = false; // Flag for Track Task
    private bool squareTaskCompleted = false; // Flag for Square Task

    private void Start()
    {
        StartCoroutine(AssignDroneReferences());
        landingPad = GameObject.Find("Landing_Pad").GetComponent<LandingPad>();

        // Initialize the Track and Square Checkpoints
        trackCheckpoint = checkpointsParent.transform.Find("TrackCheckpoint").gameObject;
        squareCheckpoint = checkpointsParent.transform.Find("SquareCheckpoint").gameObject;

        // Initially hide both checkpoints
        if (trackCheckpoint != null)
        {
            trackCheckpoint.SetActive(false); // Disable Track checkpoint initially
        }

        if (squareCheckpoint != null)
        {
            squareCheckpoint.SetActive(false); // Disable Square checkpoint initially
        }
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

        // Check for keypresses for gimbal mode
        CheckGimbalMode();

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

            case 9: // Track Objective (Complete Track)
                if (checkpointObjectiveCompleted)
                {
                    DisableTrackCheckpoint();
                    EnableSquareCheckpoint();
                    StartCoroutine(CompleteTask());
                }
                break;

            case 10: // Square Objective (Complete Square)
                if (squareTaskCompleted)
                {
                    StartCoroutine(CompleteTask());
                }
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
                // Once Gimbal Mode is entered, complete the task
                StartCoroutine(CompleteTask());
            }
        }
    }

    // ------------------- Checkpoint Task -------------------

    public void CheckpointReached()
    {
        if (currentTaskIndex == 9 && !checkpointObjectiveCompleted)
        {
            checkpointObjectiveCompleted = true;
            Debug.Log("Checkpoint Objective Completed!");
            StartCoroutine(CompleteTask());
        }
        if (currentTaskIndex == 10 && !squareTaskCompleted)
        {
            squareTaskCompleted = true;
            Debug.Log("Square Task Completed!");
            StartCoroutine(CompleteTask());
        }
    }

    // ------------------- Checkpoint Enable/Disable -------------------

    private void DisableTrackCheckpoint()
    {
        if (trackCheckpoint != null)
        {
            trackCheckpoint.SetActive(false); // Disable the track checkpoint
        }
    }

    private void EnableSquareCheckpoint()
    {
        if (squareCheckpoint != null)
        {
            squareCheckpoint.SetActive(true); // Enable the square checkpoint
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
