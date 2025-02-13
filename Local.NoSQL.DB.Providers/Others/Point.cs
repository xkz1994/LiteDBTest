

namespace Local.NoSQL.DB.Providers.Others;

/// <summary>
/// 双浮点类型坐标点
/// </summary>
[Serializable]
public struct Point(double x, double y) 
{
    public static readonly Point Empty = new(0, 0);

    private double _x = x;

    private double _y = y;

    public double X
    {
        readonly get => _x;
        set => _x = value;
    }

    public double Y
    {
        readonly get => _y;
        set => _y = value;
    }

    public readonly bool IsEmpty => _x == 0d && _y == 0d;

    public Point(Point p) : this(p.X, p.Y)
    {
    }



    public readonly bool Equals(Point other)
    {
        return _x.Equals(other._x) && _y.Equals(other._y);
    }

    public readonly override bool Equals(object? obj)
    {
        return obj is Point other && Equals(other);
    }

    public readonly override int GetHashCode()
    {
        return HashCode.Combine(_x, _y);
    }

    public Point Clone()
    {
        return new Point(_x, _y);
    }



    
}