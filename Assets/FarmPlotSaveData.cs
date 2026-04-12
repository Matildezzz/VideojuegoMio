using System;

[Serializable]
public class FarmPlotSaveData
{
    public string plotKey;
    public bool isTilled;
    public bool isWateredToday;

    public bool hasCrop;
    public string seedItemId;

    public CropSaveData crop;
}