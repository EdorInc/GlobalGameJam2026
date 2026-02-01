using System.Collections.Generic;
using UnityEngine;

public enum ObjectColor
{
    Red,
    Blue,
    Green,
}

public class ColoredObject : MonoBehaviour
{
    [SerializeField]
    private ObjectColor objectColor;

    [SerializeField]
    private LayerMask collisionMask;


    private HashSet<Camera> camerasRendering = new HashSet<Camera>();

    private void Update()
    {
        //Esto se debería cambiar cuando cambie de mascara el player, no desde aqui.
        // ¡OJO! Al cambiar de color, tenemos que negar el anterior.
        var playerOneColor = ReferenceManager.Instance.GetPlayerOne().GetComponent<PlayerColorComponent>().GetPlayerColor();
        var playerTwoColor = ReferenceManager.Instance.GetPlayerTwo().GetComponent<PlayerColorComponent>().GetPlayerColor();

        if (playerOneColor == ObjectColor.Blue || playerTwoColor == ObjectColor.Blue)
            collisionMask |= 1 << LayerMask.NameToLayer("BlueMask");

        if (playerOneColor == ObjectColor.Red || playerTwoColor == ObjectColor.Red)
            collisionMask |= 1 << LayerMask.NameToLayer("RedMask");

        if (playerOneColor == ObjectColor.Green || playerTwoColor == ObjectColor.Green)
            collisionMask |= 1 << LayerMask.NameToLayer("GreenMask");
    }
   

    private void OnCollisionEnter(Collision collision)
    {

        MonoBehaviour a = collision.gameObject.GetComponent<MonoBehaviour>();
        if (!a)
        {
            Debug.Log("It's not a player");
            return;
        }
        else
        {

            switch (objectColor)
            {
                case ObjectColor.Red:
                    Debug.Log("Collided with a Red object!");
                    break;
                case ObjectColor.Blue:
                    Debug.Log("Collided with a Blue object!");
                    break;
                case ObjectColor.Green:
                    Debug.Log("Collided with a Green object!");
                    break;
                default: break;
            }
        }
    }

    //private void SelectLayerMaskColor(ObjectColor color)
    //{
    //    switch (color)
    //    {
    //        case ObjectColor.Red:
    //            collisionMask = 1 << LayerMask.NameToLayer("RedMask");
    //            break;
    //        case ObjectColor.Blue:
    //            collisionMask = 1 << LayerMask.NameToLayer("BlueMask");
    //            break;
    //        case ObjectColor.Green:
    //            collisionMask = 1 << LayerMask.NameToLayer("GreenMask");
    //            break;
    //        default:
    //            break;      
    //    }
    //}
}
