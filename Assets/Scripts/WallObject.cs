using UnityEngine;
using UnityEngine.Tilemaps;

public class WallObject : CellObject
{
   public Tile ObstacleTile;
   public Tile[] DamageTiles;
   public int MaxHealth = 3;
   public int DamagePerHit = 1;
   private int m_Health;
  
   public override void Init(Vector2Int cell)
   {
       base.Init(cell);
       m_Health = Mathf.Max(1, MaxHealth);
       UpdateWallTile();
   }

   public override bool PlayerBumped()
   {
       m_Health -= DamagePerHit;

       if (m_Health <= 0)
       {
           GameManager.Instance.BoardManager.RestoreCellTile(m_Cell);
           DestroySelf();
           return true;
       }

       UpdateWallTile();
       return false;
   }

   private void UpdateWallTile()
   {
       Tile tile = GetTileForCurrentHealth();
       if (tile != null)
       {
           GameManager.Instance.BoardManager.SetCellTile(m_Cell, tile);
       }
   }

   private Tile GetTileForCurrentHealth()
   {
       if (DamageTiles != null && DamageTiles.Length > 0)
       {
           int maxHealth = Mathf.Max(1, MaxHealth);
           int damageTaken = maxHealth - m_Health;
           int stage = Mathf.Clamp(
               Mathf.CeilToInt((float)damageTaken * DamageTiles.Length / maxHealth) - 1,
               0,
               DamageTiles.Length - 1);
           return DamageTiles[stage];
       }

       return ObstacleTile;
   }

   public override bool PlayerWantsToEnter()
   {
       return false;
   }
}
