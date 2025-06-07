using UnityEngine;

public interface IGridObject
{
    float CellSize { get; }
    string GridType { get; }
    Transform Transform { get; }
}
