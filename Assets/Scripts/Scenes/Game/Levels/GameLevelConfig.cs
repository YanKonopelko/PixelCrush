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

    [Header("Mechanics")]
    [SerializeField] private List<SpikeData> _spikes = new List<SpikeData>();
    [SerializeField] private List<ElectricBarrierData> _electricBarriers = new List<ElectricBarrierData>();
    [SerializeField] private List<MovingObstacleData> _movingObstacles = new List<MovingObstacleData>();

    public int Height => _height;
    public int Width => _width;
    public List<PixelData> Pixels => _pixels;
    public List<Vector2> Coins => _coins;
    public float BrusherSpeed => _brusherSpeed;
    public Vector2 BrusherStartPosition => _brusherStartPosition;
    
    public List<SpikeData> Spikes => _spikes;
    public List<ElectricBarrierData> ElectricBarriers => _electricBarriers;
    public List<MovingObstacleData> MovingObstacles => _movingObstacles;

    public GameLevelConfig(int height, int width, List<PixelData> pixels, List<Vector2> coins, float brusherSpeed, Vector2 brusherStartPosition)
    {
        _height = height;
        _width = width;
        _pixels = pixels;
        _coins = coins;
        _brusherSpeed = brusherSpeed;
        _brusherStartPosition = brusherStartPosition;
        _spikes = new List<SpikeData>();
        _electricBarriers = new List<ElectricBarrierData>();
        _movingObstacles = new List<MovingObstacleData>();
    }

    public void UpdateMechanics(List<SpikeData> spikes, List<ElectricBarrierData> barriers, List<MovingObstacleData> movingObstacles)
    {
        _spikes = spikes;
        _electricBarriers = barriers;
        _movingObstacles = movingObstacles;
    }
}

[Serializable]
public struct PixelData
{
    [SerializeField] public Vector2 _pos;
    [SerializeField] public string _colorHex;

    public PixelData(Vector2 pos, string color)
    {
        _pos = pos;
        _colorHex = color;
    }
}

[Serializable]
public struct SpikeData
{
    public Vector2 position;
    public float maxHeight;
    public float timeToGrow;
    public float timeToLower;
}

[Serializable]
public struct ElectricBarrierData
{
    public Vector2 startPosition;
    public Vector2 endPosition; // Determines the length and orientation
    public float activeTime;
    public float inactiveTime;
}

public enum MovingObstacleAxis
{
    X,
    Y // or Z in Unity's 3d space, we'll map Y to Z if it makes sense.
}

[Serializable]
public struct MovingObstacleData
{
    public Vector2 startPosition;
    public float length; // Length of the obstacle
    public float speed;
    public MovingObstacleAxis axis;
    public float movementRange; // How far it moves before turning back
}