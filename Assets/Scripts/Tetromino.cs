using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Tetromino
{
    // 代表不同形状的俄罗斯方块
    I, J, L, O, S, T, Z
}

[System.Serializable]
public struct TetrominoData
{
    public Tile tile;
    public Tetromino tetromino;

    public Vector2Int[] cells;
    public Vector2Int[,] wallKicks;

    public void Initialize()
    {
        cells = DataUtil.Cells[tetromino];
        wallKicks = DataUtil.WallKicks[tetromino];
    }

 

}
