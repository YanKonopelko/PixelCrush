
using System;

public static class ShuffleUtills{
    public static void Shuffle(ref int[] array)
    {
        Random random = new Random();
        int p = array.Length;
        for (int n = p - 1; n > 0; n--)
        {
            int r = random.Next(1, n);
            int t = array[r];
            array[r] = array[n];
            array[n] = t;
        }
    }
}