using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
   public class CellData
   {
       public bool Passable;
       public Tile GroundTile;
       public CellObject ContainedObject;
   }

   private CellData[,] m_BoardData;
   private Tilemap m_Tilemap;
   private Grid m_Grid;
   private List<Vector2Int> m_EmptyCellsList;
   private List<EnemyContoroller> m_Enemies;
  
   public int Width;
   public int Height;
   public Tile[] GroundTiles;
   public Tile[] WallTiles;
   public FoodObject FoodPrefab;
   public FoodObject MeetFoodPrefab;
   public WallObject WallPrefab;
   public EnemyContoroller ScavengerPrefab;
   public int MinWallCount = 6;
   public int MaxWallCount = 10;
   public ExitCellObject ExitCellPrefab;


   public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
    m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

   public void RestoreCellTile(Vector2Int cellIndex)
   {
       CellData data = GetCellData(cellIndex);
       if (data != null && data.GroundTile != null)
       {
           SetCellTile(cellIndex, data.GroundTile);
       }
   }

   public void RemoveObject(Vector2Int cellIndex, CellObject obj)
   {
       CellData data = GetCellData(cellIndex);
       if (data != null && data.ContainedObject == obj)
       {
           data.ContainedObject = null;
       }

       EnemyContoroller enemy = obj as EnemyContoroller;
       if (enemy != null && m_Enemies != null)
       {
           m_Enemies.Remove(enemy);
       }
   }
  
   public void Init()
   {
       m_Tilemap = GetComponentInChildren<Tilemap>();
       m_Grid = GetComponentInChildren<Grid>();
       m_EmptyCellsList = new List<Vector2Int>();
       m_Enemies = new List<EnemyContoroller>();
      
       m_BoardData = new CellData[Width, Height];

       for (int y = 0; y < Height; ++y)
       {
           for(int x = 0; x < Width; ++x)
           {
               Tile tile;
               m_BoardData[x, y] = new CellData();

               if(x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
               {
                   tile = WallTiles[Random.Range(0, WallTiles.Length)];
                   m_BoardData[x, y].Passable = false;
               }
               else
               {
                   tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                   m_BoardData[x, y].Passable = true;

                   //this is a passable empty cell, add it to the list!
                   m_EmptyCellsList.Add(new Vector2Int(x, y));
               }

               m_BoardData[x, y].GroundTile = tile;

               m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
           }
       }
       m_EmptyCellsList.Remove(new Vector2Int(1, 1));

       Vector2Int endCoord = new Vector2Int(Width - 2, Height - 2);
       if (ExitCellPrefab != null)
       {
           AddObject(Instantiate(ExitCellPrefab), endCoord);
           m_EmptyCellsList.Remove(endCoord);
       }
       else
       {
           Debug.LogWarning("ExitCellPrefab is not assigned, so no exit cell will be spawned.");
       }


       GenerateWall();
       GenerateFood();
       GenerateScavenger();
   }

   public Vector3 CellToWorld(Vector2Int cellIndex)
   {
       return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
   }

   public CellData GetCellData(Vector2Int cellIndex)
   {
       if (cellIndex.x < 0 || cellIndex.x >= Width
           || cellIndex.y < 0 || cellIndex.y >= Height)
       {
           return null;
       }

       return m_BoardData[cellIndex.x, cellIndex.y];
   }

   public void Clean()
    {
        //no board data, so exit early, nothing to clean
    if(m_BoardData == null)
        return;

    for (int y = 0; y < Height; ++y)
    {
        for (int x = 0; x < Width; ++x)
        {
            var cellData = m_BoardData[x, y];

            if (cellData.ContainedObject != null)
            {
                //CAREFUL! Destroy the GameObject NOT just cellData.ContainedObject
                //Otherwise what you are destroying is the JUST CellObject COMPONENT
                //and not the whole gameobject with sprite
                Destroy(cellData.ContainedObject.gameObject);
            }

            SetCellTile(new Vector2Int(x,y), null);
        }
    }

    if (m_Enemies != null)
    {
        m_Enemies.Clear();
    }
    }

   void AddObject(CellObject obj, Vector2Int coord)
        {
       CellData data = m_BoardData[coord.x, coord.y];
       obj.transform.position = CellToWorld(coord);
       data.ContainedObject = obj;
       obj.SetBoard(this);
       obj.Init(coord);

       EnemyContoroller enemy = obj as EnemyContoroller;
       if (enemy != null)
       {
           m_Enemies.Add(enemy);
       }
        }
   void GenerateFood()
    {
       int foodCount = 5;
       for (int i = 0; i < foodCount; ++i)
       {
           int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
           Vector2Int coord = m_EmptyCellsList[randomIndex];

           m_EmptyCellsList.RemoveAt(randomIndex);

           FoodObject foodPrefab = GetRandomFoodPrefab();
           if (foodPrefab == null)
           {
               Debug.LogWarning("Food prefabs are not assigned, so no food will be spawned.");
               break;
           }

           FoodObject newFood = Instantiate(foodPrefab);
           AddObject(newFood, coord);
       }
    }

   FoodObject GetRandomFoodPrefab()
   {
       if (FoodPrefab != null && MeetFoodPrefab != null)
       {
           return Random.Range(0, 2) == 0 ? FoodPrefab : MeetFoodPrefab;
       }

       if (FoodPrefab != null)
       {
           return FoodPrefab;
       }

       return MeetFoodPrefab;
   }

   void GenerateScavenger()
   {
       if (ScavengerPrefab == null || m_EmptyCellsList.Count == 0)
       {
           return;
       }

       int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
       Vector2Int coord = m_EmptyCellsList[randomIndex];

       m_EmptyCellsList.RemoveAt(randomIndex);

       EnemyContoroller scavenger = Instantiate(ScavengerPrefab);
       AddObject(scavenger, coord);
   }

   public void MoveEnemiesTowardsPlayer(Vector2Int playerCell)
   {
       if (m_Enemies == null || m_Enemies.Count == 0)
       {
           return;
       }

       for (int i = m_Enemies.Count - 1; i >= 0; --i)
       {
           EnemyContoroller enemy = m_Enemies[i];
           if (enemy == null)
           {
               m_Enemies.RemoveAt(i);
               continue;
           }

           Vector2Int enemyCell = enemy.Cell;
           Vector2Int delta = playerCell - enemyCell;
           if (delta == Vector2Int.zero)
           {
               continue;
           }

           if (Mathf.Abs(delta.x) + Mathf.Abs(delta.y) == 1)
           {
               enemy.AttackPlayer();
               continue;
           }

           Vector2Int targetCell = enemyCell;

           if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
           {
               targetCell.x += delta.x > 0 ? 1 : -1;
           }
           else
           {
               targetCell.y += delta.y > 0 ? 1 : -1;
           }

           MoveEnemyIfPossible(enemy, targetCell, playerCell);
       }
   }

   private bool MoveEnemyIfPossible(EnemyContoroller enemy, Vector2Int targetCell, Vector2Int playerCell)
   {
       if (targetCell == playerCell)
       {
           return false;
       }

       CellData targetData = GetCellData(targetCell);
       if (targetData == null || !targetData.Passable || targetData.ContainedObject != null)
       {
           return false;
       }

       Vector2Int fromCell = enemy.Cell;
       CellData fromData = GetCellData(fromCell);
       if (fromData != null && fromData.ContainedObject == enemy)
       {
           fromData.ContainedObject = null;
       }

       targetData.ContainedObject = enemy;
       enemy.SetCell(targetCell);
       enemy.transform.position = CellToWorld(targetCell);
       return true;
   }

   void GenerateWall()
    {
    if (WallPrefab == null)
    {
        Debug.LogWarning("WallPrefab is not assigned, so no walls will be spawned.");
        return;
    }

    int minWallCount = Mathf.Min(MinWallCount, MaxWallCount);
    int maxWallCount = Mathf.Max(MinWallCount, MaxWallCount);
    int wallCount = Random.Range(minWallCount, maxWallCount + 1);
    wallCount = Mathf.Min(wallCount, m_EmptyCellsList.Count);

    for (int i = 0; i < wallCount; ++i)
    {
        int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
        Vector2Int coord = m_EmptyCellsList[randomIndex];

        m_EmptyCellsList.RemoveAt(randomIndex);
        WallObject newWall = Instantiate(WallPrefab);

        AddObject(newWall, coord);
    }
    }
}
