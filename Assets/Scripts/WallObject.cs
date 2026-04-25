using UnityEngine;
using UnityEngine.Tilemaps;

public class WallObject : CellObject
{
   public Tile ObstacleTile;
   public int Health = 3;
   public int DamagePerHit = 1;
  
   public override void Init(Vector2Int cell)
   {
       base.Init(cell);
       GameManager.Instance.BoardManager.SetCellTile(cell, ObstacleTile);
   }

   public override bool PlayerBumped()
   {
       Health -= DamagePerHit;

       if (Health <= 0)
       {
           GameManager.Instance.BoardManager.RestoreCellTile(m_Cell);
           DestroySelf();
           return true;
       }

       return false;
   }

   public override bool PlayerWantsToEnter()
   {
       return false;
   }
}
