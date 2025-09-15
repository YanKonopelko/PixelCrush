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
    [SerializeField] private float _brusherSpeed;
    [SerializeField] private Vector2 _brusherStartPosition;

    public int Height => _height;
    public int Width => _width;
    public List<PixelData> Pixels => _pixels;
    public List<Vector2> Coins => _coins;
    public float BrusherSpeed => _brusherSpeed;
    public Vector2 BrusherStartPosition => _brusherStartPosition;

    public GameLevelConfig(int height, int width, List<PixelData> pixels, List<Vector2> coins, float brusherSpeed, Vector2 brusherStartPosition)
    {
        _height = height;
        _width = width;
        _pixels = pixels;
        _coins = coins;
        _brusherSpeed = brusherSpeed;
        _brusherStartPosition = brusherStartPosition;
    }
}

[Serializable]
public struct PixelData
{
    [field: SerializeField] public Vector2 _pos { get; private set; }
    [field: SerializeField] public string _colorHex { get; private set; }

    public PixelData(Vector2 pos, string color)
    {
        _pos = pos;
        _colorHex = color;
    }
}