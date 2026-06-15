using Controller.Currency;
using Controller.Infos;
using Data;
using Data.DbCommon;
using Managers;
using UIBases;
using UIs.Toast;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UIs.Character.Promotion
{
    public class UI_Promotion: UI_Base
    {
        // private EventsManager _salaryEventsManager;
        // private EventsManager _promotionEventsManager;

        // enum Images
        // {
        //     B_GetAll
        // }
        // enum Transforms
        // {
        //     SalaryParent
        // }

        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;

            // Bind<Transform>(typeof(Transforms));
            // Bind<Image>(typeof(Images));
            //
            // Get<Image>((int) Images.B_GetAll).gameObject.BindEvent(CurrencyController.I.CanGetAnySalary,
            //     _ => GetAllSalaries(), UIEffectType.Bounce, false);

            // var promotion = LevelController.data.Promotion.Value;
            // for (var idx = 1; idx <= promotion; ++idx)
            // {
            //     Manager.UI.MakeSubItem<UI_PromotionSalary_Item>(Get<Transform>((int)Transforms.SalaryParent)).Set(idx);
            // }

            // _promotionEventsManager = new EventsManager(this, new EventsManager.Config
            // {
            //     handler = WhenPromotionChanged,
            //     updatedField = new[] {LevelController.data.Promotion}
            // });

            // _salaryEventsManager = new EventsManager(this, new EventsManager.Config
            // {
            //     handler = WhenSalaryStatusChanged,
            //     updatedField = new[] {CurrencyController.data.Salaries}
            // });
            // WhenSalaryStatusChanged();
            return true;
        }

        // private void WhenSalaryStatusChanged()
        // {
        //     Get<Image>((int) Images.B_GetAll).material = Define.GetUIMaterial(!CurrencyController.I.CanGetAnySalary());
        // }


        // private void WhenPromotionChanged()
        // {
        //     var promotion = LevelController.data.Promotion.Value;
        //     var exists = Get<Transform>((int) Transforms.SalaryParent).childCount;
        //     for (var idx = exists + 1; idx <= promotion; ++idx)
        //     {
        //         Manager.UI.MakeSubItem<UI_PromotionSalary_Item>(Get<Transform>((int)Transforms.SalaryParent)).Set(idx);
        //     }
        // }
        
        // private void OnDisable()
        // {
        //     _promotionEventsManager?.Dispose();
        //     _salaryEventsManager?.Dispose();
        // }
        //
        // private void OnEnable()
        // {
        //     _promotionEventsManager?.Reconnect();
        //     _salaryEventsManager?.Reconnect();
        // }
    }
}