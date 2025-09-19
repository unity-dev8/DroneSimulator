using UnityEngine;

public class TrackCheckpointUI : MonoBehaviour
{
    [SerializeField] private TrackCheckpoint trackCheckpoint;
    private void Start()
    {
        trackCheckpoint.OnPlayerCorrectCheckpoint += TrackCheckpoint_OnPlayerCorrectCheckpoint;
        TutorialManager.OnTrackComplete += TrackCheckpoint_OnTrackComplete;

        Show();
    }

    private void TrackCheckpoint_OnPlayerCorrectCheckpoint()
    {
        Debug.Log("TrackCheckpoint_OnPlayerCorrectCheckpoint => " + this.gameObject.name);
        Show();
    }

    private void TrackCheckpoint_OnTrackComplete()
    {
        Debug.Log("TrackCheckpoint_OnTrackComplete => " + this.gameObject.name);
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
