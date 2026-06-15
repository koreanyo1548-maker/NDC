using System.Collections.Generic;
using Controller.Infos;
using Controller.Play;
using Data;
using MEC;

namespace Managers.Game
{
    public class TimeManager
    {
        public void StartTimeCheck()
        {
            Timing.RunCoroutine(_PlayTimeChecker());
        }
        
        IEnumerator<float> _PlayTimeChecker()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(60f);
                QuestController.I.DoQuests(QuestType.PlayTime);
                DropEventController.I.TryDrop();
            }
        }
    }
}