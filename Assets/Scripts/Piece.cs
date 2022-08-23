using UnityEngine;
using UnityEngine.Tilemaps;

public class Piece
{
    public TetrominoData tetrominoData { get; private set; }

    public Vector2Int[] cells;

    public Vector3Int nextPosition { get; private set; }

    public Vector3Int currentPosition { get; private set; }

    public Tilemap tilemap { get; private set; }

    // 代表游戏面板边界
    public RectInt bounds { get; private set; }

    private float dropDelay = 0.4f;

    private float lockDelay = 0.5f;

    private float dropGameTime = 0f;

    private float lockGameTime = 0f;

    private int rotationIndex = 3;

    public bool isLocked { get; private set; }

    public void Initialize(Tilemap tilemap, 
        RectInt bounds,
        Vector3Int spawnPos, 
        TetrominoData tetrominoData)
    {
        this.tilemap = tilemap;
        this.bounds = bounds;
        this.currentPosition = spawnPos;
        this.nextPosition = spawnPos;
        this.tetrominoData = tetrominoData;
        this.isLocked = false;
        this.lockGameTime = Time.time;
        this.dropGameTime = Time.time;

        if(this.cells == null)
            this.cells = new Vector2Int[tetrominoData.cells.Length];

        for (int i = 0; i < tetrominoData.cells.Length; i++)
            this.cells[i] = tetrominoData.cells[i];
    }

   

    public void HandleMoveInput()
    {
        // 按 A 方块向左移动
        if (Input.GetKeyDown(KeyCode.A))
            Move(Vector2Int.left);
        // 按 D 方块向右移动
        else if (Input.GetKeyDown(KeyCode.D))
            Move(Vector2Int.right);

        // 按 S 下落
        if (Input.GetKeyDown(KeyCode.S))
            Move(Vector2Int.down);
    }

    public void HandleRotateInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            Rotate(1);
        else if (Input.GetKeyDown(KeyCode.E))
            Rotate(-1);
    }
   
    public void Rotate(int direction)
    {
        Clear();

        ApllyRotation(direction);

        Show();
    }

    public bool Move(Vector2Int direction)
    {
        // 清除原位置上的方块
        Clear();
        // 应用合法的移动
        bool moveFlag = ApplyMovement(direction);
        // 展示方块
        Show();

        return moveFlag;
    }

    // 应用合法的移动
    private bool ApplyMovement(Vector2Int translation)
    {
        nextPosition = currentPosition + (Vector3Int)translation;
        if (positionIsValid())
        {
            currentPosition = nextPosition;
            return true;
        }
        return false;
    }

    private void ApllyRotation(int direction)
    {
        int originalRotationIndex = rotationIndex;
        
        rotationIndex = DataUtil.Wrap(rotationIndex + direction, 0, 4);

        // Rotate all of the cells using a rotation matrix
        ApplyRotationMatrix(direction);

        // Revert the rotation if the wall kick tests fail
        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotationIndex;
            ApplyRotationMatrix(-direction);
        }
    }

    // 应用旋转矩阵
    private void ApplyRotationMatrix(int direction)
    {
        float[] rotationMatrix = DataUtil.RotationMatrix;
        // Rotate all of the cells using the rotation matrix
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2 cell = cells[i];
            int x, y;
            switch (tetrominoData.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    // "I" and "O" are rotated from an offset center point
                    cell.x -= 0.5f;
                    cell.y -= 0.5f; // 0, 1, -1, 0
                    x = Mathf.CeilToInt((cell.x * rotationMatrix[0] * direction) + (cell.y * rotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * rotationMatrix[2] * direction) + (cell.y * rotationMatrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * rotationMatrix[0] * direction) + (cell.y * rotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * rotationMatrix[2] * direction) + (cell.y * rotationMatrix[3] * direction));
                    break;
            }

            cells[i] = new Vector2Int(x, y);
        }
    }

    // 在Tilemap上绘制方块
    public void Show()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int cellPosition = currentPosition +
                (Vector3Int)cells[i];
            tilemap.SetTile(cellPosition, tetrominoData.tile);
        }
    }

    // 清除原位置上的方块
    public void Clear()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int cellPosition = currentPosition + (Vector3Int)cells[i];
            tilemap.SetTile(cellPosition, null);
        }
    }

    // 方块自动下落
    public void DropDown()
    {
        if ((Time.time - dropGameTime) >= dropDelay)
        {
            if (Move(Vector2Int.down))
                lockGameTime = Time.time;   // 若下落成功则重置锁定时间

            dropGameTime = Time.time;
        }
    }

    public void TryToLockPiece()
    {
        if (Time.time - lockGameTime >= lockDelay)
        {
            isLocked = true;
            lockGameTime = Time.time;
        }
    }

    // 判断方块位置是否合法
    private bool positionIsValid()
    {
        for(int i=0; i<tetrominoData.cells.Length; i++)
        {
            Vector2Int cellPosition = (Vector2Int)nextPosition + cells[i];
            if (!bounds.Contains(cellPosition) || tilemap.HasTile((Vector3Int)cellPosition))
                return false;
        }

        return true;
    }

    // 测试旋转后的方块是否存在合法位置
    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < tetrominoData.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = tetrominoData.wallKicks[wallKickIndex, i];
            if(ApplyMovement(translation))
                return true;
        }

        return false;
    }

    // 根据方块目前状态和旋转方向获取测试踢墙矩阵的索引
    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return DataUtil.Wrap(wallKickIndex, 0, tetrominoData.wallKicks.GetLength(0));
    }

    
}
