using UnityEngine;
using System.Collections.Generic;

public class TileManager : Singleton<TileManager>
{
    public void Initialize()
    {
        Debug.Log("TileManager initialized");
    }

    public void ProcessMatchEffects(List<Candy> matches)
    {
        var levelManager = LevelManager.Ins;
        foreach (var candy in matches)
        {
            if (candy == null) continue;
            Vector2Int pos = new Vector2Int(candy.row, candy.column);
            if (levelManager.currentLevel.dirtTiles.Exists(t => t == pos))
            {
                levelManager.OnDirtTileCleared(pos);
            }
            if (levelManager.currentLevel.lockedTiles.Exists(t => t == pos))
            {
                levelManager.OnLockedTileCleared(pos);
            }
        }
        Debug.Log($"ProcessMatchEffects: matches={matches.Count}, swapsLeft={levelManager.remainingSwaps}");
    }

    public void RemoveDirtTile(Vector2Int pos)
    {
        GridManager.Ins.RemoveDirtTile(pos);
    }

    public void RemoveLockedTile(Vector2Int pos)
    {
        GridManager.Ins.RemoveLockedTile(pos);
    }
}