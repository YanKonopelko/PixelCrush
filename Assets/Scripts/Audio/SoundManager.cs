using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YG;

public enum SoundType
{
    ButtonSound = 0,
    SliderSound,
    WinSound,
    LoseSound,
    WolfSound,
    ZombieSound,
    SkeletonSound,
    CreeperSound,
    IronGolemSound,
    SteveSound,

    ChickenSound,
    CatSound,
    AxolotlSound,
    CowSound,
    LamaSound,
    DolphinSound,
    PandaSound,

    BeeSound,
    BatSound,
    SlugSound,
    SpiderSound,
    SnowGolemSound,
    CamelSound,
    ResidentSound,
    TraderSound,

    SilverfishSound,
    PiglinSound,
    IfritSound,
    WitchSound,
    PhantomSound,
    OutlawSound,
    LavaCubeSound,
    GastSound,
    EdgeWandererSound,
    DesiccantSound,
    DragonSound,
    SpawnSound

};


public class SoundManager : MonoBehaviour
{

    [SerializeField] private CustomArrayWithEnum<SoundType, AudioClip>[] clips;
    private Dictionary<SoundType, AudioSource> sources = new Dictionary<SoundType, AudioSource>();

    public float volume = 0.4f;

    private bool soundsEnabled = true;

    public bool SoundsEnable
    {
        get { return soundsEnabled; }
        set
        {
            soundsEnabled = value; YG2.saves.soundsEnabled = soundsEnabled;
            ChangeVolume(soundsEnabled ? 1 : 0);
        }
    }
    async public void Init()
    {
        // while(!YG2.GetState()){
        //     await UniTask.Delay(3);
        // }
        // volume = YG2.saves.soundsEnable;
        soundsEnabled = YG2.saves.soundsEnabled;

        for (int i = 0; i < clips.Length; i++)
        {
            var Obj = clips[i];
            sources[Obj.key] = gameObject.AddComponent<AudioSource>();
            sources[Obj.key].volume = volume;
            // sources[Obj.key].clip = Obj.Value;
            sources[Obj.key].loop = false;
            sources[Obj.key].playOnAwake = false;
        }
    }

    public void ChangeVolume(float newVolume)
    {
        volume = newVolume;

        var keys = sources.Keys;
        foreach (SoundType type in keys)
        {
            sources[type].volume = volume;
        }

        // YandexGame.savesData.SoundsVolume = volume;
    }

    public void PlaySound(SoundType type)
    {
        sources[type].Play();
    }

}