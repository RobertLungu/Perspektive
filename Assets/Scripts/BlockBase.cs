using UnityEngine;

public abstract class BlockBase : MonoBehaviour
{
    protected abstract BlockType MyType { get; }

    void Start()
    {
        Vector3Int cell = Vector3Int.RoundToInt(transform.position);
        GridManager.Instance.RegisterBlock(cell, new BlockData(MyType, gameObject));
        OnStart();
    }

    protected virtual void OnStart() { }
}