using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class CameraOcclusion : MonoBehaviour
{
    private enum PlayerNumber { PlayerOne = 0, PlayerTwo = 1 }

    [SerializeField] private PlayerNumber playerNumber;
    [SerializeField] private LayerMask occlusionMask = ~0;
    [SerializeField] private float maxDistanceOffset = 0.05f;

    [Header("Detection")]
    [Tooltip("Si true usa SphereCastAll en vez de RaycastAll (mejora detección de objetos finos).")]
    public bool useSphereCast = false;
    public float sphereRadius = 0.15f;

    [Header("Appearance")]
    [Range(0f, 1f)] public float transparentAlpha = 0.3f;
    [Tooltip("Propiedad del color en tu shader ('_BaseColor' para URP, '_Color' para Built-in).")]
    public string colorPropertyName = "_BaseColor";

    private Camera _camera;
    private Transform target;

    // estado por cámara
    private HashSet<Renderer> currentlyTransparent = new HashSet<Renderer>();
    private Dictionary<Renderer, MaterialPropertyBlock> originalBlocks = new Dictionary<Renderer, MaterialPropertyBlock>();

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Start()
    {
        switch (playerNumber)
        {
            case PlayerNumber.PlayerOne:
                target = ReferenceManager.Instance.GetPlayerOne().transform;
                break;
            case PlayerNumber.PlayerTwo:
                target = ReferenceManager.Instance.GetPlayerTwo().transform;
                break;
        }
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        RestoreAll(); // por si acaso
        currentlyTransparent.Clear();
    }

    private void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam) => BeginCamera(cam);
    private void OnEndCameraRendering(ScriptableRenderContext ctx, Camera cam) => EndCamera(cam);

    private void BeginCamera(Camera cam)
    {
        if (cam != _camera) return;
        if (target == null) return;

        Vector3 origin = cam.transform.position;
        Vector3 dir = target.position - origin;
        float distance = dir.magnitude;
        if (distance <= 0.001f) return;

        RaycastHit[] hits;
        float castDistance = Mathf.Max(0f, distance - maxDistanceOffset);

        if (useSphereCast)
            hits = Physics.SphereCastAll(origin, sphereRadius, dir.normalized, castDistance, occlusionMask, QueryTriggerInteraction.Ignore);
        else
            hits = Physics.RaycastAll(origin, dir.normalized, castDistance, occlusionMask, QueryTriggerInteraction.Ignore);

        if (hits == null || hits.Length == 0)
        {
            // nada bloqueando: restaurar todo
            RestoreAll();
            currentlyTransparent.Clear();
            return;
        }

        // ordenar por distancia (más cercano primero)
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // recogemos todos los renderers de todos los hits (excluyendo los del target)
        HashSet<Renderer> newSet = new HashSet<Renderer>();
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            // si el collider pertenece al target, lo ignoramos
            if (IsColliderPartOfTarget(hit.collider)) continue;

            var renderers = hit.collider.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r == null) continue;
                if (IsRendererPartOfTarget(r)) continue;
                newSet.Add(r);
            }
        }

        // Aplicar transparencia a los nuevos que no estén ya transparentes
        foreach (var r in newSet)
        {
            if (!currentlyTransparent.Contains(r))
                MakeRendererTransparent(r);
        }

        // Restaurar los que estaban pero ya no están en el nuevo set
        var toRestore = new List<Renderer>();
        foreach (var r in currentlyTransparent)
        {
            if (!newSet.Contains(r))
                toRestore.Add(r);
        }
        foreach (var r in toRestore)
            RestoreRenderer(r);

        currentlyTransparent = newSet;
    }

    private void EndCamera(Camera cam)
    {
        if (cam != _camera) return;
        // Restauramos TODO para no afectar a otras cámaras en el mismo frame
        RestoreAll();
        currentlyTransparent.Clear();
    }

    private bool IsColliderPartOfTarget(Collider col)
    {
        if (target == null) return false;
        return col.transform.IsChildOf(target) || col.transform == target;
    }

    private bool IsRendererPartOfTarget(Renderer rend)
    {
        if (target == null || rend == null) return false;
        return rend.transform.IsChildOf(target) || rend.transform == target;
    }

    private void MakeRendererTransparent(Renderer rend)
    {
        if (rend == null) return;

        if (!originalBlocks.ContainsKey(rend))
        {
            var orig = new MaterialPropertyBlock();
            rend.GetPropertyBlock(orig);
            originalBlocks[rend] = orig;
        }

        var block = new MaterialPropertyBlock();
        rend.GetPropertyBlock(block);

        // obtener color base: property block -> sharedMaterial -> white
        Color baseColor = Color.white;
        if (TryGetColorFromBlock(block, out Color cb))
            baseColor = cb;
        else if (rend.sharedMaterial != null && rend.sharedMaterial.HasProperty(colorPropertyName))
            baseColor = rend.sharedMaterial.GetColor(colorPropertyName);

        baseColor.a = transparentAlpha;
        block.SetColor(colorPropertyName, baseColor);
        rend.SetPropertyBlock(block);
    }

    private void RestoreRenderer(Renderer rend)
    {
        if (rend == null) return;

        if (originalBlocks.TryGetValue(rend, out var origBlock))
        {
            rend.SetPropertyBlock(origBlock);
            originalBlocks.Remove(rend);
        }
        else
        {
            rend.SetPropertyBlock(null);
        }
    }

    private void RestoreAll()
    {
        var keys = new List<Renderer>(originalBlocks.Keys);
        foreach (var r in keys)
        {
            if (r != null)
                r.SetPropertyBlock(originalBlocks[r]);
        }
        originalBlocks.Clear();
    }

    // lectura segura desde MaterialPropertyBlock
    private bool TryGetColorFromBlock(MaterialPropertyBlock block, out Color color)
    {
        color = Color.white;
        if (block == null) return false;
        try
        {
            Vector4 v = block.GetVector(colorPropertyName);
            color = new Color(v.x, v.y, v.z, v.w);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
