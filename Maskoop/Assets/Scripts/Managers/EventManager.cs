using UnityEngine;
using System;
public static class EventManager
{
    public static Action<int> OnButtonPressed;
    public static Action<bool> OnCantPerforAction;
    public static Action OnVictory;
}
