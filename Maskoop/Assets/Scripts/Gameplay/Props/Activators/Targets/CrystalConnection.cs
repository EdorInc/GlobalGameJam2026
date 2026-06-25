using UnityEngine;

public class CrystalConnection : MonoBehaviour
{
    [SerializeField] private BaseSwitch activable;
    [SerializeField] private float flowSpeed = 1f;

    private Transform targetDoor;
    private Material material;

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();

        material = Instantiate(line.material);
        line.material = material;
    }

    private void OnEnable()
    {
        EventManager.PairDoor += SetDoor;
        EventManager.OnButtonPressed += RemoveLink;
        EventManager.OnButtonUnPressed += ActivateLink;
    }

    private void OnDisable()
    {
        EventManager.PairDoor -= SetDoor;
        EventManager.OnButtonPressed -= RemoveLink;
        EventManager.OnButtonUnPressed -= ActivateLink;
    }
    public void SetDoor(Transform door,int ID)
    {
        if(ID == activable.channel)
        {
            targetDoor = door;
        } 
    }

    private void RemoveLink(int ID)
    {
        if(ID == activable.channel)
        {
            line.enabled = false;
        }
    }
    private void ActivateLink(int ID)
    {
        if (ID == activable.channel)
        {
            line.enabled = true;
        }
    }
    private void LateUpdate()
    {
        if (targetDoor == null)
            return;

        line.SetPosition(0, transform.position);
        line.SetPosition(1, targetDoor.position);

        Vector2 offset = material.mainTextureOffset;
        offset.x += flowSpeed * Time.deltaTime;
        material.mainTextureOffset = offset;
    }
}