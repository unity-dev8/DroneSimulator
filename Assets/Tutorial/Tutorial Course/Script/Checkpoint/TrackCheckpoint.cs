using System;
using System.Collections.Generic;
using UnityEngine;

public class TrackCheckpoint : MonoBehaviour
{
    [SerializeField] private List<Checkpoint> checkpointList = new List<Checkpoint>();
    [SerializeField] private int nextCheckpointSingleIndex;

    public Action OnPlayerCorrectCheckpoint;


    private void Awake()
    {
        foreach (Transform checkpointTracnsform in transform.GetChild(0).transform)
        {
            Checkpoint checkpoint = checkpointTracnsform.GetComponent<Checkpoint>();

            checkpoint.SetTrackCheckpoints(this);

            checkpointList.Add(checkpoint);
        }

        nextCheckpointSingleIndex = 0;
    }

    private void Start()
    {
        ShowCurrentCheckpoint();
    }

    public void PlayerThroughCheckpoint(Checkpoint checkpoint)
    {
        if (checkpointList.IndexOf(checkpoint) == nextCheckpointSingleIndex)
        {
            Debug.Log("Correct");
            nextCheckpointSingleIndex = (nextCheckpointSingleIndex + 1) % checkpointList.Count;

            if (nextCheckpointSingleIndex == 0)
            {
                Debug.Log("Track completed!");
                TutorialManager.OnTrackComplete?.Invoke();
                HideCurrentCheckpoint(checkpoint);
            }
            else
            {
                OnPlayerCorrectCheckpoint?.Invoke();

                ShowNextCheckpoint();
                HideCurrentCheckpoint(checkpoint);
            }
        }
        else
        {
            Debug.Log("Wrong");
            TutorialManager.OnPlayerWrongCheckpoint?.Invoke(this, EventArgs.Empty);
        }
    }

    private void HideCurrentCheckpoint(Checkpoint checkpoint)
    {
        Checkpoint hideCurrentCheckpoint = checkpointList[checkpointList.IndexOf(checkpoint)];
        hideCurrentCheckpoint.HideCheckpoint();
    }
    private void ShowNextCheckpoint()
    {
        Checkpoint showNextCheckpoint = checkpointList[nextCheckpointSingleIndex];
        showNextCheckpoint.ShowCheckpoint();
    }
    private void ShowCurrentCheckpoint()
    {
        Checkpoint showCurentCheckpoint = checkpointList[nextCheckpointSingleIndex];
        showCurentCheckpoint.ShowCheckpoint();
    }
}
