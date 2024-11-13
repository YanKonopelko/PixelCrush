using System;
using System.Drawing;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public struct Square{
    public Vector2 LU;
    public Vector2 RU;
    public Vector2 LD;
    public Vector2 RD;
    // a     b
    // c     d
    public  Square(Vector2 lu,Vector2 ru,Vector2 ld,Vector2 rd){
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
    // public float angle;


    public void Execute(int index)
    {
        if(outPixelPainted[index]) return;
        Vector2 capsulePos = pixelsPositions[index];

        
        if(PointInQuadrilateral.IsPointInside(new PointInQuadrilateral.Point(square.LD.x,square.LD.y),new PointInQuadrilateral.Point(square.LU.x,square.LU.y),new PointInQuadrilateral.Point(square.RU.x,square.RU.y),new PointInQuadrilateral.Point(square.RD.x,square.RD.y), new PointInQuadrilateral.Point(capsulePos.x,capsulePos.y))){
            outPixelPainted[index] = true;
            // Debug.Log("Pos" + index.ToString());
            // Debug.Log(capsulePos);
            // Debug.Log(square.LD);
            // Debug.Log(square.LU);
            // Debug.Log(square.RU);
            // Debug.Log(square.RD);
        } 
        // // точка (xp, yp) слева от края (x1, y1) - (x2, y2)     
        // float x1,x2,y1,y2,d;
        // float xp = capsulePos.x;   
        // float yp = capsulePos.y;   


        // x1 = square.RU.x;
        // y1 = square.RU.y;

        // x2 = square.LU.x;
        // y2 = square.LU.y;

        // d = (x2 - x1) * (yp - y1) - (xp - x1) * (y2 - y1);
        // // Debug.Log("0");
        // if(d < 0) return;

        // x1 = square.LU.x;
        // y1 = square.LU.y;

        // x2 = square.LD.x;
        // y2 = square.LD.y;

        // d = (x2 - x1) * (yp - y1) - (xp - x1) * (y2 - y1);
        // // Debug.Log("1");

        // if(d < 0) return;

        // x1 = square.LD.x;
        // y1 = square.LD.y;

        // x2 = square.RD.x;
        // y2 = square.RD.y;

        // d = (x2 - x1) * (yp - y1) - (xp - x1) * (y2 - y1);
        // // Debug.Log("2");

        // if(d < 0) return;

        // x1 = square.RD.x;
        // y1 = square.RD.y;

        // x2 = square.RU.x;
        // y2 = square.RU.y;

        // d = (x2 - x1) * (yp - y1) - (xp - x1) * (y2 - y1);
        // // Debug.Log("3");
        // if(d < 0) return;
        // // Debug.Log("4");

        // outPixelPainted[index] = true;
        // if(capsulePos.x <= square.LU)

        // if (Mathf.Abs(capsulePos.x - brusherPosition.x) < brusherSize.y * Math.Cos(angle) + brusherSize.x * Math.Sin(angle) &&
        //      Mathf.Abs(capsulePos.y - brusherPosition.y) < brusherSize.y * Math.Sin(angle) + brusherSize.x * Math.Cos(angle))
        // {
        //         Debug.Log(brusherSize.y * Math.Cos(angle) + brusherSize.x * Math.Sin(angle)) ; 
        //     outPixelPainted[index] = true;
        // }
    }

}

public class PointInQuadrilateral
{
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

    // Метод для проверки, находится ли точка внутри четырёхугольника
    public static bool IsPointInside(Point p1, Point p2, Point p3, Point p4, Point testPoint)
    {
        // Проверка знаков векторных произведений для каждой из сторон
        double cross1 = CrossProduct(p1, p2, testPoint);
        double cross2 = CrossProduct(p2, p3, testPoint);
        double cross3 = CrossProduct(p3, p4, testPoint);
        double cross4 = CrossProduct(p4, p1, testPoint);

        // Если все произведения имеют одинаковые знаки (все положительные или все отрицательные),
        // то точка внутри четырёхугольника
        bool isPositive = cross1 > 0 && cross2 > 0 && cross3 > 0 && cross4 > 0;
        bool isNegative = cross1 < 0 && cross2 < 0 && cross3 < 0 && cross4 < 0;

        return isPositive || isNegative;
    }

    public static void Main()
    {
        // Вершины четырёхугольника
        Point p1 = new Point(0, 0);
        Point p2 = new Point(4, 0);
        Point p3 = new Point(4, 4);
        Point p4 = new Point(0, 4);

        // Точка для проверки
        Point testPoint = new Point(2, 2);

        // Проверка, находится ли точка внутри четырёхугольника
        bool result = IsPointInside(p1, p2, p3, p4, testPoint);
        Console.WriteLine("Точка внутри четырёхугольника: " + result);
    }
}
