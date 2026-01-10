namespace BasicComponents;

public static class IndexExtensions
{
    public static int GetLength(this Range range)
    {
        return range.End.Value - range.Start.Value;
    }
}