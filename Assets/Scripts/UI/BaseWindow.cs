using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseWindow : MonoBehaviour
{
    [SerializeField] EWindowType windowType;
    public async void Show()
    {
        gameObject.SetActive(true);
        transform.position = new Vector3(0,0,0);
    }

    public async void Hide()
    {
        gameObject.SetActive(false);
        Destroy(this.gameObject);
        GlobalData.Instance.UIManager.HideWindow(windowType);
    }
}
