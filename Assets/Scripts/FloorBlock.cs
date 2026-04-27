using UnityEngine;

public class FloorBlock : BlockBase
{
    protected override BlockType MyType => BlockType.Floor;

    public Material materialA;
    public Material materialB;

    protected override void OnStart()
    {
        Vector3Int cell = Vector3Int.RoundToInt(transform.position);
        bool even = (cell.x+ cell.z) % 2 == 0;
        GetComponentInChildren<MeshRenderer>().material = even ? materialA : materialB;
    }
}