using System;
using Data;
using Data.DbDefinition;
using Data.DbEquipment;

using Exceptions;
using Managers;
using TMPro;
using UIBases;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.RewardLog
{
    public class UI_RewardLog_Item: UI_Base
    {
        private bool _canDisappear;
        enum Images
        {
            IMG_ItemIcon
        }

        enum Texts
        {
            T_GetItemInfo
        }


        private Animator _animator;
        private static readonly int CanDisappear = Animator.StringToHash("CanDisappear");

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<Image>(typeof(Images));
            Bind<TextMeshProUGUI>(typeof(Texts));
            _animator = transform.GetComponent<Animator>();

            Manager.Field.CurField.ValueChanged += (_, _) => ChangeCanDisappear();
            ChangeCanDisappear();

            return true;
        }

        public void Set(CurrencyType currency, long amount, int id = -1)
        {
            if (!_isInit) Init();

            var meta = DbCurrency.Get(currency);
            Get<Image>((int) Images.IMG_ItemIcon).sprite =meta.GetResource(id);
            Get<TextMeshProUGUI>((int) Texts.T_GetItemInfo).text = $"{LocalString.Get(meta.GetNameId(id))}(+{amount:N0})";
            
            transform.SetSiblingIndex(0);
            gameObject.SetActive(true);
            _animator.Play("Start", 0, 0);
            _animator.SetBool(CanDisappear, _canDisappear);
        }

        private void ChangeCanDisappear()
        {
            var field = Manager.Field.CurField.Value;
            switch (field)
            {
                case FieldType.Stage: case FieldType.Awakening: case FieldType.Pet: case FieldType.Promotion: case FieldType.SkillGrowth: case FieldType.Training:
                    _canDisappear = true;
                    break;
                case FieldType.BlackMarket: case FieldType.Dia:
                    _animator.Play("End", 0);
                    _canDisappear = false;
                    break;
                default:
                    throw new NotDefinedFieldException(field);
            }
        }
    }
}