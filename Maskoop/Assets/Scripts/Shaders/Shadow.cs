using UnityEngine;

public class Shadow : MonoBehaviour
{
    public void SetImpact(Vector3 position, float radius)
    {
        Shader.SetGlobalVector("_ImpactPosition", position);
        Shader.SetGlobalFloat("_ImpactRadius", radius);
    }

    public void ClearImpact()
    {
        Shader.SetGlobalFloat("_ImpactRadius", 0f);
    }
}
