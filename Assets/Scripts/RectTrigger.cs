using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectTrigger : MonoBehaviour
{
    // Define how many trigger points in rect will be triggered
    [Range(3, 10)]
    public int _sensitivity = 5;

    private Camera _camera = null;

    private void Awake()
    {
        _camera = Camera.main;
    }
}
