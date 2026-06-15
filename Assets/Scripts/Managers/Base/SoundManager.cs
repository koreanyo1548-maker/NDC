using System;
using System.Collections.Generic;
using Controller;
using Controller.Play;
using Data.DbUser;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using MEC;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

namespace Managers.Base
{
    public enum BGMType
    {
        Title = 0,
        Field = 1,
        Prologue = 2,
        Boss = 3,
        Dungeon1 = 4,
        Dungeon2 = 5,
        Dungeon3 = 6,
        None = 7
    }
    
    public enum SFXType
    {
        Skill1 = 0,
        Skill2 = 1,
        Skill3 = 2,
        Skill6 = 3,
        Skill7 = 4,
        Skill9 = 5,
        Attack = 6,
        Attacked = 7,
        Reward = 8,
        Skill4 = 9,
        Skill5 = 10,
        Skill8 = 11,
        Skill10 = 12,
        Skill11 = 13,
        Skill12 = 14,
        Skill13 = 15,
        Skill14 = 16,
        Skill15 = 17,
        Skill16 = 18,
        Skill17 = 19,
        Skill18 = 20,
        Skill19 = 21,
        Skill20 = 22,
        Skill21 = 23,
        Skill13_2 = 24,
        Skill21_2 = 25,
        Boss = 26,
        Crow_voice3 = 27,
        EnterStory = 28,
        Failed = 29,
        Mon_Frog = 30,
        Mon_Mushroom = 31,
        Mon_Pig = 32,
        Mon_Radish = 33,
        Mon_Slime = 34,
        StoryChat = 35,
        SummonItem = 36,
        SummonItem_Special = 37,
        TouchToStart = 38,
        UI_Button = 39,
        UI_Close = 40,
        UI_LevelUp = 41,
        UI_MainQuest = 42,
        UI_Reward = 43,
        UI_Tab = 44,
        UI_Upgrade = 45,
        StoryAnnaChat = 46,
        Prologue_FilmFast = 47,
        Prologue_FilmSlow = 48,
        
        None = 100
    }
    
    public class SoundManager: MonoBehaviour
    {
        private List<AudioClip> _bgSounds = new ();
        private List<AudioClip> _effectSounds = new ();

        private AudioSource _bgPlayer;
        private List<AudioSource> _effectPlayers = new();
        private Dictionary<SFXType, int> _sfxCount = new();

        private BGMType _curBgm = BGMType.None;

        private bool _needSFX = true;

        private static float _bgVolumeScale = 0.66f;
        private static float _sfxVolumeScale = 0.35f;
        private float _bgVolume = _bgVolumeScale;
        private float _sfxVolume =_sfxVolumeScale;
        
        private bool _isInit;

        // private string _effect = "sfx";
        // void OnGUI()
        // {
        //     Rect position = new Rect(50, 100, Screen.width, Screen.height);
        //
        //     GUIStyle style = new GUIStyle();
        //
        //     style.fontSize = 30;
        //     style.normal.textColor = Color.white;
        //     style.fontStyle = FontStyle.Bold;
        //
        //     GUI.Label(position, _effect, style);   
        // }
        
        private void Start()
        {
            Init();
        }
        
        private void Init()
        {
            DontDestroyOnLoad(gameObject);
            _bgPlayer = gameObject.AddComponent<AudioSource>();


            _bgSounds = new List<AudioClip>();
            for (var idx = 0; idx < (int)BGMType.None; ++idx)
            {
                _bgSounds.Add(Resources.Load<AudioClip>("Sounds/BGM_" + (BGMType)idx));
            }
            _effectSounds = new List<AudioClip>();
            for (var idx = 0; idx < (int)SFXType.None; ++idx)
            {
                _effectSounds.Add(Resources.Load<AudioClip>("Sounds/SFX_" + (SFXType)idx));
            }
            
            AudioSettings.OnAudioConfigurationChanged += _ => MuteSound(false);

            SettingController.data.BGMSound.ValueChanged += (_, _) => SetBGMSound();
            SettingController.data.SfxSound.ValueChanged += (_, _) => SetSFXSound();
            
            SetBGMSound();
            SetSFXSound();
            PlayBGM(BGMType.Title);
            _isInit = true;
        }


