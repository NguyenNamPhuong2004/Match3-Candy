using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Match3/LevelData")]
public class LevelData : ScriptableObject
{
    public int level;
    public int swapLimit;
    public List<CandyGoal> goals;
    public List<Vector2Int> dirtTiles;
    public List<Vector2Int> lockedTiles;
    public List<Vector2Int> emptyTiles;
    public bool clearAllDirtTiles;
    public bool clearAllLockedTiles;
}

[System.Serializable]
public class CandyGoal
{
    public CandyType type;
    public int count;
}