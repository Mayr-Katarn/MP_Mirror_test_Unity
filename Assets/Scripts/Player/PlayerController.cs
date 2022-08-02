using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PlayerController : NetworkBehaviour
{
    public Animator animator;
    [SerializeField] private SkinnedMeshRenderer _meshRenderer;

    [SyncVar(hook = nameof(ApplyHit)), HideInInspector] public bool isHited = false;
    [SyncVar(hook = nameof(SetPoints)), HideInInspector] public int score = 0;
    [SyncVar(hook = nameof(SetColor)), HideInInspector] public ColorType playerColor;
    [SyncVar(hook = nameof(SetGameOver)), HideInInspector] private bool isGameOver = false;

    private Transform _transform;
    private PlayerMovement _playerMovement;
    private CapsuleCollider _playerCollider;
    private readonly Dictionary<ColorType, Color> _colorDictionary = new Dictionary<ColorType, Color>();
    private readonly SyncList<PlayerInfo> _playersInfo = new SyncList<PlayerInfo>();

    private void Awake()
    {
        Cursor.visible = false;
        _transform = transform;
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCollider = GetComponent<CapsuleCollider>();
        InitColorDictionary();
    }

    public override void OnStartClient()
    {
        _playersInfo.Callback += OnPlayersInfoUpdated;
        CmdAddPlayerToList();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isGameOver && _playerMovement.isDashing && other.gameObject.TryGetComponent(out PlayerController targetPlayer))
        {
            CmdApplyHitToPlayer(netId, targetPlayer.netId);
        }
    }

    private void InitColorDictionary()
    {
        _colorDictionary.Add(ColorType.Default, Color.white);
        _colorDictionary.Add(ColorType.Hit, Color.red);
    }

    private void ResetPlayer()
    {
        Transform startPosition = NetworkManager.singleton.GetStartPosition();
        _transform.SetPositionAndRotation(startPosition.position, startPosition.rotation);
        CmdResetPlayerScoreList();
    }
    private IEnumerator ApplyHitCooldown()
    {
        yield return new WaitForSeconds(CustomNetworkRoomManager.PlayerHitTime);
        CmdHitIsOver(netId);
    }

    #region SYNC VALUES
    private void OnPlayersInfoUpdated(SyncList<PlayerInfo>.Operation op, int index, PlayerInfo oldInfo, PlayerInfo newInfo)
    {
        switch (op)
        {
            case SyncList<PlayerInfo>.Operation.OP_ADD:
            case SyncList<PlayerInfo>.Operation.OP_SET:
            case SyncList<PlayerInfo>.Operation.OP_INSERT:
                UiEvents.SendUpdatePlayerInfo(newInfo);
                break;
            case SyncList<PlayerInfo>.Operation.OP_REMOVEAT:
                UiEvents.SendUpdatePlayerInfo(oldInfo, true);
                break;
        }
    }

    private void ApplyHit(bool oldValue, bool newValue)
    {
        isHited = newValue;
        StartCoroutine(ApplyHitCooldown());
    }

    private void SetColor(ColorType oldValue, ColorType newValue)
    {
        foreach (Material material in _meshRenderer.materials)
            material.color = _colorDictionary[newValue];
    }

    private void SetPoints(int oldValue, int newValue)
    {
        score = newValue;
    }

    private void SetGameOver(bool oldValue, bool newValue)
    {
        isGameOver = newValue;
        _playerCollider.enabled = !isGameOver;

        if (!isGameOver)
            ResetPlayer();
    }
    #endregion

    #region CLIENT RPC
    [ClientRpc]
    private void RpcUpdateGameOverInfo(string winnerId, string timeToRestart)
    {
        UiEvents.SendSetGameOverText(winnerId, timeToRestart);
    }
    #endregion

    #region COMMANDS
    [Command(requiresAuthority = false)]
    private void CmdAddPlayerToList()
    {
        List<NetworkIdentity> playersIdentity = NetworkServer.spawned.Values.ToList();

        foreach (NetworkIdentity identity in playersIdentity)
        {
            bool isPlayer = identity.TryGetComponent(out PlayerController player);
            if (!isPlayer)
                continue;

            bool isExistInList = _playersInfo.Any(info => info.netId == player.netId);
            if (isExistInList)
                continue;

            _playersInfo.Add(new PlayerInfo(player.netId));
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdResetPlayerScoreList()
    {
        for (int i = 0; i < _playersInfo.Count; i++)
        {
            PlayerInfo playerInfo = _playersInfo[i];
            playerInfo.score = 0;
            _playersInfo[i] = playerInfo;
        }
    }

    [Command]
    private void CmdApplyHitToPlayer(uint playerNetId, uint targetPlayerNetId)
    {
        PlayerController targetPlayer = NetworkServer.spawned[targetPlayerNetId].GetComponent<PlayerController>();

        if (!targetPlayer.isHited)
        {
            targetPlayer.isHited = true;
            targetPlayer.playerColor = ColorType.Hit;

            PlayerController player = NetworkServer.spawned[playerNetId].GetComponent<PlayerController>();
            player.score++;
            UpdatePlayersInfoPoints(player);

            if (player.score >= CustomNetworkRoomManager.HitsToWin)
            {
                GameOver(playerNetId);
            }
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdHitIsOver(uint playerNetId)
    {
        PlayerController player = NetworkServer.spawned[playerNetId].GetComponent<PlayerController>();
        player.isHited = false;
        player.playerColor = ColorType.Default;
    }
    #endregion

    #region SERVER
    [Server]
    private void UpdatePlayersInfoPoints(PlayerController player)
    {
        PlayerInfo playerInfo = _playersInfo.FirstOrDefault(info => info.netId == player.netId);
        int index = _playersInfo.IndexOf(playerInfo);
        playerInfo.score = player.score;
        _playersInfo[index] = playerInfo;
    }

    [Server]
    public override void OnStopClient()
    {
        PlayerInfo playerInfo = _playersInfo.Find(info => info.netId == netId);
        int index = _playersInfo.IndexOf(playerInfo);
        _playersInfo.RemoveAt(index);
    }

    [Server]
    private void GameOver(uint winnerId)
    {
        List<NetworkIdentity> playersIdentity = NetworkServer.spawned.Values.ToList();

        foreach (NetworkIdentity identity in playersIdentity)
        {
            bool isPlayer = identity.TryGetComponent(out PlayerController player);
            if (!isPlayer)
                continue;

            player.isGameOver = true;
        }

        StartCoroutine(GameOverInfo(winnerId));
    }

    [Server]
    private IEnumerator GameOverInfo(uint winnerId)
    {
        float timeToRestart = 5;

        while (timeToRestart > 0)
        {
            RpcUpdateGameOverInfo(winnerId.ToString(), timeToRestart.ToString());
            yield return new WaitForSeconds(1);
            timeToRestart--;
        }

        StartNewGame();
        yield return null;
    }

    [Server]
    private void StartNewGame()
    {
        isGameOver = false;
        RpcUpdateGameOverInfo("", "");
        ResetPlayers();
    }

    [Server]
    private void ResetPlayers()
    {
        for (int i = 0; i < _playersInfo.Count; i++)
        {
            PlayerInfo playerInfo = _playersInfo[i];
            PlayerController player = NetworkServer.spawned[playerInfo.netId].GetComponent<PlayerController>();
            player.score = 0;
            player.isGameOver = false;
        }
    }
    #endregion
}
