using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleButton : MonoBehaviour
{
    [SerializeField] private GameObject onState;
    [SerializeField] private GameObject offState;

    [SerializeField] public bool isOn;
    public Action<bool> onSwitch;

    public void Init(bool startValue, Action<bool> onSwitchAction = null)
    {
        isOn = startValue;
        UpdateWithValue();
        if (onSwitchAction != null)
            onSwitch += onSwitchAction;
    }

    public void SwitchState()
    {
        isOn = !isOn;
        UpdateWithValue();
    }

    public void ForceSwitchState(bool targerValue)
    {
        isOn = targerValue;
        UpdateWithValue();
    }
    private void UpdateWithValue()
    {
        onState.SetActive(isOn);
        offState.SetActive(!isOn);
        onSwitch?.Invoke(isOn);
    }
}
