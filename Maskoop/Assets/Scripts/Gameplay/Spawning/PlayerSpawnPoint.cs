using UnityEngine;

public enum PlayerSlot
{
    Player1 = 0,
    Player2 = 1
}

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private PlayerSlot playerSlot;

    public PlayerSlot PlayerSlot => playerSlot;
}