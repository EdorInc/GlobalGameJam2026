using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public InputActionReference anyButton;

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

    //Para desplegar el menú y hacerlo funcional -----------------------------------------------

    private bool menu_displayed = false;
    private bool menu_moving = false;

    [SerializeField] private float menu_speed = 30; //en porcentaje por segundo, quiero que vaya muy rápido
    private const int INITIAL_MENU_OFFSET = -70; //en porcentaje

    [SerializeField] private float menuDelay1 = 0.1f; //delay entre las piezas del menu para que no salgan todas en bloque sino que salgan en escalera
    [SerializeField] private float menuDelay2 = 0.2f; //

    private float actualMenuOffset1 = INITIAL_MENU_OFFSET;
    private float actualMenuOffset2 = INITIAL_MENU_OFFSET;
    private float actualMenuOffset3 = INITIAL_MENU_OFFSET;


    //Los botones
    private Button newGameButton;
    private Button selectButton;
    private Button exitButton;

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

    //-----------------------------------------------------------
    // Función para activar los botones
    //-----------------------------------------------------------
    private void ActivateButtons()
    {
        if (rootElement == null)
            return;

        newGameButton = rootElement.Q<Button>("NewGame");

        if (newGameButton == null)
        {
            Debug.LogError("No new game button");
            return;
        }

        exitButton = rootElement.Q<Button>("Exit");

        if (exitButton == null)
        {
            Debug.LogError("No exit button");
            return;
        }

        selectButton = rootElement.Q<Button>("LvlSel");

        if (selectButton == null)
        {
            Debug.LogError("No select button");
            return;
        }

        newGameButton.clicked += () => SceneManager.LoadScene("Lvl_1");
        selectButton.clicked += () => SceneManager.LoadScene("LevelSelectionScene");
        exitButton.clicked += () => GameManager.Instance.QuitGame();
    }

    private void Start()
    {
        rootElement = GetComponent<UIDocument>().rootVisualElement;

        //Poner el fondo aleatorio entre los 3
        var background = rootElement.Q<VisualElement>("background-image");

        var s = GetRandomScreen();

        ActivateButtons();

        if (s)
        {
            background.style.backgroundImage = new StyleBackground(GetRandomScreen());
        }

        //Iniciar el movimiento del titulo
        title_movement = true;

        //Suscribirme al evento del botón
        anyButton.action.Enable();
        anyButton.action.performed += OnAnyButtonPressed;

        //Activar el onclick de los botones
    }

    private void OnDisable()
    {
        anyButton.action.performed -= OnAnyButtonPressed;
        anyButton.action.Disable();
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

    //-------------------------------------------------------------------
    //Que empiece a moverse el menú
    //-------------------------------------------------------------------
    private void ActivateMenuMovement()
    {
        var text = rootElement.Q<Label>("pressbuton");
        text.style.visibility = Visibility.Hidden;

        press_visibility = false;
        menu_moving = true;
    }

    //---------------------------------------------------------------
    // Mover el menú en cada tick si le toca moverse
    //---------------------------------------------------------------
    private void MoveMenu()
    {
        if (menu_moving == true)
        {
            var b1 = rootElement.Q<Button>("NewGame");
            var b2 = rootElement.Q<Button>("LvlSel");
            var b3 = rootElement.Q<Button>("Exit");

            if (actualMenuOffset1 < 0)
            {
                actualMenuOffset1 = Mathf.Min(Time.deltaTime * menu_speed + actualMenuOffset1, 0);

                b1.style.right = Length.Percent(actualMenuOffset1);
            }

            if (actualMenuOffset2 < 0 && menuDelay1 <= 0) //Si no se ha terminado de mover y el delay no ha 
            {
                actualMenuOffset2 = Mathf.Min(Time.deltaTime * menu_speed + actualMenuOffset2, 0);

                
                b2.style.right = Length.Percent(actualMenuOffset2);
            }
            else if(menuDelay1 > 0)
            {
                menuDelay1 -= Time.deltaTime;
            }

            if (actualMenuOffset3 < 0 && menuDelay2 <= 0) //Si no se ha terminado de mover y el delay no ha 
            {
                actualMenuOffset3 = Mathf.Min(Time.deltaTime * menu_speed + actualMenuOffset3, 0);

                
                b3.style.right = Length.Percent(actualMenuOffset3);
            }
            else if (menuDelay2 > 0)
            {
                menuDelay2 -= Time.deltaTime;
            }

            if (actualMenuOffset1 >= 0 && actualMenuOffset2 >= 0 && actualMenuOffset3 >= 0)
            {
                menu_moving = false;
                menu_displayed = true;

                //Desactivar el anybutton para que no se salte más
                anyButton.action.performed -= OnAnyButtonPressed;
                anyButton.action.Disable();

                b1.Focus();

            }

        }
    }

    //---------------------------------------------------------------
    // Saltarse el movimiento del menú
    //---------------------------------------------------------------
    private void SkipMenuMovement()
    {
        if (rootElement != null)
        {
            var b1 = rootElement.Q<Button>("NewGame");
            var b2 = rootElement.Q<Button>("LvlSel");
            var b3 = rootElement.Q<Button>("Exit");
            actualMenuOffset1 = 0;
            actualMenuOffset2 = 0;
            actualMenuOffset3 = 0;

            b1.style.right = Length.Percent(actualMenuOffset1);
            b2.style.right = Length.Percent(actualMenuOffset2);
            b3.style.right = Length.Percent(actualMenuOffset3);

            menu_moving = false;
            menu_displayed = true;

            //Desactivar el anybutton para que no se salte más
            anyButton.action.performed -= OnAnyButtonPressed;
            anyButton.action.Disable();

            b1.Focus();

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

        MoveMenu();
    }

    private void OnAnyButtonPressed(InputAction.CallbackContext context)
    {
        Debug.Log("Boton");

        if (title_movement) //si está haciendo lo de moverse el título que se skipee lo de moverse el título
        {
            SkipTitleMovement();
        }
        else if (press_visibility)
        {
            ActivateMenuMovement();
        }
        else if (menu_moving)
        {
            SkipMenuMovement();
        }
    }

}
