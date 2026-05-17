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

   public Vector2Int CellPosition => m_CellPosition;
  
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

   private bool TryGetMoveDelta(out Vector2Int moveDelta)
   {
       moveDelta = Vector2Int.zero;

       if (Keyboard.current == null)
       {
           return false;
       }

       if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
       {
           moveDelta = Vector2Int.up;
           return true;
       }

       if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
       {
           moveDelta = Vector2Int.down;
           return true;
       }

       if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
       {
           moveDelta = Vector2Int.right;
           return true;
       }

       if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
       {
           moveDelta = Vector2Int.left;
           return true;
       }

       return false;
   }

    private Animator m_Animator;

    private void Awake()
    {
       m_Animator = GetComponent<Animator>();
    }

  
   private void Update()
   { 
       if (m_Board == null)
       {
           return;
       }

       if (m_IsGameOver)
        {
            if (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.rKey.wasPressedThisFrame))
            {
                GameManager.Instance.StartNewGame();
           }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Application.Quit();
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
       bool hasMoved = TryGetMoveDelta(out Vector2Int moveDelta);
       newCellTarget += moveDelta;

       if(hasMoved)
       {
           BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);

           if(cellData != null && cellData.Passable)
           {
                if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
                {
                    GameManager.Instance.TurnManager.Tick();
                }

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
