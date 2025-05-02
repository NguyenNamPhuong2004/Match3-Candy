using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : Singleton<GridManager>
{
    public Candy[,] candyGrid = new Candy[8, 8];
    private LevelData levelData;
    private GameObject[] candyPrefabs;
    private GameObject[] specialCandyPrefabs;
    private GameObject dirtTilePrefab;
    private GameObject lockedTilePrefab;
    private GameObject emptyTilePrefab;
    private Dictionary<Vector2Int, GameObject> dirtTileObjects = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> lockedTileObjects = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> emptyTileObjects = new Dictionary<Vector2Int, GameObject>();
    private HashSet<Vector2Int> emptyTiles;
    private const int GRID_WIDTH = 8;
    private const int GRID_HEIGHT = 8;


    public void Initialize(LevelData levelData, GameObject[] candyPrefabs, GameObject[] specialCandyPrefabs, GameObject dirtTilePrefab, GameObject lockedTilePrefab, GameObject emptyTilePrefab)
    {
        this.levelData = levelData;
        this.candyPrefabs = candyPrefabs;
        this.specialCandyPrefabs = specialCandyPrefabs;
        this.dirtTilePrefab = dirtTilePrefab;
        this.lockedTilePrefab = lockedTilePrefab;
        this.emptyTilePrefab = emptyTilePrefab;
        this.emptyTiles = new HashSet<Vector2Int>(levelData.emptyTiles);
        dirtTileObjects.Clear();
        lockedTileObjects.Clear();
        emptyTileObjects.Clear();
        Debug.Log("GridManager initialized");
    }

    public IEnumerator InitializeBoard()
    {
        if (emptyTilePrefab != null)
        {
            foreach (var pos in levelData.emptyTiles)
            {
                GameObject emptyObj = Instantiate(emptyTilePrefab, new Vector3(pos.y, pos.x, 0), Quaternion.identity);
                emptyTileObjects[pos] = emptyObj;
            }
        }

        if (dirtTilePrefab != null)
        {
            foreach (var pos in levelData.dirtTiles)
            {
                if (!emptyTiles.Contains(pos))
                {
                    GameObject dirtObj = Instantiate(dirtTilePrefab, new Vector3(pos.y, pos.x, 0), Quaternion.identity);
                    dirtTileObjects[pos] = dirtObj;
                }
            }
        }

        if (lockedTilePrefab != null)
        {
            foreach (var pos in levelData.lockedTiles)
            {
                if (!emptyTiles.Contains(pos))
                {
                    GameObject lockedObj = Instantiate(lockedTilePrefab, new Vector3(pos.y, pos.x, 0), Quaternion.identity);
                    lockedTileObjects[pos] = lockedObj;
                }
            }
        }

        for (int row = 0; row < GRID_HEIGHT; row++)
        {
            for (int col = 0; col < GRID_WIDTH; col++)
            {
                if (!emptyTiles.Contains(new Vector2Int(row, col)))
                {
                    yield return StartCoroutine(SpawnCandy(row, col, false));
                }
            }
        }
    }

    public IEnumerator SpawnCandy(int row, int col, bool withDrop, SpecialCandyType specialType = SpecialCandyType.None)
    {
        if (emptyTiles.Contains(new Vector2Int(row, col))) yield break;

        GameObject prefab;
        if (specialType != SpecialCandyType.None)
        {
            int index = specialType == SpecialCandyType.StripedHorizontal ? 0 :
                        specialType == SpecialCandyType.StripedVertical ? 1 :
                        specialType == SpecialCandyType.Wrapped ? 2 : 3;
            prefab = specialCandyPrefabs[index];
            if (index == 0 || index == 1) SoundManager.Ins.StrippedCreatedSound();
            if (index == 2 ) SoundManager.Ins.WrappedCreatedSound();
            if (index == 3 ) SoundManager.Ins.ColorBombCreatedSound();
        }
        else
        {
            prefab = candyPrefabs[Random.Range(0, candyPrefabs.Length)];
        }

        GameObject candyObj = Instantiate(prefab, new Vector3(col, GRID_HEIGHT - 1, 0), Quaternion.identity);
        if (specialType == SpecialCandyType.StripedHorizontal) candyObj.transform.Rotate(0, 0, 90);
        Candy candy = candyObj.GetComponent<Candy>();
        candy.row = row;
        candy.column = col;
        candy.isSpecial = specialType != SpecialCandyType.None;
        candy.specialType = specialType;
        candy.isLocked = levelData.lockedTiles.Exists(t => t == new Vector2Int(row, col));
        candyGrid[row, col] = candy;

        if (withDrop)
        {
            Vector3 targetPos = new Vector3(col, row, 0);
            float speed = 10f;
            while (Vector3.Distance(candyObj.transform.position, targetPos) > 0.1f)
            {
                candyObj.transform.position = Vector3.MoveTowards(candyObj.transform.position, targetPos, Time.deltaTime * speed);
                yield return null;
            }
        }
        candyObj.transform.position = new Vector3(col, row, 0);
    }

    public IEnumerator FillEmptySpaces()
    {
        bool moved = false;
        for (int col = 0; col < GRID_WIDTH; col++)
        {
            int emptyCount = 0;
            for (int row = 0; row < GRID_HEIGHT; row++)
            {
                if (candyGrid[row, col] == null)
                {
                    emptyCount++;
                }
                else if (emptyCount > 0)
                {
                    Candy candy = candyGrid[row, col];
                    candyGrid[row - emptyCount, col] = candy;
                    candyGrid[row, col] = null;
                    candy.row = row - emptyCount;
                    candy.MoveTo(new Vector3(col, row - emptyCount, 0));
                    moved = true;
                }
            }

            for (int i = 0; i < emptyCount; i++)
            {
                int row = GRID_HEIGHT - emptyCount + i;
                yield return StartCoroutine(SpawnCandy(row, col, true));
            }
        }

        if (moved)
        {
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void ReplaceWithSpecialCandy(int row, int col, SpecialCandyType specialType)
    {
        if (candyGrid[row, col] != null)
        {
            Destroy(candyGrid[row, col].gameObject);
            candyGrid[row, col] = null;
        }
        StartCoroutine(SpawnCandy(row, col, false, specialType));
    }

    public void RemoveDirtTile(Vector2Int pos)
    {
        if (dirtTileObjects.ContainsKey(pos))
        {
            Destroy(dirtTileObjects[pos]);
            dirtTileObjects.Remove(pos);
        }
    }

    public void RemoveLockedTile(Vector2Int pos)
    {
        if (lockedTileObjects.ContainsKey(pos))
        {
            Destroy(lockedTileObjects[pos]);
            lockedTileObjects.Remove(pos);
            if (candyGrid[pos.x, pos.y] != null)
            {
                candyGrid[pos.x, pos.y].isLocked = false;
            }
        }
    }

    public bool IsEmptyTile(Vector2Int pos) => emptyTiles.Contains(pos);
}