using System.Collections.Generic;
using Data.Editor.EDbEvent;
using Data.Editor.EDbShop;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.Editor.Excel
{
    [ExcelAsset(ExcelName = "Event")]
    public class EDbEvents: ScriptableObject
    {
        [SerializeField] public List<EDbSeasonPass> SeasonPass;
        [SerializeField] public List<EDbSeasonPassReward> SeasonPassReward;
        [SerializeField] public List<EDbSeasonPassQuest> SeasonPassQuest;
        [SerializeField] public List<EDbAttendEvent> AttendEvent;
        [SerializeField] public List<EDbAttendEventReward> AttendEventReward;
        [SerializeField] public List<EDbDropEvent> DropEvent;
        [SerializeField] public List<EDbDropEventShop> DropEventShop;
        [SerializeField] public List<EDbAttendReward> AttendReward;
        [SerializeField] public List<EDbFriendReward> FriendReward;
        
    }
}