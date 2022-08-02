using UnityEngine;
using Mirror;

public class CustomNetworkRoomManager : NetworkRoomManager
{
    [Header("Game configuration")]
    [SerializeField] private int _hitsToWin = 3;
    [SerializeField] private float _playerHitTime = 3f;
    [SerializeField] private float _dashDistance = 4f;
    [SerializeField] private float _dashSpeed = 10f;

    static CustomNetworkRoomManager instance;

    public static int HitsToWin { get => instance._hitsToWin; }
    public static float PlayerHitTime { get => instance._playerHitTime; }
    public static float DashDistance { get => instance._dashDistance; }
    public static float DashSpeed { get => instance._dashSpeed; }

    public override void Awake()
    {
        base.Awake();
        instance = this;
    }
}
