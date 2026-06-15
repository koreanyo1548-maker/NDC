using Fight.Units;
using Managers.Base;
using Managers.Game;
using Newtonsoft.Json;
using ThirdParty;
using TMPro;
using UnityEngine;
using Utils;
using CodeStage.AntiCheat.ObscuredTypes.Converters;

namespace Managers
{
    public class Manager: MonoBehaviour
    {
        private static Manager _instance;

        public static Manager Instance
        {
            get { Init(); return _instance; }
        }

        private AdManager _ad;
        private InputManager _input = new();
        private ResourceManager _resource = new();
        private UIManager _ui = new();
        private SoundManager _sound;
        private PoolManager _pool = new();
        private FieldManager _field = new();
        private SkillManager _skill = new();
        private GuideManager _guide = new();
        private UpdateManager _updates;
        private BackgroundManager _background;
        private DayDiffManager _dayDiff;
        private TimeManager _time = new();
        private Transform _effectParent;
        private TMP_FontAsset _font;
        private Material _thinFont;
        private Material _outlineFont;
        private Material _shadowFont;
        private Material _normalFont;


        public static TMP_FontAsset Font=> Instance._font;
        public static Material ThinFont=> Instance._thinFont;
        public static Material OutlineFont=> Instance._outlineFont;
        public static Material ShadowFont=> Instance._shadowFont;
        public static Material NormalFont=> Instance._normalFont;
        public static Player Player;
        public static Bible Bible;
        public static AdManager Ad => Instance._ad;
        public static InputManager Input => Instance._input;
        public static ResourceManager Resource => Instance._resource;
        public static UIManager UI => Instance._ui;
        public static SoundManager Sound  => Instance._sound;
        public static TimeManager Time => Instance._time;
        public static PoolManager Pool  => Instance._pool;
        public static FieldManager Field  => Instance._field;
        public static SkillManager Skill  => Instance._skill;
        public static GuideManager Guide  => Instance._guide;
        public static UpdateManager Updates  => Instance._updates;
        public static BackgroundManager Background => Instance._background;
        public static DayDiffManager DayDiff => Instance._dayDiff;
        public static Transform EffectParent => Instance._effectParent;
        
        void Start()
        {
            Init();
        }

        void Update()
        {
            _input.OnUpdate();
        }

        static void Init()

        {
            if (_instance == null)
            { 
                var obj = GameObject.Find("@Managers");
                if (obj == null)
                {
                    obj = new GameObject {name = "@Managers"};
                    obj.AddComponent<Manager>();
                }

                DontDestroyOnLoad(obj);
                _instance = obj.GetComponent<Manager>();
                
                _instance._updates = obj.GetOrAddComponent<UpdateManager>();
                _instance._background = obj.GetOrAddComponent<BackgroundManager>();
                _instance._dayDiff = obj.GetOrAddComponent<DayDiffManager>();
                _instance._sound = GameObject.Find("@Sound").GetComponent<SoundManager>();
                _instance._pool.Init();
                _instance._effectParent = new GameObject {name = "@Effects"}.transform;
                DontDestroyOnLoad(_instance._effectParent);
                
                _instance._ad = obj.AddComponent<AdManager>();
                // _instance._normalFont = Resources.Load<Material>("Fonts/Normal_" + SettingController.data.Language.Value);
                // _instance._outlineFont = Resources.Load<Material>("Fonts/Outline_" + SettingController.data.Language.Value);
                // _instance._shadowFont = Resources.Load<Material>("Fonts/Shadow_" + SettingController.data.Language.Value);
                // _instance._thinFont = Resources.Load<Material>("Fonts/ThinOutline_" + SettingController.data.Language.Value);
                // _instance._font = Resources.Load<TMP_FontAsset>("Fonts/Font_" + SettingController.data.Language.Value);

                Define.Set();
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    Converters = { new ObscuredTypesNewtonsoftConverter() }
                };
            }
        }
    }
}