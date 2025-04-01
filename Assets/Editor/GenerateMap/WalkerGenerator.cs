using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WalkerGenerator
{
    public enum Grid
    {
        FLOOR,
        WALL,
        EMPTY
    }

    public Grid[,] gridHandler;
    public List<WalkerObject> Walkers;

    [System.Serializable]
    public class TilemapConfig
    {
        public Tilemap tilemap;
        public Grid type;
        public bool useRandomTile;
        public Tile specificTile;
        public List<Tile> tileOptions;
    }
    public List<TilemapConfig> tilemapConfigs;

    public int MapWidth = 30;
    public int MapHeight = 30;
    public int MaximumWalkers = 10;
    public int TileCount = 0;
    public float FillPercentage = 0.4f;
    public int Seed = 0;

    private bool isGenerating = false;
    private bool isGeneratingFloors = false;
    private bool isGeneratingWalls = false;
    private bool isFillingEmpty = false;

    public void StartGenerate()
    {
        UnityEngine.Random.InitState(Seed);
        foreach (var config in tilemapConfigs)
        {
            config.tilemap.ClearAllTiles();
        }
        InitializeGrid();
        isGenerating = true;
        isGeneratingFloors = true;
        isGeneratingWalls = false;
        isFillingEmpty = false;
    }

    public bool GenerateStep()
    {
        if (!isGenerating) return false;

        if (isGeneratingFloors)
        {
            GenerateFloorStep();
        }
        else if (isGeneratingWalls)
        {
            GenerateWallStep();
        }
        else if (isFillingEmpty)
        {
            FillEmptyTiles();
            isFillingEmpty = false;
            isGenerating = false;
        }

        return isGenerating;
    }

    void FillEmptyTiles()
    {
        for (int x = 0; x < gridHandler.GetLength(0); x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1); y++)
            {
                if (gridHandler[x, y] == Grid.EMPTY)
                {
                    SetTile(new Vector3Int(x, y, 0), Grid.EMPTY);
                }
            }
        }
    }

    void InitializeGrid()
    {
        gridHandler = new Grid[MapWidth, MapHeight];
        for (int x = 0; x < gridHandler.GetLength(0); x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1); y++)
            {
                gridHandler[x, y] = Grid.EMPTY;
            }
        }

        Walkers = new List<WalkerObject>();
        Vector3Int TileCenter = new Vector3Int(MapWidth / 2, MapHeight / 2, 0);
        WalkerObject curWalker = new WalkerObject(new Vector2(TileCenter.x, TileCenter.y), GetDirection(), 0.5f);
        gridHandler[TileCenter.x, TileCenter.y] = Grid.FLOOR;
        SetTile(TileCenter, Grid.FLOOR);
        Walkers.Add(curWalker);
        TileCount = 1;
    }

    Vector2 GetDirection()
    {
        int choice = Mathf.FloorToInt(UnityEngine.Random.value * 3.99f);
        switch (choice)
        {
            case 0: return Vector2.down;
            case 1: return Vector2.left;
            case 2: return Vector2.up;
            case 3: return Vector2.right;
            default: return Vector2.zero;
        }
    }

    void GenerateFloorStep()
    {
        if ((float)TileCount / (float)gridHandler.Length < FillPercentage)
        {
            foreach (WalkerObject curWalker in Walkers)
            {
                Vector3Int curPos = new Vector3Int((int)curWalker.Position.x, (int)curWalker.Position.y, 0);
                if (gridHandler[curPos.x, curPos.y] != Grid.FLOOR)
                {
                    SetTile(curPos, Grid.FLOOR);
                    TileCount++;
                    gridHandler[curPos.x, curPos.y] = Grid.FLOOR;
                }
            }

            ChanceToRemove();
            ChanceToRedirect();
            ChanceToCreate();
            UpdatePosition();
        }
        else
        {
            isGeneratingFloors = false;
            isGeneratingWalls = true;
        }
    }

    void GenerateWallStep()
    {
        for (int x = 0; x < gridHandler.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1) - 1; y++)
            {
                if (gridHandler[x, y] == Grid.FLOOR)
                {
                    if (gridHandler[x + 1, y] == Grid.EMPTY)
                    {
                        gridHandler[x + 1, y] = Grid.WALL;
                        SetTile(new Vector3Int(x + 1, y, 0), Grid.WALL);
                    }
                    if (x > 0 && gridHandler[x - 1, y] == Grid.EMPTY)
                    {
                        gridHandler[x - 1, y] = Grid.WALL;
                        SetTile(new Vector3Int(x - 1, y, 0), Grid.WALL);
                    }
                    if (gridHandler[x, y + 1] == Grid.EMPTY)
                    {
                        gridHandler[x, y + 1] = Grid.WALL;
                        SetTile(new Vector3Int(x, y + 1, 0), Grid.WALL);
                    }
                    if (y > 0 && gridHandler[x, y - 1] == Grid.EMPTY)
                    {
                        gridHandler[x, y - 1] = Grid.WALL;
                        SetTile(new Vector3Int(x, y - 1, 0), Grid.WALL);
                    }
                }
            }
        }

        isGeneratingWalls = false;
        isFillingEmpty = true;
    }

    void SetTile(Vector3Int position, Grid type)
    {
        foreach (var config in tilemapConfigs)
        {
            if (config.type == type && config.tilemap != null)
            {
                Tile tileToSet;
                if (config.useRandomTile && config.tileOptions.Count > 0)
                {
                    int tileIndex = UnityEngine.Random.Range(0, config.tileOptions.Count);
                    tileToSet = config.tileOptions[tileIndex];
                }
                else
                {
                    tileToSet = config.specificTile;
                }

                if (tileToSet != null)
                {
                    config.tilemap.SetTile(position, tileToSet);
                }
                break;
            }
        }
    }

    void ChanceToRemove()
    {
        int updatedCount = Walkers.Count;
        for (int i = 0; i < updatedCount; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange && Walkers.Count > 1)
            {
                Walkers.RemoveAt(i);
                break;
            }
        }
    }

    void ChanceToRedirect()
    {
        for (int i = 0; i < Walkers.Count; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange)
            {
                WalkerObject curWalker = Walkers[i];
                curWalker.Direction = GetDirection();
                Walkers[i] = curWalker;
            }
        }
    }

    void ChanceToCreate()
    {
        int updatedCount = Walkers.Count;
        for (int i = 0; i < updatedCount; i++)
        {
            if (UnityEngine.Random.value < Walkers[i].ChanceToChange && Walkers.Count < MaximumWalkers)
            {
                Vector2 newDirection = GetDirection();
                Vector2 newPosition = Walkers[i].Position;
                WalkerObject newWalker = new WalkerObject(newPosition, newDirection, 0.5f);
                Walkers.Add(newWalker);
            }
        }
    }

    void UpdatePosition()
    {
        for (int i = 0; i < Walkers.Count; i++)
        {
            WalkerObject FoundWalker = Walkers[i];
            FoundWalker.Position += FoundWalker.Direction;
            FoundWalker.Position.x = Mathf.Clamp(FoundWalker.Position.x, 1, gridHandler.GetLength(0) - 2);
            FoundWalker.Position.y = Mathf.Clamp(FoundWalker.Position.y, 1, gridHandler.GetLength(1) - 2);
            Walkers[i] = FoundWalker;
        }
    }
}