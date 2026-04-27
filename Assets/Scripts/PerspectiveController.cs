using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PerspectiveController : MonoBehaviour
{
    public static PerspectiveController Instance { get; private set; }

    private bool is2D;
    private GridMovement player;
    private List<GameObject> hiddenObjects = new();
    private List<(GameObject obj, Vector3 origPos)> movedObjects = new();

    public Dictionary<Vector2Int, BlockType> flatGrid = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        player = FindFirstObjectByType<GridMovement>();
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        if (!is2D)
        {
            DeathCause? pendingDeath = CollapseTo2D();
            is2D = true;
            player.is2D = true;
            GameEvents.TriggerPerspective(true);

            if (pendingDeath.HasValue)
            {
                player.inputLocked = true;
                StartCoroutine(DeferredDeath(pendingDeath.Value));
            }
        }
        else
        {
            RestoreTo3D();
            is2D = false;
            player.is2D = false;
            GameEvents.TriggerPerspective(false);
        }
    }

    IEnumerator DeferredDeath(DeathCause cause)
    {
        yield return null;
        GameEvents.TriggerDeath(cause);
    }

    DeathCause? CollapseTo2D()
    {
        Vector3Int playerCell = Vector3Int.RoundToInt(player.transform.position);
        hiddenObjects.Clear();
        flatGrid.Clear();

        var columnTop = new Dictionary<Vector2Int, (int y, BlockType type)>();

        foreach (var kvp in GridManager.Instance.AllBlocks())
        {
            var col = new Vector2Int(kvp.Key.x, kvp.Key.z);
            if (!columnTop.ContainsKey(col) || kvp.Key.y > columnTop[col].y)
                columnTop[col] = (kvp.Key.y, kvp.Value.type);
        }

        var playerCol = new Vector2Int(playerCell.x, playerCell.z);
        Vector3Int below = playerCell + Vector3Int.down;

        DeathCause? death = null;

        if (columnTop.TryGetValue(playerCol, out var topData) && topData.y > playerCell.y)
            death = DeathCause.Crushed;
        else if (GridManager.Instance.TryGetBlock(below, out var underData) && underData.type == BlockType.Brick)
            death = DeathCause.Bricked;

        foreach (var kvp in columnTop)
            flatGrid[kvp.Key] = kvp.Value.type;

        movedObjects.Clear();
        foreach (var kvp in GridManager.Instance.AllBlocks())
        {
            var col = new Vector2Int(kvp.Key.x, kvp.Key.z);
            if (kvp.Key.y < columnTop[col].y)
            {
                kvp.Value.obj.SetActive(false);
                hiddenObjects.Add(kvp.Value.obj);
            }
            else
            {
                movedObjects.Add((kvp.Value.obj, kvp.Value.obj.transform.position));
                kvp.Value.obj.transform.position = new Vector3(kvp.Key.x, 0f, kvp.Key.z);
            }
        }

        player.transform.position = new Vector3(playerCell.x, 1f, playerCell.z);
        player.SetTarget(new Vector3(playerCell.x, 1f, playerCell.z));

        return death;
    }

    void RestoreTo3D()
    {
        foreach (var obj in hiddenObjects)
            if (obj != null) obj.SetActive(true);
        hiddenObjects.Clear();

        foreach (var (obj, origPos) in movedObjects)
            if (obj != null) obj.transform.position = origPos;
        movedObjects.Clear();

        flatGrid.Clear();

        Vector3Int playerCell = Vector3Int.RoundToInt(player.transform.position);
        if (GridManager.Instance.TryGetTopmostInColumn(playerCell.x, playerCell.z, out Vector3Int top))
        {
            Vector3 restore = new Vector3(top.x, top.y + 1f, top.z);
            player.transform.position = restore;
            player.SetTarget(restore);
        }
    }
}
