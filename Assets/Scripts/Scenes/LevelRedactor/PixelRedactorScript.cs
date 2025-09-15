using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class PixelRedactorScript : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject coinState;
    [SerializeField] private GameObject removedState;
    [SerializeField] private GameObject baseState;
    [SerializeField] private GameObject SelectedState;
    [SerializeField] private Animation SelectedStateAnim;
    [SerializeField] private Image baseImage;
    public Vector2 index;

    public Action action;

    public bool isCoin = false;

    void Start()
    {
        Deselect();
        // SwitchCoinState();
    }
    public void SetColor(Color color)
    {
        baseImage.color = color;
    }
    public void UpdateByValue(int value)
    {
        removedState.SetActive(false);
        baseState.SetActive(false);

        switch (value)
        {
            case 0:
                {
                    baseState.SetActive(true);
                    break;
                }
            case 1:
                {
                    removedState.SetActive(true);
                    break;
                }
        }
    }

    public bool SwitchCoinState()
    {
        isCoin = !isCoin;
        coinState.SetActive(isCoin);
        return isCoin;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        action.Invoke();
        Select();
    }

    public void Select()
    {
        SelectedState.SetActive(true);
        SelectedStateAnim.Play();
    }

    public void Deselect()
    {
        SelectedState.SetActive(false);
        SelectedStateAnim.Stop();
    }
}
