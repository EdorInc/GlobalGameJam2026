using UnityEngine;
using UnityEngine.Video;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Content")]
    public string tutorialText;
    public Sprite buttonSprite;
    public VideoClip videoClip;

    [Header("Settings")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;

        if (other.CompareTag("Player"))
        {
            EventManager.OnTutorialTriggerEnter?.Invoke(tutorialText,buttonSprite,videoClip);

            hasTriggered = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.OnTutorialTriggerExit?.Invoke();
        }
    }
}