using UnityEngine;

public enum BlockType { Floor, Brick, Red, Win }

public struct BlockData
{
    public BlockType type;
    public GameObject obj;

    public BlockData(BlockType t, GameObject g) { type = t; obj = g; }
}