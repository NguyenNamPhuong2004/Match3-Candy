using UnityEngine;
using System.Collections.Generic;

public class LevelManager : Singleton<LevelManager>
{
    public LevelData currentLevel;
    private LevelData[] levels;
    private int currentLevelIndex;
    public int remainingSwaps;
    public Dictionary<CandyType, int> matchedCandies; // Đếm kẹo match theo loại
    private HashSet<Candy> matchedCandiesSet; // Theo dõi instance kẹo đã match
    public HashSet<Vector2Int> clearedDirtTiles;
    public HashSet<Vector2Int> clearedLockedTiles;

    public void Initialize(LevelData[] levels)
    {
        this.levels = levels;
        currentLevelIndex = DataPlayer.GetLevelGame() - 1;
        LoadCurrentLevel();
        Debug.Log($"LevelManager initialized: level={currentLevel?.level}");
    }

    private void LoadCurrentLevel()
    {
        if (currentLevelIndex >= levels.Length)
        {
            currentLevel = null;
            return;
        }
        currentLevel = levels[currentLevelIndex];
        remainingSwaps = currentLevel.swapLimit;
        matchedCandies = new Dictionary<CandyType, int>();
        matchedCandiesSet = new HashSet<Candy>();
        foreach (var goal in currentLevel.goals)
        {
            matchedCandies[goal.type] = 0;
        }
        clearedDirtTiles = new HashSet<Vector2Int>();
        clearedLockedTiles = new HashSet<Vector2Int>();
        Debug.Log($"LoadLevel: level={currentLevel.level}, swapLimit={currentLevel.swapLimit}, goals={currentLevel.goals.Count}");
    }

    public void OnSwap()
    {
        remainingSwaps--;
        Debug.Log($"OnSwap: remainingSwaps={remainingSwaps}");
    }

    public void OnCandyMatched(Candy candy)
    {
        if (candy == null || matchedCandiesSet.Contains(candy))
        {
            Debug.LogWarning($"Candy skipped: id={candy?.GetInstanceID()}, alreadyMatched={matchedCandiesSet.Contains(candy)}");
            return;
        }
        matchedCandiesSet.Add(candy);
        if (matchedCandies.ContainsKey(candy.type))
        {
            matchedCandies[candy.type]++;
            Debug.Log($"OnCandyMatched: type={candy.type}, total={matchedCandies[candy.type]}, candy={candy.GetInstanceID()}, pos=({candy.row},{candy.column})");
        }
    }

    public void OnDirtTileCleared(Vector2Int pos)
    {
        clearedDirtTiles.Add(pos);
        TileManager.Ins.RemoveDirtTile(pos);
        Debug.Log($"Dirt tile cleared: {pos}, total={clearedDirtTiles.Count}/{currentLevel.dirtTiles.Count}");
    }

    public void OnLockedTileCleared(Vector2Int pos)
    {
        clearedLockedTiles.Add(pos);
        TileManager.Ins.RemoveLockedTile(pos);
        Debug.Log($"Locked tile cleared: {pos}, total={clearedLockedTiles.Count}/{currentLevel.lockedTiles.Count}");
    }

    public bool CheckWin()
    {
        bool dirtGoalMet = !currentLevel.clearAllDirtTiles || clearedDirtTiles.Count >= currentLevel.dirtTiles.Count;
        bool lockGoalMet = !currentLevel.clearAllLockedTiles || clearedLockedTiles.Count >= currentLevel.lockedTiles.Count;
        bool candyGoalMet = currentLevel.goals.TrueForAll(goal => matchedCandies[goal.type] >= goal.count);
        bool result = dirtGoalMet && lockGoalMet && candyGoalMet;
      //  Debug.Log($"CheckWin: dirtGoalMet={dirtGoalMet}, lockGoalMet={lockGoalMet}, candyGoalMet={candyGoalMet}, result={result}, matchedCandies=[{string.Join(", ", matchedCandies.Select(kv => $"{kv.Key}:{kv.Value}"))}]");
        return result;
    }

    public bool CheckLose()
    {
        bool result = remainingSwaps <= 0 && !CheckWin();
        Debug.Log($"CheckLose: remainingSwaps={remainingSwaps}, result={result}");
        return result;
    }
}