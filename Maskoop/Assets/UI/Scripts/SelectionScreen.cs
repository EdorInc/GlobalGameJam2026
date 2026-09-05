using System;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public struct TLevelData
{
    public string label;
    public Texture2D image;
};

public class SelectionScreen : MonoBehaviour
{

    [SerializeField] TLevelData[] levels;

    private VisualElement root = null;
    private uint actualIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        var levelScroll = root.Q<ScrollView>("level-scroll");
        var leftArrow = root.Q<Button>("left-button");
        var rightArrow = root.Q<Button>("right-button");

        leftArrow.SetEnabled(false);
        leftArrow.visible = false;


        foreach ( var level in levels)
        {
            VisualElement container = new VisualElement();
            container.AddToClassList("level-container");

            Label label = new Label(level.label);
            label.AddToClassList("level-label");

            Image image = new Image();
            image.image = level.image;
            image.scaleMode = ScaleMode.ScaleAndCrop;
            image.AddToClassList("level-image");

            container.Add(label);
            container.Add(image);

            levelScroll.Add(container);
        }

        leftArrow.clicked += MoveLeft;
        rightArrow.clicked += MoveRight;

        levelScroll.scrollOffset += new Vector2(0,0);
    }

    private void MoveLeft()
    {
        if (actualIndex > 0)
        {
            actualIndex--;

            var levelScroll = root.Q<ScrollView>("level-scroll");
            var card = levelScroll.contentContainer.ElementAt(0);

            levelScroll.scrollOffset -= new Vector2(card.layout.width,0);

            if (actualIndex == 0)
            {
                var leftArrow = root.Q<Button>("left-button");
                leftArrow.SetEnabled(false);
                leftArrow.visible = false;
            }
            if (actualIndex != levels.Length - 1)
            {
                var rightArrow = root.Q<Button>("right-button");
                rightArrow.SetEnabled(true);
                rightArrow.visible = true;
            }

        }

    }

    private void MoveRight()
    {
        

        if (actualIndex < levels.Length - 1)
        {
            actualIndex++;

            var levelScroll = root.Q<ScrollView>("level-scroll");


            var card = levelScroll.contentContainer.ElementAt(0);

            levelScroll.scrollOffset += new Vector2(card.layout.width,0);

            Debug.Log("Me muevo");
            Debug.Log(actualIndex);
            Debug.Log(levelScroll.scrollOffset);

            if (actualIndex == levels.Length - 1)
            {
                var rightArrow = root.Q<Button>("right-button");
                rightArrow.SetEnabled(false);
                rightArrow.visible = false;
            }
            if (actualIndex > 0 )
            {
                var leftArrow = root.Q<Button>("left-button");
                leftArrow.SetEnabled(true);
                leftArrow.visible = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
