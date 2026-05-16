using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Splines;

public class PipeEntryPoint : MonoBehaviour
{
    public TubeSpawner spline;

    private float timeFromLast = 0;

    public bool isEntry = true;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && timeFromLast > 1)
        {
            EventManager.OnPipeEntryPoint?.Invoke(other,spline, isEntry);
            timeFromLast = 0;
        }
    }

    private void Update()
    {
        timeFromLast += Time.deltaTime;
    }

}
