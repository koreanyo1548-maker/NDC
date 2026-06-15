using System;
using Controller.Infos;
using Data;
using Data.DbUser.Progress;
using ThirdParty;
using Utils;

namespace Controller.Have
{
    public class TitleController : Singleton<TitleController>
    {
        public TitleController()
        {
            DbUserTitle.ForEach(t =>
            {
                var initialCount = QuestController.I.GetInitQuestCount(t.Meta.ToDo);
                if (initialCount > 0 && t.DoCount.Value != initialCount) t.DoCount.Value = initialCount;
            });
        }

        public void DoQuests(QuestType toDo, int add)
        {
            Predicate<DbUserTitle> predicate = t => t.Level.Value < t.Meta.MaxLevel && t.Meta.ToDo == toDo;
            DbUserTitle.ForEach(predicate, DoQuest);
            // if (toDo != QuestType.MonsterKillCount) PlayFabManager.Store.Save(PlayFabStore.SaveType.Title);

            void DoQuest(DbUserTitle title)
            {
                title.DoCount.Value = Math.Min(title.DoCount.Value + add, title.Meta.Goal[title.Meta.MaxLevel-1]);
            }
        }
        
        public void SetQuests(QuestType toDo, int count)
        {
            Predicate<DbUserTitle> predicate = t => t.Level.Value < t.Meta.MaxLevel && t.Meta.ToDo == toDo;
            DbUserTitle.ForEach(predicate, SetQuest);
            // PlayFabManager.Store.Save(PlayFabStore.SaveType.Title);

            void SetQuest(DbUserTitle title)
            {
                title.DoCount.Value = Math.Min(count, title.Meta.Goal[title.Meta.MaxLevel-1]);
            }
        }
    }
}