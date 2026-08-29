using UnityEngine;
using UnityEngine.UIElements;

public class TitleScreen : MonoBehaviour
{
    //Para elegir las imagenes de fondo
    [SerializeField] private Texture2D[] images;

    private Texture2D GetRandomScreen() {

        Texture2D t = null;

        if (images.Length > 0)
        {
            int rIndex = Random.Range(0, images.Length);
            t = images[rIndex];
            
        }

        return t;
    }

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        //Poner el fondo aleatorio entre los 3
        var background = root.Q<VisualElement>("background-image");

        var s = GetRandomScreen();

        if(s)
            background.style.backgroundImage = new StyleBackground(GetRandomScreen());
    }

}
