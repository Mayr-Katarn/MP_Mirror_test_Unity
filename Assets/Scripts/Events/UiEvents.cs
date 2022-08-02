using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UiEvents
{
    public static UnityEvent<string, string> OnSetGameOverText = new UnityEvent<string, string>();
    public static UnityEvent<PlayerInfo, bool> OnUpdatePlayerInfo = new UnityEvent<PlayerInfo, bool>();

    public static void SendSetGameOverText(string winnerId, string timeToRestart = "") => OnSetGameOverText.Invoke(winnerId, timeToRestart);
    public static void SendUpdatePlayerInfo(PlayerInfo playersInfo, bool isRemove = false) => OnUpdatePlayerInfo.Invoke(playersInfo, isRemove);
}
