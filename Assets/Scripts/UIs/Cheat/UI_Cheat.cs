using System.Collections.Generic;
using Controller;
using Controller.Currency;
using Controller.Infos;
using Controller.Play;
using Data;
using Data.DbDefinition;
using Data.DbEquipment;
using Data.DbPetInfo;

using Data.DbUser.Equipment;
using ThirdParty;
using TMPro;
using UIBases;
using UIs.Utils;
using UnityEngine.UI;
using Utils;

namespace UIs.Cheat
{
    public class UI_Cheat: UI_Popup
    {
        private List<TMP_Dropdown.OptionData> moneyOptions = new();
        private List<TMP_Dropdown.OptionData> stoneOptions = new();
        private List<TMP_Dropdown.OptionData> ticketOptions = new();
        private List<TMP_Dropdown.OptionData> bookOptions = new();
        private List<TMP_Dropdown.OptionData> haveOptions = new();
        private List<TMP_Dropdown.OptionData> weaponOptions = new();
        private List<TMP_Dropdown.OptionData> accessoryOptions = new();
        private List<TMP_Dropdown.OptionData> skillOptions = new();
        private List<TMP_Dropdown.OptionData> petOptions = new();

        private List<TMP_Dropdown.OptionData> levelOptions = new()
        {
            new TMP_Dropdown.OptionData("경험치"),
            new TMP_Dropdown.OptionData("설정 초기화"),
            new TMP_Dropdown.OptionData("최대스테이지"),
            new TMP_Dropdown.OptionData("데이터초기화")
        };

        private Dictionary<int, CurrencyType> money = new();
        private Dictionary<int, CurrencyType> stone = new();
        private Dictionary<int, CurrencyType> ticket = new();
        private Dictionary<int, CurrencyType> book = new();
        private Dictionary<int, CurrencyType> have = new();
        private Dictionary<int, int> weapon = new();
        private Dictionary<int, int> accessory = new();
        private Dictionary<int, int> skill = new();
        private Dictionary<int, int> pet = new();


        private List<int> counts = new() {1, 3, 5, 10, 50, 100, 500, 1000, 10000, 1000000};
        
        enum Dropdowns
        {
            Category,
            Field,
            Count
        }

        enum Texts
        {
            ResultText
        }

        private void Start()
        {
            Init();
        }

        public override bool Init()
        {
            if (!base.Init()) return false;
            Bind<TMP_Dropdown>(typeof(Dropdowns));
            Bind<TextMeshProUGUI>(typeof(Texts));
            
            DbCurrency.ForEach(c =>
            {
                switch (c.Category)
                {
                    case CurrencyCategoryType.Money:
                        money.Add(moneyOptions.Count, c.Id);
                        moneyOptions.Add(new TMP_Dropdown.OptionData(LocalString.Get(c.NameId)));
                        break;
                    case CurrencyCategoryType.Stone:
                        stone.Add(stoneOptions.Count, c.Id);
                        stoneOptions.Add(new TMP_Dropdown.OptionData(LocalString.Get(c.NameId)));
                        break;
                    case CurrencyCategoryType.Ticket:
                        ticket.Add(ticketOptions.Count, c.Id);
                        ticketOptions.Add(new TMP_Dropdown.OptionData(LocalString.Get(c.NameId)));
                        break;
                    case CurrencyCategoryType.Have:
                        if (c.Id == CurrencyType.LevelPass1) haveOptions.Add(new TMP_Dropdown.OptionData("레벨패스1"));
                        else if (c.Id == CurrencyType.LevelPass2) haveOptions.Add(new TMP_Dropdown.OptionData("레벨패스2"));
                        else if (c.Id == CurrencyType.LevelPass3) haveOptions.Add(new TMP_Dropdown.OptionData("레벨패스3"));
                        else if (c.Id == CurrencyType.LevelPass4) haveOptions.Add(new TMP_Dropdown.OptionData("레벨패스4"));
                        else if (c.Id == CurrencyType.LevelPass5) haveOptions.Add(new TMP_Dropdown.OptionData("레벨패스5"));
                        else if (c.Id == CurrencyType.StagePass1) haveOptions.Add(new TMP_Dropdown.OptionData("스테이지패스1"));
                        else if (c.Id == CurrencyType.StagePass2) haveOptions.Add(new TMP_Dropdown.OptionData("스테이지패스2"));
                        else if (c.Id == CurrencyType.StagePass3) haveOptions.Add(new TMP_Dropdown.OptionData("스테이지패스3"));
                        else if (c.Id == CurrencyType.StagePass4) haveOptions.Add(new TMP_Dropdown.OptionData("스테이지패스4"));
                        else if (c.Id == CurrencyType.StagePass5) haveOptions.Add(new TMP_Dropdown.OptionData("스테이지패스5"));
                        else if (c.Id == CurrencyType.AdSkip) haveOptions.Add(new TMP_Dropdown.OptionData("광고 제거"));
                        else if (c.Id == CurrencyType.Bookshelf2) haveOptions.Add(new TMP_Dropdown.OptionData("2번 책장"));
                        else if (c.Id == CurrencyType.Bookshelf3) haveOptions.Add(new TMP_Dropdown.OptionData("3번 책장"));
                        else return;
                        have.Add(haveOptions.Count-1, c.Id);
                        break;
                    case CurrencyCategoryType.Book:
                        book.Add(bookOptions.Count, c.Id);
                        bookOptions.Add(new TMP_Dropdown.OptionData(LocalString.Get(c.NameId)));
                        break;
                }
            });
            DbWeapon.ForEach(w =>
            {
                weapon.Add(weaponOptions.Count, w.Id);
                weaponOptions.Add(new TMP_Dropdown.OptionData(LocalString.Get(w.NameId)));
            });
            DbAccessory.ForEach(a =>
            {
                accessory.Add(accessoryOptions.Count, a.Id);
                accessoryOptions.Add(new TMP_Dropdown.OptionData(LocalString.Get(a.NameId)));
            });
            DbSkill.ForEach(s =>
            {
                skill.Add(skillOptions.Count, s.Id);
                skillOptions.Add(new TMP_Dropdown.OptionData(LocalString.Get(s.NameId)));
            });
            DbPet.ForEach(p =>
            {
                pet.Add(petOptions.Count, p.Id);
                petOptions.Add(new TMP_Dropdown.OptionData(LocalString.Get(p.NameId)));
            });
            
            Get<TMP_Dropdown>((int) Dropdowns.Category).onValueChanged.AddListener(OnCategoryChanged);
            Util.FindChild<Button>(gameObject, "EnterButton", true).onClick.AddListener(OnEnterCheat);
            Util.FindChild(gameObject, "B_Close", true).BindEvent(Functions.TrueCondition, _ => ClosePopupUI());

            OnCategoryChanged(0);

            return true;
        }

