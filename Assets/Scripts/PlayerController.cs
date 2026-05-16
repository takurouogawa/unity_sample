using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
   private BoardManager m_Board;
   private Vector2Int m_CellPosition;
   private bool m_IsGameOver;
   private bool m_IsMoving;
   private Vector3 m_MoveTarget;
   private CellObject m_PendingEnteredObject;

   [SerializeField]
   private float MoveSpeed = 4f;

   public void Spawn(BoardManager boardManager, Vector2Int cell)
   {
       m_Board = boardManager;
       MoveTo(cell, true);
   }
  
   public void MoveTo(Vector2Int cell, bool immediate = false)
   {
        m_CellPosition = cell;

        if (immediate)
        {
            m_IsMoving = false;
            m_PendingEnteredObject = null;
            transform.position = m_Board.CellToWorld(m_CellPosition);
        }
        else
        {
            m_IsMoving = true;
            m_MoveTarget = m_Board.CellToWorld(m_CellPosition);
        }
        
        if (m_Animator != null)
        {
            m_Animator.SetBool("Moving", m_IsMoving);
        }
   }

   private void PlayAttackAnimation()
   {
       if (m_Animator != null)
       {
           m_Animator.SetTrigger("Attack");
       }
   }

   public void GameOver()
    {
    m_IsGameOver = true;
    }
    public void Init()
    {
    m_IsGameOver = false;
    }

    private Animator m_Animator;

    private void Awake()
    {
       m_Animator = GetComponent<Animator>();
    }

  
   private void Update()
   { 
       if (m_IsGameOver)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
           }
           return;
       }

       if (m_IsMoving)
       {
           transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, MoveSpeed * Time.deltaTime);
          
           if (transform.position == m_MoveTarget)
           {
               m_IsMoving = false;

               if (m_Animator != null)
               {
                   m_Animator.SetBool("Moving", false);
               }

               if (m_PendingEnteredObject != null)
               {
                   CellObject enteredObject = m_PendingEnteredObject;
                   m_PendingEnteredObject = null;
                   enteredObject.PlayerEntered();
               }
           }

           return;
       }

       Vector2Int newCellTarget = m_CellPosition;
       bool hasMoved = false;

       if(Keyboard.current.upArrowKey.wasPressedThisFrame)
       {
           newCellTarget.y += 1;
           hasMoved = true;
       }
       else if(Keyboard.current.downArrowKey.wasPressedThisFrame)
       {
           newCellTarget.y -= 1;
           hasMoved = true;
       }
       else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
       {
           newCellTarget.x += 1;
           hasMoved = true;
       }
       else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
       {
           newCellTarget.x -= 1;
           hasMoved = true;
       }

       if(hasMoved)
       {
           BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);

           if(cellData != null && cellData.Passable)
           {
                GameManager.Instance.TurnManager.Tick();

                if (cellData.ContainedObject == null)
                {
                    MoveTo(newCellTarget);
                }
                else if(cellData.ContainedObject.PlayerWantsToEnter())
                {
                    m_PendingEnteredObject = cellData.ContainedObject;
                    MoveTo(newCellTarget);
                }
                else
                {
                    PlayAttackAnimation();
                    if (cellData.ContainedObject.PlayerBumped())
                    {
                        m_PendingEnteredObject = null;
                        MoveTo(newCellTarget);
                    }
                }
           }
       }
   }

}
