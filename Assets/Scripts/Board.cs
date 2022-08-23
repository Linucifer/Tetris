using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    // ����˹�������� ���е� tile ��Inspector����ʼ��
    [SerializeField]
    private TetrominoData[] tetrominoes;

    private Tilemap tilemap;

    private Piece activePiece;  // �����Ծ�Ķ���˹����

    private Vector2Int boardSize = new Vector2Int(10, 20);

    // ��������С��λ�õľ��� ����λ��(0,0)
    private RectInt bounds
    {
        get
        {
            Vector2Int recPos = new Vector2Int(-(boardSize.x / 2), -(boardSize.y / 2));
            return new RectInt(recPos, boardSize);
        }
    }

    // ÿ������˹�������ɵ�
    [SerializeField]
    private Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    [SerializeField]
    private float lockDelay = 0.2f;

    private float gameTime = 0f;

    [SerializeField]    
    private float stepDelay = 0.2f;

    // ��Ϸ����״̬ true�������� false�������
    private bool gameIsLiving = true;
    


    private void Awake()
    {
        activePiece = new Piece();

        tilemap = GetComponentInChildren<Tilemap>();
        
        for(int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();    // ��ʼ�����еĶ���˹��������
        }
    }

    private void Start()
    {
        // ��ʼ����Ծ�Ķ���˹����
        SpawnActivePiece();
        // ��Tilemap�ϻ��ƻ�Ծ�Ķ���˹����
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

    // �ж�ָ�����Ƿ��Ѿ�����
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

    // ���ɲ���ʼ����Ծ�Ķ���˹����
    private void SpawnActivePiece()
    {
        int tetrominoIndex = Random.Range(0, tetrominoes.Length);
        //tetrominoIndex = 0;
        TetrominoData tetrominoData = tetrominoes[tetrominoIndex];
        activePiece.Initialize(tilemap, bounds, spawnPosition, tetrominoData);
    }
    // ��Tilemap�ϻ��ƻ�Ծ�Ķ���˹����
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
