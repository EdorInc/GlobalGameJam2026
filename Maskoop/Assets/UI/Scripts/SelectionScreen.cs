using UnityEngine;
using UnityEngine.UIElements;

struct TLevelData
{
    string label;
    Texture2D image;

};

public class SelectionScreen : MonoBehaviour
{

    [SerializeField] private TLevelData[] levels;

    private VisualElement root = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        var levelScroll = root.Q<ScrollView>("level-scroll");

        foreach ( TLevelData level in levels)
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
