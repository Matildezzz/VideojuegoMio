using System;

[Serializable]
public class ChestSaveData
{
    public string chestId;
    public ContainerSaveData container = new ContainerSaveData();
}