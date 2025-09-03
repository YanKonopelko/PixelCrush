using System;
using System.Drawing;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public struct Square{
    public PointInQuadrilateral.Point LU;
    public PointInQuadrilateral.Point RU;
    public PointInQuadrilateral.Point LD;
    public PointInQuadrilateral.Point RD;
    // a     b
    // c     d
    public  Square(PointInQuadrilateral.Point lu, PointInQuadrilateral.Point ru, PointInQuadrilateral.Point ld, PointInQuadrilateral.Point rd){
        LU = lu;
        RU = ru;
        LD = ld;
        RD = rd;
    }
}

public struct TriggerPixelJob : IJobParallelFor
{

    public NativeArray<Vector2> pixelsPositions;
    public NativeArray<bool> outPixelPainted;

    public Vector2 brusherPosition;
    public Vector2 brusherSize;
    public Vector2 pixelSize;
    public Square square;
    public PointInQuadrilateral.Point circle1;
    public float radius;
    public PointInQuadrilateral.Point circle2;
    // public float angle;

    public void Execute(int index)
    {
        // Ранний выход если пиксель уже покрашен
        if(outPixelPainted[index]) return;
        
        PointInQuadrilateral.Point capsulePos = new PointInQuadrilateral.Point(pixelsPositions[index].x, pixelsPositions[index].y);

        // Проверяем квадрат первым (обычно быстрее)
        if(PointInQuadrilateral.IsPointInside(square.LD,square.LU,square.RU,square.RD, capsulePos)){
            outPixelPainted[index] = true;
            return; // Ранний выход после успешной проверки
        }
        
        // Проверяем круги только если не попали в квадрат
        if (PointInQuadrilateral.IsPointInsideCircle(circle1, radius, capsulePos))
        {
            outPixelPainted[index] = true;
            return;
        }
        
        if (PointInQuadrilateral.IsPointInsideCircle(circle2, radius, capsulePos))
        {
            outPixelPainted[index] = true;
        }
    }
}

public class PointInQuadrilateral
{
    public static bool IsPointInsideCircle(Point circleCentre, float radius, Point point)
    {
        // Оптимизированная версия - используем квадрат расстояния для избежания извлечения корня
        double dx = point.X - circleCentre.X;
        double dy = point.Y - circleCentre.Y;
        double distanceSquared = dx * dx + dy * dy;
        double radiusSquared = radius * radius;

        return distanceSquared <= radiusSquared;
    }
    
    // Структура для представления точки
    public struct Point
    {
        public double X;
        public double Y;

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    // Метод для вычисления векторного произведения двух векторов
    private static double CrossProduct(Point p1, Point p2, Point p3)
    {
        return (p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X);
    }

    // Оптимизированный метод для проверки, находится ли точка внутри четырёхугольника
    public static bool IsPointInside(Point p1, Point p2, Point p3, Point p4, Point testPoint)
    {
        // Вычисляем все векторные произведения за один проход
        double cross1 = CrossProduct(p1, p2, testPoint);
        double cross2 = CrossProduct(p2, p3, testPoint);
        double cross3 = CrossProduct(p3, p4, testPoint);
        double cross4 = CrossProduct(p4, p1, testPoint);

        // Оптимизированная проверка знаков - используем битовые операции
        bool allPositive = cross1 > 0 && cross2 > 0 && cross3 > 0 && cross4 > 0;
        bool allNegative = cross1 < 0 && cross2 < 0 && cross3 < 0 && cross4 < 0;

        return allPositive || allNegative;
    }
}
