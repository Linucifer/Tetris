using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    // 俄罗斯方块数据 其中的 tile 在Inspector面板初始化
    [SerializeField]
    private TetrominoData[] tetrominoes;

    private Tilemap tilemap;

    private Piece activePiece;  // 代表活跃的俄罗斯方块

    private Vector2Int boardSize = new Vector2Int(10, 20);

    // 代表面板大小和位置的矩形 中心位于(0,0)
    private RectInt bounds
    {
        get
        {
            Vector2Int recPos = new Vector2Int(-(boardSize.x / 2), -(boardSize.y / 2));
            return new RectInt(recPos, boardSize);
        }
    }

    // 每个俄罗斯方块生成点
    [SerializeField]
    private Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    [SerializeField]
    private float lockDelay = 0.2f;

    private float gameTime = 0f;

    [SerializeField]    
    private float stepDelay = 0.2f;

    // 游戏运行状态 true代表运行 false代表结束
    private bool gameIsLiving = true;
    


    private void Awake()
    {
        activePiece = new Piece();

        tilemap = GetComponentInChildren<Tilemap>();
        
        for(int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();    // 初始化所有的俄罗斯方块数据
        }
    }

    private void Start()
    {
        // 初始化活跃的俄罗斯方块
        SpawnActivePiece();
        // 在Tilemap上绘制活跃的俄罗斯方块
        ShowActivePiece();
        //Debug.Log("Mathf.Cos(Mathf.PI / 2f) = " + DataUtil.cos);
        //Debug.Log("Mathf.Sin(Mathf.PI / 2f) = " + DataUtil.sin);

    }

    private void Update()
    {
        if (gameIsLiving)
        {
            activePiece.HandleMoveInput();
            activePiece.HandleRotateInput();
            activePiece.DropDown();
            activePiece.TryToLockPiece();
            TryToUpdateBoardAndSpawnPiece();
        } 
    }

    private void TryToUpdateBoardAndSpawnPiece()
    {
        if (activePiece.isLocked)
        {
            if (!IsGameOver())
            {
                UpdateBoard();
                SpawnActivePiece();
                ShowActivePiece();
            }    
        }
    }

    private void ClearLine(int row)
    {
        while (row < bounds.yMax - 1)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int aboveCellPosition = new Vector3Int(col, row + 1, 0);
                Vector3Int cellPosition = new Vector3Int(col, row, 0);
                TileBase aboveTile = tilemap.GetTile(aboveCellPosition);
                tilemap.SetTile(cellPosition, aboveTile);
            }
            row++;
        }
    }

    private void UpdateBoard()
    {
        if (activePiece.isLocked)
        {
            int row = bounds.yMax - 2;

            while (row >= bounds.yMin)
            {
                if (IsLineFull(row))
                    ClearLine(row);
                row--;
            }
        }
        
    }

    // 判断指定行是否已经填满
    private bool IsLineFull(int row)
    {
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int cellPosition = new Vector3Int(col, row, 0);
            if (!tilemap.HasTile(cellPosition))
                return false;
        }

        return true;
    }

    // 生成并初始化活跃的俄罗斯方块
    private void SpawnActivePiece()
    {
        int tetrominoIndex = Random.Range(0, tetrominoes.Length);
        //tetrominoIndex = 0;
        TetrominoData tetrominoData = tetrominoes[tetrominoIndex];
        activePiece.Initialize(tilemap, bounds, spawnPosition, tetrominoData);
    }
    // 在Tilemap上绘制活跃的俄罗斯方块
    private void ShowActivePiece()
    {
        if (activePiece != null)
            activePiece.Show();
    }



    private bool IsGameOver()
    {
        int topRow = bounds.yMax - 1;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int cellPosition = new Vector3Int(col, topRow, 0);
            if (tilemap.HasTile(cellPosition))
            {
                gameIsLiving = false;
                return true;
            }
        }

        return false;
    }
   
}
