using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polynome2
{
    public static int GetRealSolution(float _a, float _b, float _c, ref Vector2 _solution)
    {
        float discriminant = _b * _b - 4f * _a * _c;

        if (discriminant < 0f)
        {
            return 0;
        }
        else if (discriminant == 0f)
        {
            _solution.x = -_b / (2f * _a);
            return 1;
        }
        else
        {
            _solution.x = (-_b + Mathf.Sqrt(discriminant)) / (2f * _a);
            _solution.y = (-_b - Mathf.Sqrt(discriminant)) / (2f * _a);
            return 2;
        }    
    }
}
