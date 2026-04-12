using System;

[Serializable]
public class CropSaveData
{
    public int stageIndex;
    public int wateredDaysInCurrentStage;
    public int dryDaysWithoutWater;
    public bool isHarvestable;
    public bool isWithered;
}