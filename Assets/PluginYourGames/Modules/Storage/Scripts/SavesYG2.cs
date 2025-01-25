
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
        public Queue<int> LevelScheduleList = new Queue<int>();

    }
}
