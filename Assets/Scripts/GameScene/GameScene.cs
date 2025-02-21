using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using InventoryNamespace;
using UnityEngine;
using UnityEngine.UI;

public class GameScene : MonoBehaviour
{

    [SerializeField] private GameObject StartCanvas;

    [SerializeField] private Text debugText;
    // [SerializeField] private GameObject loadScreen;

    [SerializeField] private LevelCreator levelCreator;
    [SerializeField] private BrusherRotation brusherRotation;
    [SerializeField] private CameraController cameraController;

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
        GlobalData.Instance.MusicManager.Swap(EMusicType.BaseBackMusic); 
        BaseWindow window = GlobalData.Instance.UIManager.ShowWindow(EWindowType.LoadingWindow);
        await levelCreator.AsyncCreateLevel();
        brusherRotation.gameObject.transform.position = levelCreator.GetLevelCenter();
        DOTween.SetTweensCapacity(200, 250);
        window.Hide();
        Item a = ItemFabric.GetItem(0);
        await UniTask.Delay(2);
        Debug.Log(a.ID);
        Debug.Log(a.Icon);
    }
    public void Update()
    {
        debugText.text = "Level: " + ((PlayerData.Instance.AdditionalIndex > -1 ? PlayerData.Instance.CurrentLevel + PlayerData.Instance.AdditionalIndex : PlayerData.Instance.CurrentLevel) + 1).ToString() + $"\n Fps: {1 / Time.deltaTime}";
    }
    public async UniTask Win()
    {
        if (!PlayerData.Instance.TutorialComplete)
        {
            PlayerData.Instance.MarkTutorialComplete();
        }
        IsFinish = true;
        PlayerData.Instance.MarkLevelComplete();
        await brusherRotation.FinishAnimation(0.5f);
        await cameraController.FinishAnim();
        brusherRotation.ReloadRot();
        isStart = false;
        isLose = false;
        IsFinish = false;
        PlayerData.Instance.LastLevel = -1;
        BaseWindow window = GlobalData.Instance.UIManager.ShowWindow(EWindowType.LoadingWindow);
        await levelCreator.AsyncCreateLevel();
        cameraController.ToDefaultValues();
        StartCanvas.SetActive(true);
        brusherRotation.gameObject.transform.position = levelCreator.GetLevelCenter();
        GlobalData.Instance.UnloadLevelTexture(PlayerData.Instance.LastLevel);
        PlayerData.Instance.Save();
        await UniTask.Delay(1500);
        window.Hide();
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
        if (!PlayerData.Instance.TutorialComplete)
        {
            TutorialWindowData windowData = new TutorialWindowData();
            windowData.HideCallback = () => brusherRotation.ChangeDirection();
            windowData.Step = 0;
            GlobalData.Instance.UIManager.ShowWindow(EWindowType.TutorialWindow, windowData);
        }
    }

    public void OpenSettings()
    {
        GlobalData.Instance.UIManager.ShowWindow(EWindowType.Settings);
    }

}
