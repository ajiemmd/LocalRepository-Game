using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethod 
{
    private const float dotThreshold = 0.5f;
    public static bool IsFacingTarget(this Transform transform,Transform target)
    {
        var vectorTOTarget = target.position - transform.position;
        vectorTOTarget.Normalize();

        float dot = Vector3.Dot(transform.forward, vectorTOTarget);

        return dot >= dotThreshold;
    }

}
