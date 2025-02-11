
using System.Collections.Generic;

namespace YG
{
    [System.Serializable]
    public partial class SavesYG
    {
        public int idSave;
        public int currentLevel = 0;
        public int additionalIndex = -1;
        public int LastLevel = -1;
        public List<int> LevelScheduleList = new List<int>();

        public bool soundsEnabled = true;
        public bool musicEnabled = true;
        public bool fxEnable = true;
        public bool vibrationEnable = true;
        public bool tutorialComplete = false;
        public bool loseTutorialComplete = false;
    }
}
