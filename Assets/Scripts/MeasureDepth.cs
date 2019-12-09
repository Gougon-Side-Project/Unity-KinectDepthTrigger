using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;

public class MeasureDepth : MonoBehaviour
{
    public const int DEPTH_RESOLUTION_X = 512;
    public const int DEPTH_RESOLUTION_Y = 424;

    public const int CAMERA_RESOLUTION_X = 1920;
    public const int CAMERA_RESOLUTION_Y = 1080;

    public float CANVAS_SCALE = 0.5609375f;

    private const int TRIGGER_POINT_INTERVAL = 3;
    private const int TRIGGER_REQUIRED_POINTS = 5;

    // private const int TRIGGER_RECT_SIZE = 5;

    public MultiSourceManager _multiManager = null;

    [Range(1, 2)]
    public int _filteringSensitivity = 1;

    [Range(0.001f, 0.1f)]
    public float _startTriggerDistance = 0.025f;

    [Range(0.001f, 0.5f)]
    public float _endTriggerDistance = 0.2f;

    private KinectSensor _sensor = null;

    private CoordinateMapper _mapper = null;

    private ColorSpacePoint[] _colorSpacePoints = null;

    private CameraSpacePoint[] _cameraSpacePoints = null;

    private ushort[] _wallDepth = null;

    private ushort[] _depthData = null;

    private List<Vector2> _triggerPoints = null;

    private Camera _camera = null;

    private void Start()
    {
        _sensor = KinectSensor.GetDefault();
        _mapper = _sensor.CoordinateMapper;

        _colorSpacePoints = new ColorSpacePoint[DEPTH_RESOLUTION_X * DEPTH_RESOLUTION_Y];
        _cameraSpacePoints = new CameraSpacePoint[DEPTH_RESOLUTION_X * DEPTH_RESOLUTION_Y];

        _camera = Camera.main;
    }

    private void Update()
    {
        ControllingTriggerRange();

        _depthData = _multiManager.GetDepthData();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _wallDepth = DetectingWallDepth();
        }

