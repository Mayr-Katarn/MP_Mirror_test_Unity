using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct PlayerInfo
{
    public PlayerInfo(uint netId, int score = 0)
    {
        this.netId = netId;
        this.score = score;
    }

    public uint netId;
    public int score;
}
