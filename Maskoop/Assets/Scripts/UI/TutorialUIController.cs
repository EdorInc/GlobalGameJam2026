using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class TutorialUIController : MonoBehaviour
{

    private Label textLabel;
    private Image buttonImage;
    private Image videoImage;
    private VisualElement root;
    private VideoPlayer videoPlayer;

    [Header("Video")]
    public RenderTexture renderTexture;

    private void OnEnable()
    {
        EventManager.OnTutorialTriggerEnter += ShowTutorial;
        EventManager.OnTutorialTriggerExit += HideTutorial;
    }

    private void OnDisable()
    {
        EventManager.OnTutorialTriggerEnter -= ShowTutorial;
        EventManager.OnTutorialTriggerExit += HideTutorial;
    }

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        var rootElement = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("tutorial-ui");

        root = rootElement.Q<VisualElement>("tutorial-root");
        textLabel = rootElement.Q<Label>("tutorial-text");
        buttonImage = rootElement.Q<Image>("tutorial-button");
        videoImage = rootElement.Q<Image>("tutorial-video");

        videoImage.image = renderTexture;

        HideTutorial();
    }

    public void ShowTutorial(string text, Sprite button, VideoClip clip)
    {
        textLabel.text = text;

        buttonImage.image = button.texture;

        videoPlayer.clip = clip;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.isLooping = true;
        videoPlayer.Play();

        root.style.display = DisplayStyle.Flex;
    }

    public void HideTutorial()
    {
        root.style.display = DisplayStyle.None;

        videoPlayer.Stop();
    }
}