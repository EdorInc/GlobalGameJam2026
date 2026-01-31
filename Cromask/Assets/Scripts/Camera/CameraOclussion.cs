using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]

public class CameraOcclusion: MonoBehaviour
{
    private enum PlayerNumber
    {
        PlayerOne = 0, PlayerTwo = 1,
    };


    [SerializeField]
    private PlayerNumber playerNumber;
   
    public LayerMask occlusionMask = ~0;           // Capas que pueden bloquear la vista
    public float maxDistanceOffset = 0.05f;        // pequeño margen para no chocar con el propio target

    [Header("Appearance")]
    [Range(0f, 1f)] public float transparentAlpha = 0.3f;
    
    private string colorPropertyName = "_BaseColor";
    // Estado runtime
    private Camera _camera;
    private HashSet<Renderer> currentlyTransparent = new HashSet<Renderer>();
    private Dictionary<Renderer, MaterialPropertyBlock> originalBlocks = new Dictionary<Renderer, MaterialPropertyBlock>();

    private Transform target;                   

    private void Start()
    {
        switch(playerNumber)
        {
            case PlayerNumber.PlayerOne:
                target = ReferenceManager.Instance.GetPlayerOne().transform;
                break;
            case PlayerNumber.PlayerTwo:
                target = ReferenceManager.Instance.GetPlayerTwo().transform;
                break;
        }
    }
    private void Awake()
    {
        _camera = GetComponent<Camera>();
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

        // Aseguramos restaurar por si se desactiva mientras hay cosas modificadas
        RestoreAll();
    }

    // ----- SRP callbacks -----
    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        BeginCamera(cam);
    }

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        EndCamera(cam);
    }

    // ----- Core -----
    private void BeginCamera(Camera cam)
    {
        if (cam != _camera) return;            // solo para la cámara a la que está attached
        if (target == null) return;

        Vector3 origin = cam.transform.position;
        Vector3 dir = target.position - origin;
        float distance = dir.magnitude;
        if (distance <= 0.001f) return;

        // Raycast simple (primer hit)
        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, Mathf.Max(0f, distance - maxDistanceOffset), occlusionMask, QueryTriggerInteraction.Ignore))
        {
            var hitRenderers = hit.collider.GetComponentsInChildren<Renderer>();
            HashSet<Renderer> newSet = new HashSet<Renderer>(hitRenderers);

            // Poner transparentes los nuevos
            foreach (var r in newSet)
            {
                if (r == null) continue;
                if (!currentlyTransparent.Contains(r))
                    MakeRendererTransparent(r);
            }

            // Restaurar los que ya no están en el nuevo set
            var toRestore = new List<Renderer>();
            foreach (var r in currentlyTransparent)
                if (!newSet.Contains(r))
                    toRestore.Add(r);

            foreach (var r in toRestore)
                RestoreRenderer(r);

            currentlyTransparent = newSet;
        }
        else
        {
            // No hit: restaurar todo lo que estuviera transparente para esta cámara
            RestoreAll();
            currentlyTransparent.Clear();
        }
    }

    private void EndCamera(Camera cam)
    {
        if (cam != _camera) return;
        // Restauramos TODO para que no afecte a siguientes cámaras
        RestoreAll();
        currentlyTransparent.Clear();
    }

    // ----- Helper: aplicar transparencia usando MaterialPropertyBlock -----
    private void MakeRendererTransparent(Renderer rend)
    {
        if (rend == null) return;

        // Guardar block original si no lo tenemos
        if (!originalBlocks.ContainsKey(rend))
        {
            var orig = new MaterialPropertyBlock();
            rend.GetPropertyBlock(orig);
            originalBlocks[rend] = orig;
        }

        // Obtener block actual (podría ser vacío) y modificar color.alpha
        var block = new MaterialPropertyBlock();
        rend.GetPropertyBlock(block);

        // Leer color base (intento: property block > shared material > white)
        Color baseColor = Color.white;
        bool gotFromBlock = TryGetColorFromBlock(block, out Color colorFromBlock);
        if (gotFromBlock) baseColor = colorFromBlock;
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
            // Restaurar el property block original (puede estar vacío)
            rend.SetPropertyBlock(origBlock);
            originalBlocks.Remove(rend);
        }
        else
        {
            // No teníamos block original: limpiar
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

    // Intento seguro de leer color desde MaterialPropertyBlock
    private bool TryGetColorFromBlock(MaterialPropertyBlock block, out Color color)
    {
        color = Color.white;
        if (block == null) return false;
        // MaterialPropertyBlock no tiene TryGetColor público, usamos GetVector con control de errores
        try
        {
            Vector4 v = block.GetVector(colorPropertyName);
            // si v es (0,0,0,0) podría ser que no exista la propiedad, pero también puede ser color negro.
            // Aun así lo consideramos válido si cualquier componente != 0, o si alpha != 0.
            color = new Color(v.x, v.y, v.z, v.w);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
