using UnityEngine;
using System;
using UnityEngine.Video;
using UnityEngine.Splines;

/// <summary>
/// Centralized static event manager for game-wide events.
/// </summary>
public static class EventManager
{
    // -------------------- Button Events --------------------

    /// <summary>
    /// Invoked when a button is pressed. Parameter: channel ID.
    /// </summary>
    public static Action<int> OnButtonPressed;

    /// <summary>
    /// Invoked when a button is released. Parameter: channel ID.
    /// </summary>
    public static Action<int> OnButtonUnPressed;

    /// <summary>
    ///  Invoked when a button state is locked. Parameter: channel ID.
    /// </summary>
    public static Action<int> OnButtonLock;

    // -------------------- Player Events --------------------

    /// <summary>
    /// Invoked when an action cannot be performed. Parameter: target GameObject.
    /// </summary>
    public static Action<GameObject> OnCantPerforAction;

    /// <summary>
    /// Invoked when a fall starts. Parameter: affected GameObject.
    /// </summary>
    public static Action<GameObject> OnFallStarted;

    /// <summary>
    /// Invoked when a fall ends. Parameter: affected GameObject.
    /// </summary>
    public static Action<GameObject> OnFallEnded;

    /// <summary>
    /// Invoked when a GameObject tries to move. Parameter: moving GameObject.
    /// </summary>
    public static Action<GameObject> OnTryingToMove;

    /// <summary>
    /// Invoked when a GameObject tries to be freed. Parameter: target GameObject.
    /// </summary>
    public static Action<GameObject> OnTryingToBeFree;

    /// <summary>
    /// Invoked when a GameObject is thrown.
    /// Parameters: thrown GameObject, isSuccessful, thrower GameObject.
    /// </summary>
    public static Action<GameObject, bool, GameObject> OnThrow;

    // -------------------- Environmental Events --------------------

    /// <summary>
    /// Invoked when a collider enters an air current.
    /// Parameters: collider, entry position.
    /// </summary>
    public static Action<Collider, Vector3,bool> OnAirCurrentEnter;

    /// <summary>
    /// Invoked when a collider exits an air current.
    /// Parameter: collider.
    /// </summary>
    public static Action<Collider,bool> OnAirCurrentExit;


    /// <summary>
    /// Invoked when a collider enters a pipe entry.
    /// Parameter: collider, spline.
    /// </summary>
    public static Action<Collider, TubeSpawner, bool> OnPipeEntryPoint;


    /// <summary>
    /// Invoked when a collider enters a pipe entry.
    /// Parameter: collider, spline.
    /// </summary>
    public static Action<Collision, Transform> OnWaterWall;

    /// <summary>
    /// Invoked when a collider is lit on fire.
    /// Parameter: collider.
    /// </summary>
    public static Action<Collider> OnLitOnFire;

    /// <summary>
    /// Invoked when a victory condition is met.
    /// </summary>
    public static Action OnVictory;

    /// <summary>
    /// Invoked when entering a tutorialTrigger
    /// </summary>
    public static Action<string, Sprite, VideoClip> OnTutorialTriggerEnter;

    /// <summary>
    /// Invoked when exiting a tutorialTrigger
    /// </summary>
    public static Action OnTutorialTriggerExit;


    // -------------------- Status Events --------------------

    /// <summary>
    /// Invoked when a GameObject receives damage.
    /// Parameters: damaged GameObject, damage source position.
    /// </summary>
    public static Action<GameObject, Vector3> OnDamageRecived;
    /// <summary>
    /// Invoked when a GameObject respawns
    /// Parameters: respawned GameObject.
    /// </summary>
    public static Action<GameObject> OnRespawn;

    // -------------------- Navigation Events --------------------

    /// <summary>
    /// Invoked when the NavMesh is updated.
    /// Parameter: NavMeshManager instance.
    /// </summary>
    public static Action<NavMeshManager> OnNavMeshUpdate;
}
