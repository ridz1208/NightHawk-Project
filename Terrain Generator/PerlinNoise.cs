
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PerlinNoise {

    private static readonly int MAX_NUMBER_BEFORE_WRAP = 255;
    private static readonly int NUMBER_GRADIENTS = 16;
    private static readonly Vector2[] gradients = new Vector2[NUMBER_GRADIENTS];

    private static readonly int[] seeds = new int[MAX_NUMBER_BEFORE_WRAP + 1];
    private static readonly int[] pseeds = new int[2 * (MAX_NUMBER_BEFORE_WRAP + 1)]; //permuteated seeds


    static PerlinNoise()
    {
        //populate the gradients
        for (int i = 0; i < NUMBER_GRADIENTS; i++)
            gradients[i] = UnityEngine.Random.insideUnitCircle;
      
                //populate array with elements
        for (int i = 0; i < MAX_NUMBER_BEFORE_WRAP; i++)
            seeds[i] = i;

        //randomly shuffle the array 
        System.Random r = new System.Random();
        int i1, i2, i3, i4, v1, v2, v3, v4;
        for (int i = 0; i < MAX_NUMBER_BEFORE_WRAP; i++)
        {
            i1 = i;
            i2 = r.Next(0, MAX_NUMBER_BEFORE_WRAP + 1);
            i3 = r.Next(0, MAX_NUMBER_BEFORE_WRAP + 1);
            i4 = r.Next(0, MAX_NUMBER_BEFORE_WRAP + 1);
            v1 = seeds[i1]; v2 = seeds[i2]; v3 = seeds[i3]; v4 = seeds[i4];
            seeds[i1] = v2; seeds[i2] = v4; seeds[i3] = v1; seeds[i4] = v3;
        }

        //duplicate the seeds array to avoid index wrapping
        for(int i = 0; i < 2 * MAX_NUMBER_BEFORE_WRAP + 1; i++)
            pseeds[i] = seeds[i & MAX_NUMBER_BEFORE_WRAP];
    }

    /*
        returns the noise value between the interval [-1,1] of the given point P
           
        *-------*--------*--------*
        |       |        |        |
        |       |        |        |
        |       |        |        |
        *-------*--------*--------*
        |       |        |        |
        |       |        |        |
        |       |        |        |
        *-------*--------*--------*
        |       |        |        |
        |       |        |    P   |  P is at position (x,y) represented by a Vector2 type variable
        |       |        |        |
        *-------*--------*--------*
                         x = 2, y = 0
     */
    public static float noise(float x, float z)
    {
        //get position in grid 
        int xPos = floor(x);
        int zPos = floor(z);

        //get relative position in the small grid square
        x = x - xPos;
        z = z - zPos;

        // Wrap the integer cells at the given maximum specified 
        xPos = xPos & MAX_NUMBER_BEFORE_WRAP;
        zPos = zPos & MAX_NUMBER_BEFORE_WRAP;

        // Calculate a set of 4 hashed gradient indices. For example, gradient at corner 00 is gradients[gi00] etc..
        int nGradients = gradients.Length;
        int gi00 = pseeds[xPos + pseeds[zPos]] % nGradients;
        int gi01 = pseeds[xPos + pseeds[zPos + 1]] % nGradients;
        int gi10 = pseeds[xPos + 1 + pseeds[zPos]] % nGradients;
        int gi11 = pseeds[xPos + 1 +  pseeds[zPos + 1]] % nGradients;

        // Calculate noise contributions from each of the eight corners
        float n00 = dot(gradients[gi00], x, z);
        float n10 = dot(gradients[gi01], x - 1, z);
        float n01 = dot(gradients[gi10], x, z - 1);
        float n11 = dot(gradients[gi11], x - 1, z - 1);
      
        // Compute the weight curve value for each of x z. This value will be needed to proportionally give more weight to nearby gradients of the
        // given point P when interpolating
        float u = weight(x);
        float w = weight(z);

        // Interpolate along x the contributions from each of the corners
        float nx0 = mix(n00, n10, u);
        float nx1 = mix(n01, n11, u);

        // Interpolate the two last results along z
        float nxz = mix(nx0, nx1, w);

        return nxz;
    }

    #region helper methods
    private static int floor(float x)
    {
        return x > 0 ? (int) x : (int)x - 1;
    }
    private static float dot(Vector2 g, float x, float z)
    {
        return g.x * x + g.y * z; //should be z instead of y
    }
    private static float mix(float a, float b, float t)
    {
        return (1 - t) * a + t * b;
    }
    private static float weight(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }
    #endregion
}