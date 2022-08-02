using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class PlayerInfoTable : MonoBehaviour
{
    [SerializeField] private PlayerInfoLine _playerInfoLinePrefab;

    private Transform _transform;
    private const float LINE_HEIGHT = 16;
    private readonly List<PlayerInfoLine> _playersInfoLines = new List<PlayerInfoLine>();

    private void Awake()
    {
        _transform = transform;
    }

    private void OnEnable()
    {
        UiEvents.OnUpdatePlayerInfo.AddListener(UpdatePlayersInfo);
    }

    private void UpdatePlayersInfo(PlayerInfo playersInfo, bool isRemove)
    {
        PlayerInfoLine playerInfoLine = _playersInfoLines.Find(line => line.PlayerNetId == playersInfo.netId);

        if (playerInfoLine != null)
        {
            if (isRemove)
            {
                Destroy(playerInfoLine.gameObject);
                _playersInfoLines.Remove(playerInfoLine);
                ResetLinesPosition();
            }
            else
            {
                playerInfoLine.UpdatePlayerInfo(playersInfo);
            }
        }
        else if (!isRemove)
        {
            CreatePlayerLineInfo(playersInfo);
        }
    }

    private void CreatePlayerLineInfo(PlayerInfo playersInfo)
    {
        Vector2 anchoredPosition = new Vector2(0, LINE_HEIGHT * _playersInfoLines.Count);
        PlayerInfoLine playerInfoLine = Instantiate(_playerInfoLinePrefab, _transform);
        playerInfoLine.Init(anchoredPosition, playersInfo);
        _playersInfoLines.Add(playerInfoLine);
    }

    private void ResetLinesPosition()
    {
        for (int i = 0; i < _playersInfoLines.Count; i++)
        {
            Vector2 position = new Vector2(0, LINE_HEIGHT * i);
            _playersInfoLines[i].SetPosition(position);
        }
    }
}
