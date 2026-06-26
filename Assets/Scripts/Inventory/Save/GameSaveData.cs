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

    public bool hasCameraBoundary;
    public string cameraBoundaryName;

    public bool hasPlayerGold;
    public int playerGold;

    public bool hasTimeData;
    public int day;
    public int hour;
    public int minute;

    public List<FarmPlotSaveData> farmPlots = new List<FarmPlotSaveData>();

    public List<string> museumDonatedItemIds = new List<string>();

    public List<NPCFriendshipSaveData> npcFriendships = new List<NPCFriendshipSaveData>();

    public LanguageSaveData language = new LanguageSaveData();
    public AlienDiarySaveData alienDiary = new AlienDiarySaveData();
    public AlienNameSaveData alienNames = new AlienNameSaveData();
    public QuestSystemSaveData quests = new QuestSystemSaveData();
}
