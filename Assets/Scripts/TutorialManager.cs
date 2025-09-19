using System;
using System.Collections;
using TMPro;
using UnityEngine;

public enum TaskState
{
    ArmTheDrone = 0,
    Ascend = 1,
    Descend = 2,
    Pitch = 3,
    Roll = 4,
    Yaw = 5,
    EnterGimbalMode = 6,
    TrackObjective = 7,
    SquareObjective = 8,
    Land = 9,
    Disarm = 10,

}

public class TutorialManager : MonoBehaviour
{
    [Header("⚙️ Task Setup")]
    [SerializeField, TextArea] private string[] tutorialTasks;   // Task instructions for UI
    [SerializeField] private TaskState currentTask = TaskState.ArmTheDrone;

    [Header("📋 UI References")]
    [SerializeField] private TextMeshProUGUI taskText;   // Assign TMP text in Inspector
    [SerializeField] private GameObject tutorialUI;      // UI container

    [Header("🚁 Drone References")]
    [SerializeField] private DRONECONT droneController;  // Will be auto-assigned at runtime
    [SerializeField] private FSJoystickInput joystickInput;

    [Header("🎯 Checkpoint References")]
    [SerializeField] private TrackCheckpoint trackCheckpoint;
    [SerializeField] private TrackCheckpoint squareCheckpoint;

    [Header("🛬 Landing Pad Reference")]
    [SerializeField] private LandingPad landingPad;

    private int currentTaskIndex = 0;
    private bool taskInProgress = false;
    private Coroutine activeTaskCoroutine;

    private bool hasEnteredGimbalMode = false;
    private bool checkpointObjectiveCompleted = false;
    private bool squareTaskCompleted = false;

    public static EventHandler OnPlayerWrongCheckpoint;
    public static Action OnTrackComplete;

    private void Start()
    {
        StartCoroutine(AssignDroneReferences());

        //currentTask = TaskState.ArmTheDrone;

        if (trackCheckpoint != null) trackCheckpoint.gameObject.SetActive(false);
        if (squareCheckpoint != null) squareCheckpoint.gameObject.SetActive(false);
    }

    private IEnumerator AssignDroneReferences()
    {
        yield return null;

        GameObject droneObj = GameObject.FindGameObjectWithTag("Player");
        if (droneObj != null)
        {
            droneController = droneObj.GetComponent<DRONECONT>();
            joystickInput = droneObj.GetComponent<FSJoystickInput>();
        }
        else
        {
            Debug.LogError("❌ No Drone with tag 'Player' found in the scene!");
        }

        if (tutorialTasks.Length > 0 && taskText != null)
        {
            taskText.text = tutorialTasks[0];
        }

        BeginTask(currentTask);
    }

    private void BeginTask(TaskState task)
    {
        if (activeTaskCoroutine != null)
            StopCoroutine(activeTaskCoroutine);

        switch (task)
        {
            case TaskState.ArmTheDrone:
                activeTaskCoroutine = StartCoroutine(StartCheckArmTheDrone(StartCompleteTask));
                break;

            case TaskState.Ascend:
                activeTaskCoroutine = StartCoroutine(StartCheckAscend(StartCompleteTask));
                break;

            case TaskState.Descend:
                activeTaskCoroutine = StartCoroutine(StartCheckDescend(StartCompleteTask));
                break;

            case TaskState.Pitch:
                activeTaskCoroutine = StartCoroutine(StartCheckPitch(StartCompleteTask));
                break;

            case TaskState.Roll:
                activeTaskCoroutine = StartCoroutine(StartCheckRoll(StartCompleteTask));
                break;

            case TaskState.Yaw:
                activeTaskCoroutine = StartCoroutine(StartCheckYaw(StartCompleteTask));
                break;

            case TaskState.EnterGimbalMode:
                activeTaskCoroutine = StartCoroutine(StartCheckGimbalMode(StartCompleteTask));
                break;

            case TaskState.TrackObjective:
                activeTaskCoroutine = StartCoroutine(StartCheckTrackObjective(StartCompleteTask));
                break;

            case TaskState.SquareObjective:
                activeTaskCoroutine = StartCoroutine(StartCheckSquareObjective(StartCompleteTask));
                break;

            case TaskState.Land:
                activeTaskCoroutine = StartCoroutine(StartCheckLand(StartCompleteTask));
                break;

            case TaskState.Disarm:
                activeTaskCoroutine = StartCoroutine(StartCheckDisarm(StartCompleteTask));
                break;
        }
    }

