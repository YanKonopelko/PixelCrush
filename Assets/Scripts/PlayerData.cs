using System;
using System.Collections.Generic;
using System.Diagnostics;
using YG;

public class PlayerData
{
    // const variables
    public static PlayerData Instance = new PlayerData();
    public const int LevelsCount = 5;
    private Queue<int> LevelScheduleList = new Queue<int>();

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
    public bool IsMaxLevelNow()
    {
        return currentLevel == LevelsCount;
    }

    private void FromSave()
    {
        currentLevel = YG2.saves.currentLevel;
        additionalIndex = YG2.saves.additionalIndex;
        LastLevel = YG2.saves.LastLevel;
        LevelScheduleList = YG2.saves.LevelScheduleList;
    }

    public int GetLevelFromList()
    {
        if (LevelScheduleList.Count == 0)
        {
            int[] ar = new int[LevelsCount];
            for (int i = 0; i < LevelsCount; i++)
            {
                ar[i] = i;
            }
            Shuffle(ar);
            for (int i = 0; i < LevelsCount; i++)
            {
                LevelScheduleList.Enqueue(ar[i]);
            }
            Debug.Print("Update schedule");
        }
        return LevelScheduleList.Dequeue();
    }

    public void Save()
    {
        YG2.saves.currentLevel = currentLevel;
        YG2.saves.additionalIndex = additionalIndex;
        YG2.saves.LastLevel = LastLevel;
        YG2.saves.LevelScheduleList = LevelScheduleList;
        YG2.SaveProgress();
    }
    void Shuffle(int[] array)
    {
        Random random = new Random();
        int p = array.Length;
        for (int n = p - 1; n > 0; n--)
        {
            int r = random.Next(1, n);
            int t = array[r];
            array[r] = array[n];
            array[n] = t;
        }
    }
}
