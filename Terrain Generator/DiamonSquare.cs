using UnityEngine;
using System.Collections;
using System;

//class that computes height values using the diamondSquareAlgorithm
public static class DiamondSquare
{
    static float[,] heightMap;
    static int size_x;
    static int size_z;
    static bool ignoreNorth = false, ignoreSouth = false, ignoreEast = false, ignoreWest = false;
    static float h;
    static bool computed = false;

    public static void generateHeightMap(float[,] seededHeights, int sizeX, int sizeZ, int featureSize, float randomDisplacement)
    {
       // if (computed)
         //   return;

        //assign class parameters
        heightMap = seededHeights;
        size_x = sizeX;
        size_z = sizeZ;
        h = randomDisplacement;

        //assign 
        float scale = 1f;
        int sampleSize = featureSize;

        while (sampleSize > 1)
        {
            diamondSquareAlg(sampleSize, scale);

            sampleSize /= 2;
            scale /= 2;
        }

        computed = true;
    }

    public static float noise(int x, int z)
    {
        return heightMap[x & (size_x -1), z & (size_z - 1)];
    }
   
    //general algorithmic procedure for diamond square algorithm
    private static void diamondSquareAlg(int step_size, float scale)
    {
        int half_step = step_size / 2;

        for (int z = half_step; z < size_z + half_step; z += step_size)
        {
            for (int x = half_step; x < size_x + half_step; x += step_size)
            {
                setSquareHeight(x, z, step_size, UnityEngine.Random.Range(-1f, 1f) * scale * h);
            }
        }

        for (int z = 0; z < size_z; z += step_size)
        {
            for (int x = 0; x < size_x; x += step_size)
            {
                setDiamondHeight(x + half_step, z, step_size, UnityEngine.Random.Range(-1f, 1f) * scale * h); //HERE, RANDOM VALUE
                setDiamondHeight(x, z + half_step, step_size, UnityEngine.Random.Range(-1f, 1f) * scale * h); //HERE, RANDOM VALUE
            }
        }
    }

    //returns value of height at position (x,z)
    private static float getHeight(int x, int z)
    {
        try
        {
            return heightMap[x & (size_x -1) , z & (size_z -1)];
        }
        catch (Exception e) //index out of bounds
        {
            return float.MinValue;
        }
    }

    //sets value of height at position (x,z)
    private static void setHeight(int x, int z, float value)
    {
        try
        {
            heightMap[x & ( size_x - 1), z & (size_z -1)] = value;
            return;
        }
        catch (Exception e) //index out of bounds
        {
            return;
        }
    }

    //sets the height value of mid point of square
    private static void setSquareHeight(int x, int z, int size, float value)
    {
        //calculate half step size
        int half_step = size / 2;

        // a     b 
        //
        //    x
        //
        // c     d

        float
            a = getHeight(x - half_step, z - half_step),
            b = getHeight(x + half_step, z - half_step),
            c = getHeight(x - half_step, z + half_step),
            d = getHeight(x + half_step, z - half_step);

        //check for index out of bounds
        float count = 4f;
        if (a == float.MinValue)
        {
            a = 0;
            count--;
        }
        if (b == float.MinValue)
        {
            b = 0;
            count--;
        }
        if (c == float.MinValue)
        {
            c = 0;
            count--;
        }
        if (d == float.MinValue)
        {
            d = 0;
            count--;
        }

        //calcuate height of midpoint as average of 4 corners
        float centerPointHeight = (a + b + c + d) / count + value;

        //assign height value to array
        setHeight(x, z, centerPointHeight);
    }

    //sets the height value of mid point of diamond
    private static void setDiamondHeight(int x, int z, int size, float value)
    {
        //calculate half step size
        int half_step = size / 2;

        //   c
        //
        //a  x  b
        //
        //   d

        float
            a = getHeight(x - half_step, z),
            b = getHeight(x + half_step, z),
            c = getHeight(x, z - half_step),
            d = getHeight(x, z + half_step);

        //check for index out of bounds
        float count = 4f;
        if (a == float.MinValue)
        {
            a = 0;
            count--;
        }
        if (b == float.MinValue)
        {
            b = 0;
            count--;
        }
        if (c == float.MinValue)
        {
            c = 0;
            count--;
        }
        if (d == float.MinValue)
        {
            d = 0;
            count--;
        }

        //calcuate height of midpoint as average of 4 corners
        float centerPointHeight = (a + b + c + d) / count + value;

        //assign height value to array
        setHeight(x, z, centerPointHeight);
    }
}
