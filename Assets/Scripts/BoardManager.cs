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
  
   public int Width;
   public int Height;
   public Tile[] GroundTiles;
   public Tile[] WallTiles;
   public FoodObject FoodPrefab;
   public FoodObject MeetFoodPrefab;
   public WallObject WallPrefab;

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
   }
  
   public void Init()
   {
       m_Tilemap = GetComponentInChildren<Tilemap>();
       m_Grid = GetComponentInChildren<Grid>();
       m_EmptyCellsList = new List<Vector2Int>();
      
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
       GenerateWall();
       GenerateFood();
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
   void AddObject(CellObject obj, Vector2Int coord)
        {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.SetBoard(this);
        obj.Init(coord);
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
               return;
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

  void GenerateWall()
    {
    int wallCount = Random.Range(6, 10);
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
