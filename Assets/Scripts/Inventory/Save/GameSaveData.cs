using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public PlayerInventorySaveData playerInventory = new PlayerInventorySaveData();
    public List<ChestSaveData> chests = new List<ChestSaveData>();
    public bool hasPlayerPosition;
    public Vector3 playerPosition;
    public bool hasPlayerGold;
    public int playerGold;
}
