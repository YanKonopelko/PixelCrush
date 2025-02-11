using YG;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public enum EMusicType
{
    NULL = 0,
    BaseBackMusic = 1,
}
public class MusicManager : MonoBehaviour
{
    [SerializeField] private CustomArrayWithEnum<EMusicType, AudioClip>[] clips;
    private Dictionary<EMusicType, AudioSource> sources = new Dictionary<EMusicType, AudioSource>();

    [SerializeField] private float volume;

    private EMusicType curMusic;

    private bool musicEnabled = true;
    private float lastVolumeValue = 1;

    public bool MusicEnabled
    {
        get { return musicEnabled; }
        set
        {
            musicEnabled = value; YG2.saves.musicEnabled = musicEnabled;
            ChangeVolume(musicEnabled ? lastVolumeValue : 0);
        }
    }

    async public void Init()
    {
        musicEnabled = YG2.saves.musicEnabled;
        lastVolumeValue = volume;
        if (!musicEnabled)
        {
            volume = 0;
        }
        curMusic = EMusicType.NULL;
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
        if (_volume != 0)
        {
            lastVolumeValue = _volume;
        }
        volume = _volume;
        var keys = sources.Keys;
        foreach (EMusicType type in keys)
        {
            sources[type].volume = volume;
        }
        // YandexGame.savesData.MusicVolume = volume;
    }

    async public void Swap(EMusicType type, float time = 1.5f)
    {
        if (type == curMusic) return;
        sources[type].Play();

        if (curMusic != EMusicType.NULL)
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