using Managers;
using Rat_P;
using UnityEngine;

public class MainMenu : MonoBehaviour
{

    // THis script is attached to the main menu canvas, and is responsible for starting the game
    
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _buttonStartGame;
    [SerializeField] private GameObject _buttonQuitGame;
    
    
    void Start()
    {
        // Add listeners to the buttons
        if (_buttonStartGame == null || _buttonQuitGame == null)
        {
            Debug.LogError("Buttons are not assigned in the inspector.");
            return;
        }
        _buttonStartGame.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(StartGame);
        _buttonQuitGame.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(QuitGame);
        _buttonStartGame.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnButtonHover(_buttonStartGame.GetComponent<UnityEngine.UI.Button>()));
        _buttonQuitGame.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnButtonHover(_buttonQuitGame.GetComponent<UnityEngine.UI.Button>()));
    }
    
    
    public void OnButtonHover(UnityEngine.UI.Button button)
    {
        UAudio.Instance.PlayRATP_ButtonPressSound();
    }
    
    private void StartGame()
    {
        // Load the first level
        RatPSystem.Instance.InitializeGame();
        UAudio.Instance.PlayRATP_ButtonSuccessSound();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MiniGame_Development");
    }
    
    private void QuitGame()
    {
        // Quit the game
        UAudio.Instance.PlayRATP_ButtonSuccessSound();
        Application.Quit();
    }
}