        private TweenerCore<float, float, FloatOptions> _tween1;
        private TweenerCore<float, float, FloatOptions> _tween2;
        public void PlayBGM(BGMType sound,  bool isForce = false)
        {
            if (_curBgm == sound && !isForce) return;
            _curBgm = sound;
            
            _tween1?.Kill();
            _tween2?.Kill();
            _tween1 = DOTween.To(() => _bgPlayer.volume, x => _bgPlayer.volume = x, 0, 0.7f).SetEase(Ease.Linear)
                .OnComplete(CrescendoPlay);

            void CrescendoPlay()
            {
                Play(_bgPlayer, _bgSounds[(int)sound], true, 0);
                _tween2 = DOTween.To(() => _bgPlayer.volume, x => _bgPlayer.volume = x, _bgVolume, 0.7f).SetEase(Ease.Linear);
            }
        }
        
        public void PlaySFX(SFXType sound, bool isLoop = false)
        {
            if (!_needSFX) return;
            if (sound == SFXType.None) return;
            if (sound != SFXType.SummonItem && sound != SFXType.SummonItem_Special)
            {
                if (_sfxCount.ContainsKey(sound))
                {
                    if (_sfxCount[sound] >= 3) return;
                    _sfxCount[sound]++;
                }
                else
                {
                    _sfxCount.Add(sound, 1);
                }
                if (!isLoop) Timing.CallDelayed(_effectSounds[(int)sound].length, () => _sfxCount[sound]--);
            }
            
            // if (!ignore.Contains(sound))
            // {
            //     var splits = _effect.Split("\n");
            //     if (splits.Length > 3) _effect = splits[1] + "\n" + splits[2] + "\n" + splits[3];
            //     _effect += "\n" +sound;
            // }
            
            int childCount = _effectPlayers.Count;
            int playIdx = -1;
            for (int idx = 0; idx < childCount; ++idx)
            {
                if (!_effectPlayers[idx].isPlaying)
                {
                    playIdx = idx;
                    break;
                }
            }

            if (playIdx == -1) playIdx = CreateNewEffectPlayer();

            Play(_effectPlayers[playIdx], _effectSounds[(int)sound], isLoop, 0);
        }

        public void StopSFX(SFXType sound)
        {
            var childCount = _effectPlayers.Count;
            for (int idx = 0; idx < childCount; ++idx)
            {
                if (_effectPlayers[idx].isPlaying && _effectPlayers[idx].clip == _effectSounds[(int)sound])
                {
                    _effectPlayers[idx].Stop();
                    _sfxCount[sound]--;
                }
            }
        }
        
        private int CreateNewEffectPlayer()
        {
            var obj = new GameObject();
            obj.transform.SetParent(transform);
            var newPlayer = obj.AddComponent<AudioSource>();
            newPlayer.volume = _sfxVolume;
            _effectPlayers.Add(newPlayer);
            return _effectPlayers.Count - 1;
        }

        private void Play(AudioSource source, AudioClip clip, bool isLoop, float time)
        {
            source.Stop();
            source.clip = clip;
            source.loop = isLoop;
            source.time = time;
            source.Play();
        }

        private void SetBGMSound()
        {
            _bgVolume = SettingController.data.BGMSound.Value * _bgVolumeScale;
            _bgPlayer.volume = _bgVolume;
        }

        private void SetSFXSound()
        {
            _sfxVolume = SettingController.data.SfxSound.Value * _sfxVolumeScale;
            _needSFX = _sfxVolume > 0;
            for (var idx = 0; idx < _effectPlayers.Count; ++idx)
                _effectPlayers[idx].volume = _sfxVolume;
        }

        public void MuteSound(bool isMute)
        {
            if (isMute)
            {
                _bgPlayer.volume = 0;
                _needSFX = false;
                for (var idx = 0; idx < _effectPlayers.Count; ++idx)
                    _effectPlayers[idx].volume = 0;
            }
            else
            {
                // _bgPlayer.enabled = true;
                // foreach (var effectPlayer in _effectPlayers) effectPlayer.enabled = true;
                // _bgPlayer.UnPause();
                // foreach (var effectPlayer in _effectPlayers) effectPlayer.UnPause();
                SetBGMSound();
                SetSFXSound();
                PlayBGM(_curBgm, true);
            }
        }

        private void OnApplicationPause(bool isPause)
        {
            if (!_isInit) return;
            MuteSound(isPause);
        }
    }
}