using System;
using System.Collections.Generic;
using Cameras;
using Controller;
using Controller.Play;
using Managers;
using MEC;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UIs.Etc
{
    public class UI_PowerSaving : UI_Base
    {
        private EventsManager _stageChangeManager;

        enum Texts
        {
            T_Time,
            T_Date,
            T_StageNum,
            T_Gold,
            T_Exp,
            T_WeaponGrowth,
            T_AccessoryGrowth,
            T_Weapon,
            T_Accessory,
            T_BeadsOre
        }

        enum Sliders
        {
            IMG_UnlockSlider
        }

        private void Awake()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TextMeshProUGUI>(typeof(Texts));
            Bind<Slider>(typeof(Sliders));

            var trigger = new EventTrigger.TriggerEvent();
            trigger.AddListener(CheckSlider);
            Util.FindChild(gameObject, "IMG_UnlockSlider", true).GetOrAddComponent<EventTrigger>().triggers.Add(
                new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerUp,
                    callback = trigger
                });

            _stageChangeManager = new EventsManager(this, new EventsManager.Config
            {
                updatedField = new[] {PlayController.data.MonsterCount},
                handler = WhenStageChanged
            });
            WhenStageChanged();
            
            OnPowerSaving();
            return true;
        }

        private void CheckSlider(BaseEventData data)
        {
            if (Get<Slider>((int) Sliders.IMG_UnlockSlider).value < 0.9f)
            {
                Get<Slider>((int) Sliders.IMG_UnlockSlider).value = 0;
            }
            else
            {
                OffPowerSaving();
            }
        }

        private void OffPowerSaving()
        {
            CameraController.I.OnOffPowerSaving(false);
            Get<Slider>((int) Sliders.IMG_UnlockSlider).value = 0;
            Manager.Sound.MuteSound(false);
            PlayController.I.PowerSaveOnOff(false);
            Manager.UI.SetPowerSavingTimer();
            Manager.Resource.Destroy(gameObject);
            _stageChangeManager?.Dispose();
        }

        private void OnEnable()
        {
            if (_isInit)
            {
                OnPowerSaving();
            }
            _stageChangeManager?.Reconnect();
        }

        private void OnPowerSaving()
        {
            CameraController.I.OnOffPowerSaving(true);
            Manager.Sound.MuteSound(true);
            gameObject.SetActive(true);
            PlayController.I.PowerSaveOnOff(true);
            Timing.RunCoroutine(_PowerSavingGoldExp().CancelWith(gameObject));
            Timing.RunCoroutine(_SaveRoutine().CancelWith(gameObject));
            
            Get<TextMeshProUGUI>((int)Texts.T_Date).text = DateTime.Now.ToString("d");
            Timing.RunCoroutine(_TimeRoutine().CancelWith(gameObject));
        }

        public void WhenStageChanged()
        {
            Get<TextMeshProUGUI>((int) Texts.T_StageNum).text = Manager.Field.GetStageName();
        }

        IEnumerator<float> _PowerSavingGoldExp()
        {
            while (true)
            {
                Get<TextMeshProUGUI>((int) Texts.T_Gold).text = Define.AddUnit(PlayController.I.powerSavingGold, 3, 2);
                Get<TextMeshProUGUI>((int) Texts.T_Exp).text = Define.AddUnit(PlayController.I.powerSavingExp, 3, 2);
                Get<TextMeshProUGUI>((int) Texts.T_Weapon).text = Define.AddUnit(PlayController.I.powerSavingWeapon, 3, 2);
                Get<TextMeshProUGUI>((int) Texts.T_Accessory).text = Define.AddUnit(PlayController.I.powerSavingAccessory, 3, 2);
                Get<TextMeshProUGUI>((int) Texts.T_WeaponGrowth).text = Define.AddUnit(PlayController.I.powerSavingWeaponGrowth, 3, 2);
                Get<TextMeshProUGUI>((int) Texts.T_AccessoryGrowth).text = Define.AddUnit(PlayController.I.powerSavingAccessoryGrowth, 3, 2);
                Get<TextMeshProUGUI>((int) Texts.T_BeadsOre).text = Define.AddUnit(PlayController.I.powerSavingBeadsOre, 3, 2);
                yield return Timing.WaitForSeconds(3);
            }
        }

        IEnumerator<float> _SaveRoutine()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(30);
                // PlayFabManager.Store.Save(PlayFabStore.SaveType.Currency, PlayFabStore.SaveType.Weapon, PlayFabStore.SaveType.Accessory);
            }
        }

        IEnumerator<float> _TimeRoutine()
        {
            Get<TextMeshProUGUI>((int) Texts.T_Time).text = DateTime.Now.ToString("HH:mm");
            yield return Timing.WaitForSeconds(60 - DateTime.Now.Second);
            while (true)
            {
                Get<TextMeshProUGUI>((int) Texts.T_Time).text = DateTime.Now.ToString("HH:mm");
                yield return Timing.WaitForSeconds(60);
            }
        }
    }
}