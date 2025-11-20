namespace MiniIT.CORE
{
    /// <summary>
    /// Interface for items that live in a grid (x,y).
    /// </summary>
    public interface IGridItem
    {
        int X { get; set; }
        int Y { get; set; }
    }
}