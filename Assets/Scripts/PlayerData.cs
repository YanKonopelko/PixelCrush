using System;
using System.Collections.Generic;
using System.Diagnostics;
using YG;

public class PlayerData
{
    // const variables
    public static PlayerData Instance = new PlayerData();
    public const int LevelsCount = 113;
    private List<int> LevelScheduleList = new List<int>();

    public PlayerData()
    {
        FromSave();
    }

    // cahngable variables
    private int currentLevel;
    private int additionalIndex;
    private bool tutorialComplete = false;
    public bool TutorialComplete{
        get{return tutorialComplete;}
    }

    private bool loseTutorialComplete = false;
    public bool LoseTutorialComplete{
        get{return loseTutorialComplete;}
    }

    private bool fxEnable = true;

    public bool EffectsEnabled
    {
        get { return fxEnable; }
        set
        {
            fxEnable = value; YG2.saves.fxEnable = fxEnable;
        }
    }
    private bool vibrationEnable = true;

    public bool VibrationEnable
    {
        get { return vibrationEnable; }
        set
        {
            vibrationEnable = value; YG2.saves.vibrationEnable = fxEnable;
        }
    }

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

    public void MarkTutorialComplete(){
        Dictionary<string, string> eventData = new Dictionary<string, string>();
        eventData.Add("Step","0");
        YG2.MetricaSend("TutorialCompleted",eventData);
        tutorialComplete = true;
    }
    public void MarkLoseTutorialComplete(){
        Dictionary<string, string> eventData = new Dictionary<string, string>();
        eventData.Add("Step","1");
        YG2.MetricaSend("TutorialCompleted",eventData);
        loseTutorialComplete = true;
    }

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
        fxEnable = YG2.saves.fxEnable;
        tutorialComplete = YG2.saves.tutorialComplete;
        loseTutorialComplete = YG2.saves.loseTutorialComplete;
        vibrationEnable = YG2.saves.vibrationEnable;
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
                LevelScheduleList.Add(ar[i]);
            }
            Debug.Print("Update schedule");
        }
        int num = LevelScheduleList[LevelScheduleList.Count - 1];
        LevelScheduleList.Remove(LevelScheduleList[LevelScheduleList.Count - 1]);
        Save();
        return num;
    }

    public void Save()
    {
        YG2.saves.currentLevel = currentLevel;
        YG2.saves.additionalIndex = additionalIndex;
        YG2.saves.LastLevel = LastLevel;
        YG2.saves.LevelScheduleList = LevelScheduleList;
        YG2.saves.fxEnable = fxEnable;
        YG2.saves.tutorialComplete = tutorialComplete;
        YG2.saves.loseTutorialComplete = loseTutorialComplete;
        YG2.saves.vibrationEnable = vibrationEnable;
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
