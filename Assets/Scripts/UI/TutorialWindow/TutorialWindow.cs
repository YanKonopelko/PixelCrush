using Cysharp.Threading.Tasks;
using UnityEngine;
using YG;

public class TutorialWindow : BaseWindowWithData<TutorialWindowData>
{
    [SerializeField] GameObject fingerObject;

    [SerializeField] GameObject Step0;
    [SerializeField] GameObject Step1;
    private bool isInteractable = false;
    private int awaitTime = 3500;

    override public async void Show()
    {
        if (Data.Step == 0)
        {
            Step0.SetActive(true);
            Step1.SetActive(false);
        }
        else
        {
            Step0.SetActive(false);
            Step1.SetActive(true);
        }
        fingerObject.SetActive(false);
        base.Show();
        await UniTask.Delay(awaitTime);
        fingerObject.SetActive(true);
        isInteractable = true;
    }

    override public async void Hide()
    {
        if (!isInteractable) return;
        base.Hide();
        if (Data?.HideCallback != null)
        {
            Data.HideCallback();
        }
    }

}
