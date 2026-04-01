using UnityEngine;
using System;
public static class EventManager
{
    public static Action<int> OnButtonPressed;
    public static Action<int> OnButtonUnPressed;
    public static Action<GameObject> OnCantPerforAction;
    public static Action OnVictory;
    public static Action<GameObject> OnFallStarted;
    public static Action<GameObject> OnFallEnded;
    public static Action<Collider, Vector3> OnAirCurrentEnter;
    public static Action<Collider> OnAirCurrentExit;
    public static Action<Collider> OnLitOnFire;
    public static Action<GameObject,Vector3> OnDamageRecived;
    public static Action<NavMeshManager> OnNavMeshUpdate;
    public static Action<GameObject> TryingToBeFree;
    public static Action<GameObject,bool,GameObject> Throw;
    public static Action<GameObject> TryingToMove;
}
