using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

public class GameManager : MonoBehaviour
{
   [System.Serializable]
   private class SaveData
   {
       public int Level;
       public int Food;
   }

   private const string SaveFileName = "roguelike_save.json";

   public static GameManager Instance { get; private set; }
  
   public BoardManager BoardManager;
   public PlayerController PlayerController;

   public TurnManager TurnManager { get; private set;}
   private int m_FoodAmount = 10;
   public UIDocument UIDoc;
   private Label m_FoodLabel;
   private int m_CurrentLevel = 1;
   private VisualElement m_GameOverPanel;
   private Label m_GameOverMessage;

   private void Awake()
   {
       if (Instance != null)
       {
           Destroy(gameObject);
           return;
       }
      
       Instance = this;
   }
  
   void Start()
   {
       TurnManager = new TurnManager();
       TurnManager.OnTick += OnTurnHappen;

       m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
    //    m_FoodLabel.text = "Food : " + m_FoodAmount;

       m_GameOverPanel = UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
       m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");

       LoadOrStartNewGame();
      
   }

    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }

    private void SaveProgress()
    {
        SaveData saveData = new SaveData
        {
            Level = m_CurrentLevel,
            Food = m_FoodAmount
        };

        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(SavePath, json);
    }

    private bool TryLoadProgress()
    {
        if (!HasSaveFile())
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            if (saveData == null)
            {
                return false;
            }

            m_CurrentLevel = Mathf.Max(1, saveData.Level);
            m_FoodAmount = Mathf.Max(0, saveData.Food);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Failed to load save data, starting a new game instead: " + ex.Message);
            return false;
        }
    }

    private void DeleteSaveFile()
    {
        if (!HasSaveFile())
        {
            return;
        }

        try
        {
            File.Delete(SavePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Failed to delete save data: " + ex.Message);
        }
    }

    private void PrepareNewRun()
    {
        m_GameOverPanel.style.visibility = Visibility.Hidden;

        m_FoodLabel.text = "Food : " + m_FoodAmount;
        
        BoardManager.Clean();
        BoardManager.ConfigureForLevel(m_CurrentLevel);
        BoardManager.Init();
        
        PlayerController.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1,1));
    }

    public void StartNewGame()
    {
        DeleteSaveFile();
        m_CurrentLevel = 1;
        m_FoodAmount = 20;
        PrepareNewRun();
    }

    private void LoadOrStartNewGame()
    {
        if (!TryLoadProgress())
        {
            m_CurrentLevel = 1;
            m_FoodAmount = 20;
        }

        PrepareNewRun();
    }

   void OnTurnHappen()
    {
        ChangeFood(-1);
        BoardManager.MoveEnemiesTowardsPlayer(PlayerController.CellPosition);
    }
   public void ChangeFood(int amount)
    {
        m_FoodAmount += amount;
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        if (m_FoodAmount <= 0)
        {
            PlayerController.GameOver();
            DeleteSaveFile();
            m_GameOverPanel.style.visibility = Visibility.Visible;
            m_GameOverMessage.text = "Game Over!\n\nYou traveled through " + m_CurrentLevel + " levels";

        }
    }

    public void NewLevel()
    {
    m_CurrentLevel++;
    BoardManager.Clean();
    BoardManager.ConfigureForLevel(m_CurrentLevel);
    BoardManager.Init();
    PlayerController.Spawn(BoardManager, new Vector2Int(1,1));
    SaveProgress();
    }
}
