
using System.Collections.Generic;
using InventoryNamespace;

namespace YG
{
    [System.Serializable]
    public partial class SavesYG
    {
        public int idSave;
        //Level Progress 
        public int currentLevel = 0;
        public int additionalIndex = -1;
        public int LastLevel = -1;
        public List<int> LevelScheduleList = new List<int>();

        //Settings 
        public bool soundsEnabled = true;
        public bool musicEnabled = true;
        public bool fxEnable = true;
        public bool vibrationEnable = true;

        //Tutorial 
        public bool tutorialComplete = false;
        public bool loseTutorialComplete = false;

        //Currencies
        public int coins = 0;


        //Inventory
        public List<uint> collectedItems;
        public List<uint> equipedItems;
        public Item equipedCircleSkin;
        public Item equipedStickSkin;

    }
}
