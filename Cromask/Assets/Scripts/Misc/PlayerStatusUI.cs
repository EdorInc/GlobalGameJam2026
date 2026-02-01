using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("Player 1 UI")]
    [SerializeField] private RawImage maskPlayer1;
    [SerializeField] private RawImage boxPlayer1;

    [Header("Player 2 UI")]
    [SerializeField] private RawImage maskPlayer2;
    [SerializeField] private RawImage boxPlayer2;

    [Header("Mask Textures")]
    [SerializeField] private Texture redMaskTexture;
    [SerializeField] private Texture blueMaskTexture;
    [SerializeField] private Texture greenMaskTexture;

    [Header("Box Indicator")]
    [SerializeField] private Texture boxGrabbedTexture;
    [SerializeField] private Texture boxEmptyTexture;

    private MaskManager maskManagerP1;
    private MaskManager maskManagerP2;
    private GrabAction grabActionP1;
    private GrabAction grabActionP2;

    void Start()
    {
        ReferenceManager refManager = ReferenceManager.Instance;

        if (refManager == null)
        {
            Debug.LogError("ReferenceManager no encontrado.");
            return;
        }

        GameObject playerOne = refManager.GetPlayerOne();
        GameObject playerTwo = refManager.GetPlayerTwo();

        if (playerOne != null)
        {
            maskManagerP1 = playerOne.GetComponent<MaskManager>();
            grabActionP1 = playerOne.GetComponent<GrabAction>();
            maskPlayer1.enabled = false;
        }

        if (playerTwo != null)
        {
            maskManagerP2 = playerTwo.GetComponent<MaskManager>();
            grabActionP2 = playerTwo.GetComponent<GrabAction>();
            maskPlayer2.enabled = false;
        }
    }

    void Update()
    {
        UpdatePlayerMaskUI(maskManagerP1, maskPlayer1);
        UpdatePlayerMaskUI(maskManagerP2, maskPlayer2);

        UpdatePlayerBoxUI(grabActionP1, boxPlayer1);
        UpdatePlayerBoxUI(grabActionP2, boxPlayer2);
    }

    private void UpdatePlayerMaskUI(MaskManager maskManager, RawImage maskImage)
    {
        if (maskManager == null || maskImage == null) return;

        Mask currentMask = maskManager.GetCurrentMask();

        if (currentMask == Mask.Unmasked)
        {
            maskImage.enabled = false;
            return;
        }

        maskImage.enabled = true;

        maskImage.texture = currentMask switch
        {
            Mask.Red => redMaskTexture,
            Mask.Blue => blueMaskTexture,
            Mask.Green => greenMaskTexture,
            _ => null
        };
    }


    private void UpdatePlayerBoxUI(GrabAction grabAction, RawImage boxImage)
    {
        if (grabAction == null || boxImage == null) return;

        GameObject grabbedObject = grabAction.GetGrabbedObject();

        bool hasBox = grabbedObject != null &&
                      grabbedObject.GetComponent<EquipableObject>() == null;

        boxImage.texture = hasBox ? boxGrabbedTexture : boxEmptyTexture;
        boxImage.enabled = hasBox || boxEmptyTexture != null;
    }
}
