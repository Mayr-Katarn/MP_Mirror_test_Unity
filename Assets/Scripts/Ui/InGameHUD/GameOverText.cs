using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class GameOverText : MonoBehaviour
{
    private TMP_Text _text;

    private void Awake() => _text = GetComponent<TMP_Text>();

    private void OnEnable() => UiEvents.OnSetGameOverText.AddListener(UpdateDebugText);

    private void UpdateDebugText(string winerId, string timeToRestart)
    {
        bool isActive = winerId.Length != 0 && timeToRestart.Length != 0;
        gameObject.SetActive(isActive);
        _text.text = $"Player [{winerId}] WIN\nrestart in {timeToRestart}";
    }
}
