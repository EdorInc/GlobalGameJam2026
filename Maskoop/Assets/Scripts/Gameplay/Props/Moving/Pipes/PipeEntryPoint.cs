using UnityEngine;
using UnityEngine.Splines;

public class PipeEntryPoint : MonoBehaviour
{
    public SplineContainer spline;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.OnPipeEntryPoint?.Invoke(other,spline);
        }
    }

}
