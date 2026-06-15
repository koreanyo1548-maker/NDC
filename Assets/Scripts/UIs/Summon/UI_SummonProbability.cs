using System;
using System.Collections.Generic;
using Controller;
using Controller.Infos;
using Data;

using Data.DbSummon;
using Data.Utils;
using dynamicscroll;
using Managers;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine;
using Utils;

namespace UIs.Summon
{
    public class UI_SummonProbability: UI_Popup
     {
         private SummonType _summonType;
         private int _curLevel;
         
         enum Texts
         {
             T_SummonProbability,
             T_Level,
             T_Probability0,
             T_Probability1,
             T_Probability2,
             T_Probability3,
             T_Probability4,
             T_Probability5,
             T_Probability6
         }

         enum GameObjects
         {
             B_Next,
             B_Prev
         }

         public override bool Init()
         {
             if (!base.Init()) return false;
 
             Bind<TextMeshProUGUI>(typeof(Texts));
             Bind<GameObject>(typeof(GameObjects));
             
             Get<GameObject>((int)GameObjects.B_Prev).BindEvent(Functions.TrueCondition, _=>MoveLevel(-1), UIEffectType.Bounce);
             Get<GameObject>((int)GameObjects.B_Next).BindEvent(Functions.TrueCondition, _=>MoveLevel(1), UIEffectType.Bounce);
             Util.FindChild(gameObject, "IMG_Dimmed", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI(), UIEffectType.None, false);
             
             return true;
         }
         
         public void Set(SummonType summonType)
         {
             if (!_isInit) Init();
             
             _summonType = summonType;
             Get<TextMeshProUGUI>((int) Texts.T_SummonProbability).text =
                 string.Format(LocalString.Get(210286), StringMaker.GetSummonName(_summonType));

             _curLevel = Math.Min(summonType == SummonType.Weapon || summonType == SummonType.Accessory ? 10 : DbSummonLevel.Count, LevelController.I.GetSummonLevel(summonType));
             Set();
         }

         private void Set()
         {
             var isRelic = _summonType == SummonType.Relic; 
             Get<GameObject>((int)GameObjects.B_Prev).SetActive(!isRelic && _curLevel > 1);
             var maxSummonLevel = _summonType == SummonType.Skill ? DbSummonLevel.Count : 10;
             Get<GameObject>((int)GameObjects.B_Next).SetActive(!isRelic && _curLevel < maxSummonLevel);

             Get<TextMeshProUGUI>((int) Texts.T_Level).text = isRelic ? string.Empty :
                 string.Format(LocalString.Get(210100), StringMaker.GetSummonName(_summonType), _curLevel == maxSummonLevel ? _curLevel + "+" : _curLevel);

             var summonPr = DbSelector.GetSummonProbability(_summonType, _curLevel);
             for (var idx = (int) Texts.T_Probability0; idx <= (int) Texts.T_Probability6; ++idx)
             {
                 Get<TextMeshProUGUI>(idx).text = (summonPr.GetPr(idx - (int)Texts.T_Probability0) / 10000f) + "%";
             }
         }
         
         private void MoveLevel(int add)
         {
             _curLevel += add;
             Set();
         }
 
         public override bool NeedRaycast()
         {
             return true;
         }
 
         public override void WhenPopupClosed()
         {
         }
     }
 }