using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
public class VibrationManager : MonoBehaviour
{
 
    private static VibrationManager _instance;
    public static VibrationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AudioManager");
                _instance = go.AddComponent<VibrationManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private List<Gamepad> connectedGamepads = new List<Gamepad>();
    private Dictionary<Gamepad, Coroutine> rumbleCoroutines = new Dictionary<Gamepad, Coroutine>();

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;

        foreach (var pad in Gamepad.all)
            connectedGamepads.Add(pad);
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;

        StopAllRumble();
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad pad)
        {
            if (change == InputDeviceChange.Added)
                connectedGamepads.Add(pad);
            else if (change == InputDeviceChange.Removed)
                connectedGamepads.Remove(pad);
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void RumblePulse(Gamepad pad, float low, float high, float duration)
    {
        if (pad == null)
            return;

        pad.SetMotorSpeeds(low, high);

        if (rumbleCoroutines.ContainsKey(pad))
            StopCoroutine(rumbleCoroutines[pad]);

        rumbleCoroutines[pad] = StartCoroutine(StopRumble(pad, duration));
    }

    private IEnumerator StopRumble(Gamepad pad, float duration)
    {
        yield return new WaitForSeconds(duration);
        pad.SetMotorSpeeds(0f, 0f);
        rumbleCoroutines.Remove(pad);
    }

    public void StopAllRumble()
    {
        foreach (var pad in connectedGamepads)
            pad.SetMotorSpeeds(0f, 0f);

        StopAllCoroutines();
        rumbleCoroutines.Clear();
    }
}
