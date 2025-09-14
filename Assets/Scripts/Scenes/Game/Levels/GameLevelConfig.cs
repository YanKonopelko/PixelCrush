using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameLevelConfig", menuName = "Scriptable Objects/GameLevelConfig")]
public class GameLevelConfig : ScriptableObject
{
    [SerializeField] private int _height;
    [SerializeField] private int _width;
    [SerializeField] private List<PixelData> _pixels;
    [SerializeField] private List<Vector2> _coins;
}

[Serializable]
public struct PixelData
{
    [field: SerializeField] public Vector2 pos { get; private set; }
    [field: SerializeField] public Vector3 color { get; private set; }
}