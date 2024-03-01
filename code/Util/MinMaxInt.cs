namespace Kira;

public struct MinMaxInt
{
    [Property]
    public int Min { get; set; } = 1;
    [Property]
    public int Max { get; set; } = 2;

    public MinMaxInt(int min, int max)
    {
        Min = min;
        Max = max;
    }
}