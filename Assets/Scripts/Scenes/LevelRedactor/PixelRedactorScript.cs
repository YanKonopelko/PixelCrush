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

    void Start()
    {
        Deselect();
    }
    public void SetColor(Color color)
    {
        baseImage.color = color;
    }
    public void UpdateByValue(int value)
    {
        coinState.SetActive(false);
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
                    coinState.SetActive(true);
                    break;
                }
            case 2:
                {
                    removedState.SetActive(true);
                    break;
                }
        }
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