        if (_wallDepth != null)
        {
            _depthData = FittingDepthDataIntoTriggerRange();

            _triggerPoints = MakeTriggerPoints();
        }
    }

    /* On GUI, For debug
    private void OnGUI()
    {
        if (_triggerPoints == null)
            return;

        foreach (Vector2 point in _triggerPoints)
        {
            Vector2 rectSize = new Vector2(RECT_SIZE, RECT_SIZE);
            Rect rect = new Rect(point, rectSize);

            GUI.Box(rect, "");
        }
    }
    */

    #region Is Trigger
    public bool IsTrigger(Rect cameraResolutionRect)
    {
        Rect actualResolutionRect = TransformCameraResolutionRectToActualResolutionRect(cameraResolutionRect);
        
        int count = 0;
        foreach (Vector2 point in _triggerPoints)
        {
            if (IsPointInRectRange(point, actualResolutionRect))
            {
                count++;
            }
        }
        return count >= TRIGGER_REQUIRED_POINTS;
    }

    private Rect TransformCameraResolutionRectToActualResolutionRect(Rect cameraResolutionRect)
    {
        float x = cameraResolutionRect.xMin * CANVAS_SCALE;
        float y = cameraResolutionRect.yMin * CANVAS_SCALE;
        float width = cameraResolutionRect.width * CANVAS_SCALE;
        float height = cameraResolutionRect.height * CANVAS_SCALE;
        Rect actualResolutionRect = new Rect(x, y, width, height);
        return actualResolutionRect;
    }

    private bool IsPointInRectRange(Vector2 point, Rect rect)
    {
        float x = point.x;
        float y = point.y;
        float left = rect.xMin;
        float top = rect.yMin;
        float width = rect.width;
        float height = rect.height;
        return IsX_InRectRange(x, left, width) && IsY_InRectRange(y, top, height);
    }

    private bool IsX_InRectRange(float x, float left, float width)
    {
        return x >= left && x <= left + width;
    }

    private bool IsY_InRectRange(float y, float top, float height)
    {
        return y >= top && y <= top + height;
    }
    #endregion

    private ushort[] DetectingWallDepth()
    {
        ushort[] wallDepth = new ushort[DEPTH_RESOLUTION_X * DEPTH_RESOLUTION_Y];

        for (int index = 0; index < _depthData.Length; index++)
        {
            wallDepth[index] = _depthData[index];
        }

        return wallDepth;
    }

    private ushort[] FittingDepthDataIntoTriggerRange()
    {
        ushort[] depthData = new ushort[DEPTH_RESOLUTION_X * DEPTH_RESOLUTION_Y];

        for (int index = 0; index < depthData.Length; index++)
        {
            ushort pointDepth = _depthData[index];
            ushort wallDepth = _wallDepth[index];
            depthData[index] = FittingEachDepthData(pointDepth, wallDepth);
        }

        return depthData;
    }

    private ushort FittingEachDepthData(ushort pointDepth, ushort wallDepth)
    {
        // Depth data range is 0 ~ 4096(mm), so convert meter to minimeter
        float startTriggerDistance = wallDepth - _startTriggerDistance * 1000;
        float endTriggerDistance = wallDepth - _endTriggerDistance * 1000;

        ushort fittingDepthData = (pointDepth < wallDepth &&
            IsPointDepthInTriggerRange(pointDepth, startTriggerDistance, endTriggerDistance)) ?
            pointDepth : (ushort)0;

        return fittingDepthData;
    }

    private bool IsPointDepthInTriggerRange(ushort pointDepth, float startTriggerDistance, float endTriggerDistance)
    {
        return pointDepth >= endTriggerDistance && pointDepth <= startTriggerDistance;
    }

    #region Make trigger points
    // According to current depth data, make trigger points that contain x & y
    private List<Vector2> MakeTriggerPoints()
    {
        MappingSpace();

        List<Vector2> triggerPoints = new List<Vector2>();
        for (int y = 0; y < DEPTH_RESOLUTION_Y / TRIGGER_POINT_INTERVAL; y++)
        {
            for (int x = 0; x < DEPTH_RESOLUTION_X / TRIGGER_POINT_INTERVAL; x++)
            {
                MakeTriggerPoint(triggerPoints, x, y);
            }
        }

        return triggerPoints;
    }

    private void MappingSpace()
    {
        _mapper.MapDepthFrameToColorSpace(_depthData, _colorSpacePoints);
        _mapper.MapDepthFrameToCameraSpace(_depthData, _cameraSpacePoints);
    }

    private void MakeTriggerPoint(List<Vector2> triggerPoints, int x, int y)
    {
        // Transform index to depth index
        int index = x + (y * DEPTH_RESOLUTION_X);
        index *= TRIGGER_POINT_INTERVAL;

        ColorSpacePoint point = _colorSpacePoints[index];

        // If depth != 0 -> trigger point
        if (_depthData[index] > 0)
        {
            if (!IsDepthDataExist(index) || Mathf.Abs(point.X - 0) < 1 || Mathf.Abs(point.Y - 0) < 1)
                return;

            Vector2 screenPoint = ScreenToCamera(new Vector2(point.X, point.Y));

            triggerPoints.Add(screenPoint);
        }
    }
    #endregion

    #region Median Filtering

    // Using median filtering to judge is depth data belong noise
    private bool IsDepthDataExist(int index)
    {
        int row = index / DEPTH_RESOLUTION_X;
        int col = index % DEPTH_RESOLUTION_X;

        // Find neighbors
        Dictionary<int, ushort> neighbors = DetectNeighborPoint(row, col);

        // Choose median depth data to replace all neighbor datas
        List<ushort> values = neighbors.Values.ToList();
        values.Sort();
        ushort median = values[values.Count / 2];
        foreach (KeyValuePair<int, ushort> neighbor in neighbors)
        {
            _depthData[neighbor.Key] = median;
        }

        return median != 0;
    }

    // Detect neighbor of point (3*3)
    // It won't detect the neigobor who next to it, it will detect the neighbor who has trigger_interval pixels from it
    // Noise down
    private Dictionary<int, ushort> DetectNeighborPoint(int row, int col)
    {
        int filteringSensitivity = _filteringSensitivity * TRIGGER_POINT_INTERVAL;
        
        Dictionary<int, ushort> neighbor = new Dictionary<int, ushort>();
        for (int i = row - filteringSensitivity; i <= row + filteringSensitivity; i += TRIGGER_POINT_INTERVAL)
        {
            if (i < 0 || i >= DEPTH_RESOLUTION_Y)
                continue;
            for (int j = col - filteringSensitivity; j <= col + filteringSensitivity; j += TRIGGER_POINT_INTERVAL)
            {
                if (j < 0 || j >= DEPTH_RESOLUTION_X)
                    continue;
                int index = i * DEPTH_RESOLUTION_X + j;
                neighbor.Add(index, _depthData[index]);
            }
        }
        return neighbor;
    }
    #endregion

    // Translate color space position to camera position
    private Vector2 ScreenToCamera(Vector2 screenPosition)
    {
        float normalizeX = Mathf.InverseLerp(0, CAMERA_RESOLUTION_X, screenPosition.x);
        float normalizeY = Mathf.InverseLerp(0, CAMERA_RESOLUTION_Y, screenPosition.y);
        Vector2 normalizeScreen = new Vector2(normalizeX, normalizeY);

        Vector2 screenPoint = new Vector2(normalizeX * _camera.pixelWidth, normalizeY * _camera.pixelHeight);

        return screenPoint;
    }

    // Inhibiting end < start
    private void ControllingTriggerRange()
    {
        if (_endTriggerDistance <= _startTriggerDistance)
            _endTriggerDistance = _startTriggerDistance + 0.0001f;
    }
}
