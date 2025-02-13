using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YG;

public enum ESoundType
{
    PixelDisapearSound = 0
};


public class SoundManager : MonoBehaviour
{

    [SerializeField] private CustomArrayWithEnum<ESoundType, AudioClip>[] clips;
    private Dictionary<ESoundType, AudioClip> sourcesClips = new Dictionary<ESoundType, AudioClip>();

    private Dictionary<ESoundType, List<AudioSource>> sources = new Dictionary<ESoundType, List<AudioSource>>();

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
        DontDestroyOnLoad(this.gameObject);

        for (int i = 0; i < clips.Length; i++)
        {
            var Obj = clips[i];
            sourcesClips[Obj.key] = Obj.value;
            sources[Obj.key] = new List<AudioSource>();
            // sources[Obj.key].Add(CreateSource());
        }
    }

    public void ChangeVolume(float newVolume)
    {
        volume = newVolume;

        var keys = sources.Keys;
        foreach (ESoundType type in keys)
        {
            for (int i = 0; i < sources[type].Count; i++)
            {
                sources[type][i].volume = volume;
            }
        }

        // YandexGame.savesData.SoundsVolume = volume;
    }

    public void PlaySound(ESoundType type)
    {
        if(!soundsEnabled) return;
        List<AudioSource> array = sources[type];
        for (int i = 0; i < array.Count; i++)
        {
            if (!array[i].isPlaying)
            {
                array[i].Play();
                return;
            }
        }
        if(array.Count>=2) return;
        var source = CreateSource(sourcesClips[type]);
        array.Add(source);
    }

    private AudioSource CreateSource(AudioClip clip)
    {
        
        AudioSource source = this.gameObject.AddComponent<AudioSource>();
        source.volume = volume;
        source.clip = clip;
        source.loop = false;
        source.playOnAwake = false;
        return source;
    }
}