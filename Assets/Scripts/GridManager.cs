using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    private Dictionary<Vector3Int, BlockData> grid = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RegisterBlock(Vector3Int cell, BlockData data) => grid[cell] = data;

    public bool HasBlock(Vector3Int cell) => grid.ContainsKey(cell);
    public bool TryGetBlock(Vector3Int cell, out BlockData data) => grid.TryGetValue(cell, out data);

    public bool TryGetTopmostInColumn(int x, int z, out Vector3Int topCell)
    {
        topCell = default;
        int best = int.MinValue;
        bool found = false;

        foreach (var kvp in grid)
        {
            if (kvp.Key.x == x && kvp.Key.z == z && kvp.Key.y > best)
            {
                best = kvp.Key.y;
                topCell = kvp.Key;
                found = true;
            }
        }
        return found;
    }

    public IEnumerable<KeyValuePair<Vector3Int, BlockData>> AllBlocks() => grid;
}
