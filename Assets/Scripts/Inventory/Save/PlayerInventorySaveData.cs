using System;
using UnityEngine;

[Serializable]
public class PlayerInventorySaveData
{
    public ContainerSaveData hotbar = new ContainerSaveData();
    public ContainerSaveData backpack = new ContainerSaveData();
    public int selectedHotbarIndex;
}