        public override void WhenPopupClosed()
        {
            
        }

        private void OnEnterCheat()
        {
            var category = Get<TMP_Dropdown>((int) Dropdowns.Category).value;
            var fieldIdx = Get<TMP_Dropdown>((int) Dropdowns.Field).value;
            var fieldName = Get<TMP_Dropdown>((int) Dropdowns.Field).options[fieldIdx].text;
            var count = counts[Get<TMP_Dropdown>((int) Dropdowns.Count).value];
            switch (category)
            {
                case 0: case 1: case 2: case 3: 
                    if (category == 0) CurrencyController.I.GetReward(money[fieldIdx], count);
                    if (category == 1) CurrencyController.I.GetReward(stone[fieldIdx], count);
                    if (category == 2) CurrencyController.I.GetReward(ticket[fieldIdx], count);
                    if (category == 3) CurrencyController.I.GetReward(book[fieldIdx], count);
                    Get<TextMeshProUGUI>((int) Texts.ResultText).text = fieldName  + "을(를) " + count + "개 추가";
                    break;
                case 4:
                    CurrencyController.I.GetReward(have[fieldIdx], count);
                    Get<TextMeshProUGUI>((int) Texts.ResultText).text = fieldName + "을(를) 보유";
                    break;
                case 5:
                    CurrencyController.I.GetReward(CurrencyType.Weapon, count, weapon[fieldIdx]);
                    Get<TextMeshProUGUI>((int) Texts.ResultText).text =
                        fieldName + "을(를) " + count + "개 추가";
                    break;
                case 6:
                    CurrencyController.I.GetReward(CurrencyType.Accessory, count, accessory[fieldIdx]);
                    Get<TextMeshProUGUI>((int) Texts.ResultText).text =
                        fieldName + "을(를) " + count + "개 추가";
                    break;
                case 7:
                    CurrencyController.I.GetReward(CurrencyType.Skill, count, skill[fieldIdx]);
                    Get<TextMeshProUGUI>((int) Texts.ResultText).text =
                        fieldName + "을(를) " + count + "개 추가";
                    break;
                case 8:
                    CurrencyController.I.GetReward(CurrencyType.Pet, count, pet[fieldIdx]);
                    Get<TextMeshProUGUI>((int) Texts.ResultText).text =
                        fieldName + "을(를) " + count + "개 추가";
                    break;
                case 9:
                    if (fieldIdx == 0)
                    {
                        CurrencyController.I.GetReward(CurrencyType.Exp, count);
                        Get<TextMeshProUGUI>((int) Texts.ResultText).text =
                            "경험치 " + count + " 추가";
                    }
                    else if (fieldIdx == 1)
                    {  
                        SettingController.data.Reset();
                        Get<TextMeshProUGUI>((int) Texts.ResultText).text =
                            "모든 설정 초기화";
                    }
                    else if (fieldIdx == 2)
                    {
                        LevelController.data.MaxStage.Value = count;
                        LevelController.I.MoveStage(count + 1);
                        Get<TextMeshProUGUI>((int)Texts.ResultText).text =
                            "최대 스테이지를 " + count + "로 변경 후 도전 시작";
                    }
                    else if (fieldIdx == 3)
                    {
                        PlayFabManager.Store.ResetUserData();
                    }
                    break;
            }
        }

        private void OnCategoryChanged(int category)
        {
            var fields = Get<TMP_Dropdown>((int) Dropdowns.Field);
            fields.value = 0;
            fields.options = category switch
            {
                0 => moneyOptions,
                1 => stoneOptions,
                2 => ticketOptions,
                3 => bookOptions,
                4 => haveOptions,
                5 => weaponOptions,
                6 => accessoryOptions,
                7 => skillOptions,
                8 => petOptions,
                9 => levelOptions,
                _ => fields.options
            };
        }
        
        public override bool NeedRaycast()
        {
            return true;
        }
    }
}