using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageViewer : MonoBehaviour
{
    public RawImage _depthImage = null;

    public MeasureDepth _measureDepth = null;

    public MultiSourceManager _multiManager = null;
    
    void Update()
    {
        _depthImage.texture = _multiManager.GetColorTexture();
    }
}
