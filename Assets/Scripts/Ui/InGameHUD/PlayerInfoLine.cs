using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInfoLine : MonoBehaviour
{
    private uint _playerNetId;
    public uint PlayerNetId => _playerNetId;

    private RectTransform _rectTransform;
    private TMP_Text _TMPtext;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _TMPtext = GetComponent<TMP_Text>();
    }

    public void Init(Vector2 anchoredPosition, PlayerInfo info)
    {
        _playerNetId = info.netId;
        SetPosition(anchoredPosition);
        UpdatePlayerInfo(info);
    }

    public void SetPosition(Vector2 anchoredPosition)
    {
        _rectTransform.anchoredPosition = anchoredPosition;
    }

    public void UpdatePlayerInfo(PlayerInfo info)
    {
        string text = $"Player[{info.netId}] | {info.score}";
        _TMPtext.text = text;
    }
}
