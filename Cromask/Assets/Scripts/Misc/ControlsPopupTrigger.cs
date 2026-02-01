using UnityEngine;

public class ControlsPopupTrigger : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private Canvas popupCanvas;
    [SerializeField] private Transform popupAnchor;

    [Header("Options")]
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private Vector3 popupOffset = new Vector3(0.5f, 1.5f, 0f);


    [SerializeField]private Transform mainCamera;

    private void Awake()
    {
        if (popupCanvas != null)
            popupCanvas.enabled = false;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PositionPopup(other.transform);
        ShowPopup();
    }


    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        HidePopup();
    }

    private void ShowPopup()
    {
        popupCanvas.enabled = true;
    }

    private void HidePopup()
    {
        popupCanvas.enabled = false;
    }

    private void LateUpdate()
    {
        if (!faceCamera || !popupCanvas.enabled)
            return;

        popupCanvas.transform.LookAt(mainCamera);
        popupCanvas.transform.Rotate(0f, 180f, 0f); // face camera correctly
    }

    private void PositionPopup(Transform target)
    {
        popupCanvas.transform.position =
            target.position +
            target.right * popupOffset.x +
            Vector3.up * popupOffset.y +
            target.forward * popupOffset.z;
    }

}
