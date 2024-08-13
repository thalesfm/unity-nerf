using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public Vector3 point;
    public float degreesPerSec = 30.0f;

    void Update()
    {
        transform.RotateAround(point, Vector3.up, degreesPerSec * Time.deltaTime);
    }
}
