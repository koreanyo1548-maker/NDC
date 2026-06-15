using System;
using Cameras;
using Controller.Infos;
using Data;
using Data.DbDefinition;
using Data.DbEquipment;
using Data.DbSummon;
using Data.Utils;
using DG.Tweening;
using Managers;
using Managers.Base;
using MEC;
using TMPro;
using UIBases;
using UIs.FieldMain;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Summon
{
    public class UI_SummonResult_Item : UI_Base
    {
        private UI_SummonResult _popup;
        enum Images
        {
            IMG_Grade,
            IMG_Equipment
        }

        enum Particles
        {
            Glowing,
            Line,
            Unique,
            Heroic,
            Legendary,
            Mythic
        }

        enum Animators
        {
            GlowCover
        }

        enum Texts
        {
            T_Grade,
            T_Count
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<Image>(typeof(Images));
            Bind<Animator>(typeof(Animators));
            Bind<ParticleSystem>(typeof(Particles));
            Bind<TextMeshProUGUI>(typeof(Texts));
            _popup = Manager.UI.GetPopupUI<UI_SummonResult>();
            
            return true;
        }

        public float SetInfo(IDbCanSummon summon, SummonType summonType, int count, float wait)
        {
            if (!_isInit) Init();

            for (var particle = 0; particle <= (int) Particles.Mythic; ++particle)
            {
                Get<ParticleSystem>(particle).gameObject.SetActive(false);
            }

            gameObject.SetActive(false);
            var effectType = GetEffectVersion();
            Timing.CallDelayed(wait, Show);
            return wait + (effectType == 0 ? 2f : effectType == 1 ? 0.75f : 0.05f);

            void Show()
            {
                var grade = summon.GetGrade();
                Manager.Sound.PlaySFX(grade <= GradeType.Rare ? SFXType.SummonItem : SFXType.SummonItem_Special);
                Get<Image>((int) Images.IMG_Grade).sprite = Manager.Resource.Load<Sprite>(grade.ToString());
                Get<Image>((int) Images.IMG_Equipment).sprite = Manager.Resource.Load<Sprite>(summon.GetResource());
                
                Get<TextMeshProUGUI>((int) Texts.T_Grade).text = LocalString.Get(DbGrade.Get(grade).NameId);
                Get<TextMeshProUGUI>((int) Texts.T_Count).text = count > 0 ? count.ToString() : string.Empty;

                if (grade >= GradeType.Heroic)
                {
                    var color = Define.SummonFxColor(grade);
                    for (var idx = (int) Particles.Glowing; idx <= (int) Particles.Line; ++idx)
                    {
                        var main = Get<ParticleSystem>(idx).main;
                        main.startColor = color;
                    }
                }

                transform.localScale = Vector3.zero;
                gameObject.SetActive(true);
                if (grade >= GradeType.Heroic && effectType == -1)
                {
                    PlayParticle(Get<ParticleSystem>((int) Particles.Glowing));
                }

                if (effectType == -1)
                {
                    Get<Animator>((int) Animators.GlowCover).Play("Summon", 0, 0);
                }
                else
                {
                    _popup.Shake(effectType == 0 ? 2 : 0.5f);
                    Timing.CallDelayed(effectType == 0 ? 2 : 0.5f, () =>
                    {
                        PlayParticle(Get<ParticleSystem>((int) Particles.Unique + grade - GradeType.Unique), true);
                        PlayParticle(Get<ParticleSystem>((int) Particles.Glowing));
                        PlayParticle(Get<ParticleSystem>((int) Particles.Line));
                        Get<Animator>((int) Animators.GlowCover).Play("Summon", 0, 0);
                    });
                }

                transform.DOScale(Define.OnePointFive, 0.15f).SetEase(Ease.InQuart).OnComplete(() =>
                {
                    transform.DOScale(Define.One, 0.5f).SetEase(Ease.OutQuart);
                });
            }

            void PlayParticle(ParticleSystem particle, bool changeSpeed = false)
            {
                if (changeSpeed)
                {
                    var childs = particle.transform.GetComponentsInChildren<ParticleSystem>();
                    foreach (var child in childs)
                    {
                        var main = child.main;
                        main.simulationSpeed = effectType == 0 ? 0.5f : 2;
                    }
                }
                particle.gameObject.SetActive(true);
                particle.Simulate(0, true, true);
                particle.Play();
            }

            // -1: no effect, 0: long version, 1: short version
            int GetEffectVersion()
            {
                var isRorN = summonType == SummonType.Relic || summonType == SummonType.Necklace;
                
                var grade = summon.GetGrade();
                if (grade < GradeType.Unique) return -1;
                
                // 무/장/스 유니크 처음 뽑을 때 연출 2 진행
                if (!isRorN && grade == GradeType.Unique)
                {
                    if (DbSelector.GetUserEquipment(Define.SummonTypeToEquipmentType(summonType), summon.GetId()).IsNew())
                    {
                        return 1;
                    }
                }
                
                var curLevel = Math.Min(summonType == SummonType.Weapon || summonType == SummonType.Accessory ? 10 : DbSummonLevel.Count, LevelController.I.GetSummonLevel(summonType));
                if (!isRorN)
                {
                    // 무/장/스 0.3% 이하면 연출 1 진행
                    if (DbSelector.GetSummonProbability(summonType, curLevel).GetPr(grade - GradeType.Normal) <= 3000)
                    {
                        return 0;
                    }
                    
                    // 무/장/스 영웅 등급 처음 뽑히면 연출 1 진행
                    if (DbSelector.GetUserEquipment(Define.SummonTypeToEquipmentType(summonType), summon.GetId()).IsNew())
                    {
                        return 0;
                    }
                }
                
                // 2% 이하면 연출 2 진행
                if (DbSelector.GetSummonProbability(summonType, curLevel).GetPr(grade - GradeType.Normal) <= 20000) return 1;

                return -1;
            }
        }
    }
}