using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public PlayerInventorySaveData playerInventory = new PlayerInventorySaveData();
    public List<ChestSaveData> chests = new List<ChestSaveData>();
}