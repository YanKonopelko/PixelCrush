using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using InventoryNamespace;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameScene : MonoBehaviour
{

    [SerializeField] private GameObject StartCanvas;

    [SerializeField] private Text debugText;
    // [SerializeField] private GameObject loadScreen;

    [SerializeField] public LevelCreator levelCreator;
    [SerializeField] private BrusherRotation brusherRotation;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private GameObject toRedactorButton;

    public static GameScene Instance;

    public Action OnStart;
    public Action OnLose;
    public Action OnFinish;
    public bool isStart;
    public bool isLose;
    public bool IsFinish;
    private async void Awake()
    {
        Instance = this;
        toRedactorButton.SetActive(SessionData.isFromLevelCreator);
        GlobalData.Instance.MusicManager.Swap(EMusicType.BaseBackMusic); 
        BaseWindow window = GlobalData.Instance.UIManager.ShowWindow(EWindowType.LoadingWindow);
        await levelCreator.AsyncCreateLevel();
        Vector3 pos = levelCreator.GetPoses()[levelCreator.GetPoses().Length%50];
        pos.y = 0.5f;
        brusherRotation.gameObject.transform.position = pos;
        DOTween.SetTweensCapacity(200, 250);
        window.Hide();
        // Item a = await ItemFabric.GetItem(0);
        // Debug.Log(a.ID);
        // Debug.Log(a.Icon);
    }
    public void Update()
    {
        if(!GlobalData.Instance.IsRelease)
            debugText.text = "Level: " + ((PlayerData.Instance.AdditionalIndex > -1 ? PlayerData.Instance.CurrentLevel + PlayerData.Instance.AdditionalIndex : PlayerData.Instance.CurrentLevel) + 1).ToString() + $"\n Fps: {1 / Time.deltaTime}";
        else
            debugText.text = "";
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
         Vector3 pos = levelCreator.GetPoses()[levelCreator.GetPoses().Length%50];
        pos.y = 0.5f;
        brusherRotation.gameObject.transform.position = pos;
        GlobalData.Instance.UnloadLevelTexture(PlayerData.Instance.LastLevel);
        PlayerData.Instance.Save();
        await UniTask.Delay(1500);
        window.Hide();
        StartCanvas.SetActive(true);
    }
    public void Restart()
    {
        isStart = false;
        isLose = false;
        IsFinish = false;
        levelCreator.CreateLevel();
         Vector3 pos = levelCreator.GetPoses()[levelCreator.GetPoses().Length%50];
        pos.y = 0.5f;
        brusherRotation.gameObject.transform.position = pos;
        StartCanvas.SetActive(true);
    }

    public void StatGame()
    {
        isStart = true;
        OnStart?.Invoke();
        StartCanvas.SetActive(false);
        // if (!PlayerData.Instance.TutorialComplete)
        // {
        //     TutorialWindowData windowData = new TutorialWindowData();
        //     windowData.HideCallback = () => brusherRotation.ChangeDirection();
        //     windowData.Step = 0;
        //     GlobalData.Instance.UIManager.ShowWindow(EWindowType.TutorialWindow, windowData);
        // }
    }

    public void OpenSettings()
    {
        GlobalData.Instance.UIManager.ShowWindow(EWindowType.Settings);
    }
 public void ReturnToRedactor()
    {
        SceneManager.LoadScene("LevelRedactor");
        // GlobalData.Instance.UIManager.ShowWindow(EWindowType.Settings);
    }
}
