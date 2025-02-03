using YG;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public enum MusicType
{
    MainMenuMusic,
    Level_0,
    Level_1,
    Level_2,
    Level_3,
    NULL
}
public class MusicManager : MonoBehaviour
{
    [SerializeField] private CustomArrayWithEnum<MusicType, AudioClip>[] clips;
    private Dictionary<MusicType, AudioSource> sources = new Dictionary<MusicType, AudioSource>();

    public float volume { get; private set; }

    private MusicType curMusic;

    private bool musicEnabled = true;

    public bool MusicEnabled
    {
        get { return musicEnabled; }
        set
        {
            musicEnabled = value; YG2.saves.musicEnabled = musicEnabled;
            ChangeVolume(musicEnabled ? 1 : 0);
        }
    }

    async public void Init()
    {
        // YandexGame.GetDataEvent
        // while(!YandexGame.SDKEnabled){
        //     await UniTask.Delay(3);
        // }

        // volume = YandexGame.savesData.MusicVolume;
        musicEnabled = YG2.saves.musicEnabled;
        curMusic = MusicType.NULL;
        for (int i = 0; i < clips.Length; i++)
        {
            var Obj = clips[i];
            sources[Obj.key] = gameObject.AddComponent<AudioSource>();
            sources[Obj.key].volume = volume;
            sources[Obj.key].clip = Obj.value;
            sources[Obj.key].loop = true;
            sources[Obj.key].playOnAwake = false;
        }

    }

    public void ChangeVolume(float _volume)
    {
        volume = _volume;
        var keys = sources.Keys;
        foreach (MusicType type in keys)
        {
            sources[type].volume = volume;
        }
        // YandexGame.savesData.MusicVolume = volume;
    }

    async public void Swap(MusicType type, float time = 1.5f)
    {
        if (type == curMusic) return;
        sources[type].Play();

        if (curMusic != MusicType.NULL)
        {
            float curTime = 0;
            while (curTime < time)
            {
                sources[curMusic].volume = Mathf.Lerp(volume, 0, curTime / time);
                sources[type].volume = Mathf.Lerp(0, volume, curTime / time);
                curTime += Time.fixedUnscaledDeltaTime;
                await UniTask.Delay(1);
            }
            sources[curMusic].Stop();
        }
        curMusic = type;
        // sources[curMusic].volume = 1;
    }

}