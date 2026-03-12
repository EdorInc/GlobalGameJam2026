using UnityEngine;

public class TextureScaler : MonoBehaviour
{
    [ContextMenu("Scale")]
    void Scale()
    {
        Renderer r = GetComponentInChildren<Renderer>();
        Vector3 scale = transform.localScale;
        Material mat = new Material(r.sharedMaterial);

        float maxScale = Mathf.Max(scale.z, scale.x);
        float minScale = Mathf.Min(scale.z, scale.x);

        r.sharedMaterial = mat;
        mat.mainTextureScale = new Vector2(maxScale, minScale);
    }
}