    private void StartCompleteTask()
    {
        if (activeTaskCoroutine != null)
            StopCoroutine(activeTaskCoroutine);

        activeTaskCoroutine = StartCoroutine(CompleteTask());
    }

    private IEnumerator CompleteTask()
    {
        Debug.Log($"✅ Task {currentTask} Complete");

        taskInProgress = true;
        yield return new WaitForSeconds(2.0f); // delay for feedback

        currentTaskIndex++;

        if (currentTaskIndex < tutorialTasks.Length)
        {
            taskText.text = tutorialTasks[currentTaskIndex];
            currentTask = (TaskState)currentTaskIndex;

            // Start the next task check
            BeginTask(currentTask);
        }
        else
        {
            Debug.Log("🎉 Tutorial Completed!");
            if (tutorialUI != null) Destroy(tutorialUI);
        }

        taskInProgress = false;
        activeTaskCoroutine = null;
    }

    // ─────────── Checkpoint Hooks ───────────

    private void OnEnable()
    {
        OnTrackComplete += CheckpointReached;
    }
    private void OnDisable()
    {
        OnTrackComplete -= CheckpointReached;
    }
    public void CheckpointReached()
    {
        Debug.Log("CheckpointReached Called currentTask => " + currentTask + " || squareTaskCompleted => " + squareTaskCompleted + " || checkpointObjectiveCompleted => " + checkpointObjectiveCompleted);
        if (currentTask == TaskState.TrackObjective && !checkpointObjectiveCompleted)
        {
            checkpointObjectiveCompleted = true;
            Debug.Log("Checkpoint Objective Completed!");
        }
        if (currentTask == TaskState.SquareObjective && !squareTaskCompleted)
        {
            squareTaskCompleted = true;
            Debug.Log("Square Task Completed!");
        }
    }

    // ─────────── Task Condition Coroutines ───────────

    private IEnumerator StartCheckArmTheDrone(Action onComplete)
    {
        yield return new WaitUntil(() => droneController != null && droneController.startupDone);
        onComplete?.Invoke();
    }

    private IEnumerator StartCheckAscend(Action onComplete)
    {
        yield return new WaitUntil(() => droneController != null && droneController.finalVertical > 0.1f);
        onComplete?.Invoke();
    }

    private IEnumerator StartCheckDescend(Action onComplete)
    {
        yield return new WaitUntil(() => droneController != null && droneController.finalVertical < -0.1f);
        onComplete?.Invoke();
    }

    private IEnumerator StartCheckPitch(Action onComplete)
    {
        yield return new WaitUntil(() => droneController != null && Mathf.Abs(droneController.finalHorizontalZ) > 0.1f);
        onComplete?.Invoke();
    }

    private IEnumerator StartCheckRoll(Action onComplete)
    {
        yield return new WaitUntil(() => droneController != null && Mathf.Abs(droneController.finalHorizontalX) > 0.1f);
        onComplete?.Invoke();
    }

    private IEnumerator StartCheckYaw(Action onComplete)
    {
        yield return new WaitUntil(() => droneController != null && Mathf.Abs(droneController.finalYaw) > 0.1f);
        onComplete?.Invoke();
    }

    private IEnumerator StartCheckGimbalMode(Action onComplete)
    {
        yield return new WaitUntil(() => hasEnteredGimbalMode);
        onComplete?.Invoke();
    }

    private IEnumerator StartCheckTrackObjective(Action onComplete)
    {
        trackCheckpoint.gameObject.SetActive(true);
        yield return new WaitUntil(() => checkpointObjectiveCompleted);
        trackCheckpoint.gameObject.SetActive(false);
        onComplete?.Invoke();
    }
    private IEnumerator StartCheckSquareObjective(Action onComplete)
    {
        squareCheckpoint.gameObject.SetActive(true);
        yield return new WaitUntil(() => squareTaskCompleted);
        squareCheckpoint.gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    private IEnumerator StartCheckLand(Action onComplete)
    {
        yield return new WaitUntil(() => droneController != null && droneController.inGround && landingPad.isLanding);
        onComplete?.Invoke();
    }

    private IEnumerator StartCheckDisarm(Action onComplete)
    {
        yield return new WaitUntil(() => droneController != null && !droneController.startupDone);
        onComplete?.Invoke();
    }


    // ─────────── Input Hooks ───────────
    private void Update()
    {
        // Handle Gimbal Mode input manually
        if (currentTask == TaskState.EnterGimbalMode && !hasEnteredGimbalMode)
        {
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                hasEnteredGimbalMode = true;
                Debug.Log("🎥 Gimbal Mode Entered!");
            }
        }
    }
}
