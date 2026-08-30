using UnityEngine;
using UnityEngine.UIElements;

public class TitleScreen : MonoBehaviour
{
    //Pa mover el título al principio --------------------------------------------
    private const float INITIAL_TITLE_OFFSET = -50;
    private const float FINAL_TITLE_OFFSET = -5;

    [SerializeField] private float title_speed = 12; //porcentaje por segundo

    private float actualOffset = INITIAL_TITLE_OFFSET;

    private bool title_movement = false;

    //Para elegir las imagenes de fondo -------------------------------------------------------
    [SerializeField] private Texture2D[] images;

    private VisualElement rootElement = null;

    //Para parpadear el texto press any button ------------------------------------------------

    private bool press_visibility = false;

    private bool press_actualStatus = true;

    [SerializeField] private float press_active_time = 1.2f;

    [SerializeField] private float press_unactive_time = 0.12f;
    
    private float actualTime = 0;
    
    //-----------------------------------------------------------
    // Devuelve una de las n imagenes que puedan ser la imagen de fondo de la pantalla del título
    //-----------------------------------------------------------
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
        rootElement = GetComponent<UIDocument>().rootVisualElement;

        //Poner el fondo aleatorio entre los 3
        var background = rootElement.Q<VisualElement>("background-image");

        var s = GetRandomScreen();

        if (s)
        {
            background.style.backgroundImage = new StyleBackground(GetRandomScreen());
        }

        //Iniciar el movimiento del titulo
        title_movement = true;
    }

    //-----------------------------------------------------------------
    //Mueve el título poco a poco hacia abajo
    //-----------------------------------------------------------------
    private void MoveTitle()
    {
        if (title_movement == true && actualOffset < FINAL_TITLE_OFFSET)
        {
            if (rootElement != null)
            {
                actualOffset = Mathf.Min(Time.deltaTime * title_speed + actualOffset, FINAL_TITLE_OFFSET);

                var image = rootElement.Q<Image>("title");
                image.style.top = Length.Percent(actualOffset);
            }

            if (actualOffset == FINAL_TITLE_OFFSET)
            {
                title_movement = false;
                press_visibility = true;
            }

        }
    }

    //-------------------------------------------------------------------
    //Se salta el movimiento del titulo y lo coloca directamente
    //-------------------------------------------------------------------
    private void SkipTitleMovement()
    {

        if (rootElement != null)
        {
            var image = rootElement.Q<Image>("title");
            actualOffset = FINAL_TITLE_OFFSET;
            image.style.top = Length.Percent(actualOffset);
            title_movement = false;
            press_visibility = true;
        }

    }

    //-----------------------------------------------------------------
    // Hace parpadear el texto que dice "press any button"
    //-----------------------------------------------------------------
    private void PressButtonFlicker()
    {
        if (press_visibility == true)
        {
            
            actualTime += Time.deltaTime;

            if (press_actualStatus && actualTime > press_active_time) //Si está encendido y ya lleva más del tiempo que debería
            {
                press_actualStatus = false;
                actualTime = 0;
            }
            else if (!press_actualStatus && actualTime > press_unactive_time)
            {
                press_actualStatus = true;
                actualTime = 0;
            }

            var text = rootElement.Q<Label>("pressbuton");

            if (press_actualStatus && text.style.visibility != Visibility.Visible) //encenderlo si debería estar encendido y no lo está
                text.style.visibility = Visibility.Visible;
            if (!press_actualStatus && text.style.visibility != Visibility.Hidden) //apagarlo si debería estar apagado y no lo está
                text.style.visibility = Visibility.Hidden;
        }
    }

    void Update()
    {
        //Mover el título hacia abajo
        MoveTitle();

        //Hacer parpadear el otro textito
        PressButtonFlicker();
    }

}
