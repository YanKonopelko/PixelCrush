using System;
using YG;

public class PlayerData
{
    // const variables
    public static PlayerData Instance = new PlayerData();
    public const int LevelsCount = 11;
    public PlayerData()
    {
        FromSave();
    }

    // cahngable variables
    int currentLevel;
    int additionalIndex;

    // public variables
    public int CurrentLevel
    {
        get { return currentLevel; }
    }
    public int AdditionalIndex
    {
        get { return additionalIndex; }
    }
    public int LastLevel;

    public void MarkLevelComplete()
    {

        currentLevel = Math.Clamp(currentLevel + 1, 0, LevelsCount);
        if (currentLevel == LevelsCount)
        {
            additionalIndex += 1;
        }
        //DoAnaliticsEvent
    }
    public bool IsMaxLevelNow(){
        return currentLevel == LevelsCount;
    }

    private void FromSave()
    {
        currentLevel = YG2.saves.currentLevel;
        additionalIndex = YG2.saves.additionalIndex;
        LastLevel = YG2.saves.LastLevel;
    }

    public void Save()
    {
        YG2.saves.currentLevel = currentLevel;
        YG2.saves.additionalIndex = additionalIndex;
        YG2.saves.LastLevel = LastLevel;
        YG2.SaveProgress();
    }

}
