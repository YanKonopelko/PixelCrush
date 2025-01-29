using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameScene : MonoBehaviour
{

    [SerializeField] private GameObject StartCanvas;

    [SerializeField] private Text debugText;
    [SerializeField] private GameObject loadScreen;

    [SerializeField] private LevelCreator levelCreator;
    [SerializeField] private BrusherRotation brusherRotation;

    public static GameScene Instance;

    public Action OnStart;
    public Action OnLose;
    public Action OnFinish;
    public bool isStart;
    public bool isLose;
    public bool IsFinish;
    private async void Start()
    {
        Instance = this;
        loadScreen.SetActive(true);
        // LevelCreator.Instance = this;
        await levelCreator.AsyncCreateLevel();
        brusherRotation.gameObject.transform.position = levelCreator.GetLevelCenter();
        loadScreen.SetActive(false);
        DOTween.SetTweensCapacity(200, 250);
        // Application.targetFrameRate = 50;
    }
    public void Update()
    {
        debugText.text = "Level: " + ((PlayerData.Instance.AdditionalIndex > -1 ? PlayerData.Instance.CurrentLevel + PlayerData.Instance.AdditionalIndex : PlayerData.Instance.CurrentLevel) + 1).ToString() + $"\n Fps: {1 / Time.deltaTime}";
    }
    public async UniTask Win()
    {
        IsFinish = true;
        PlayerData.Instance.MarkLevelComplete();
        brusherRotation.FinishAnimation(0.5f);
        await UniTask.Delay(500);
        brusherRotation.ReloadRot();
        isStart = false;
        isLose = false;
        IsFinish = false;
        PlayerData.Instance.LastLevel = -1;
        loadScreen.SetActive(true);
        await levelCreator.AsyncCreateLevel();
        StartCanvas.SetActive(true);
        brusherRotation.gameObject.transform.position = levelCreator.GetLevelCenter();
        GlobalData.Instance.UnloadLevelTexture(PlayerData.Instance.LastLevel);
        PlayerData.Instance.Save();
        await UniTask.Delay(1500);
        loadScreen.SetActive(false);
    }
    public void Restart()
    {
        isStart = false;
        isLose = false;
        IsFinish = false;
        levelCreator.CreateLevel();
        brusherRotation.gameObject.transform.position = levelCreator.GetLevelCenter();
        StartCanvas.SetActive(true);
    }

    public void StatGame()
    {
        isStart = true;
        OnStart?.Invoke();
        StartCanvas.SetActive(false);
    }
}
