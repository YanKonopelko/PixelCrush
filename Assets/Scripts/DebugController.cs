using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YG;

public class DebugController : MonoBehaviour
{
    [SerializeField] GameObject debugButton;
    [SerializeField] GameObject cheatRoots;
    // Start is called before the first frame update
    void Start()
    {
        debugButton.SetActive(!GlobalData.Instance.IsRelease);
        cheatRoots.SetActive(false);
    }
    public void OpenCheats(){
        cheatRoots.SetActive(true);
    }
    public void CloseCheats(){
        cheatRoots.SetActive(false);
    }
    public void ResetSave(){
        YG2.SetDefaultSaves();
        YG2.SaveProgress();
    }
}
