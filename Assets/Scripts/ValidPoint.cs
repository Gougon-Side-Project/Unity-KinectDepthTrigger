using Windows.Kinect;

public class ValidPoint
{
    private float _x = 0;
    private float _y = 0;
    private float _z = 0;

    public ValidPoint(ColorSpacePoint colorSpacePoint, float z)
    {
        _x = colorSpacePoint.X;
        _y = colorSpacePoint.Y;
        _z = z;
    }

    public float X
    {
        get
        {
            return _x;
        }
    }

    public float Y
    {
        get
        {
            return _y;
        }
    }

    public float Z
    {
        get
        {
            return _z;
        }
    }
}
