using UnityEngine;

public class CellObject : MonoBehaviour
{
   protected Vector2Int m_Cell;
   protected BoardManager m_Board;

   public virtual void Init(Vector2Int cell)
   {
       m_Cell = cell;
   }

   public void SetBoard(BoardManager board)
   {
       m_Board = board;
   }
   //Called when the player enter the cell in which that object is
   public virtual void PlayerEntered()
   {
      
   }
   //Called when the player tries to move into the cell but is blocked
   public virtual bool PlayerBumped()
   {
       return false;
   }
   public virtual bool PlayerWantsToEnter()
    {
    return true;
    }

   protected void RemoveFromBoard()
   {
       if (m_Board != null)
       {
           m_Board.RemoveObject(m_Cell, this);
       }
   }

   protected void DestroySelf()
   {
       RemoveFromBoard();
       Destroy(gameObject);
   }
}
