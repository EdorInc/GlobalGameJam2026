using UnityEngine;
using System;
public static class EventManager
{
    public static Action<int> OnButtonPressed;
    public static Action<GameObject> OnCantPerforAction;
    public static Action OnVictory;
    public static Action<GameObject> OnFallStarted;
    public static Action<GameObject> OnFallEnded;
}
