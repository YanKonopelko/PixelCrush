using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseWindowWithData<T>:BaseWindow where T : BaseWindowData
{
    public T  Data;
    public override void PrepareWindowData<TData>(TData data)
    {
        if (data is T typedData)
        {
            Data = typedData;
        }
    }
}
