using Managers;
using Rat_P;
using TMPro;
using UnityEngine;

public class GameEnd : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    [SerializeField] private GameObject _GameEndMenu;
    [SerializeField] private GameObject _buttonReplay;
    [SerializeField] private GameObject _textGameScore;
    
    void Start()
    {
        if (_buttonReplay)
        {
            _buttonReplay.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnButtonbReplayClicked);
            
        }
        if (_textGameScore)
        {
            // get the TMP from the textGameScore and set the text to the current score
            TMP_Text tmpText = _textGameScore.GetComponent<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = GameStateManager.Instance.GetCurrentScore().ToString();
            }
            else
            {
                Debug.LogError("TMP_Text component not found on _textGameScore GameObject.");
            }
            
        }
        
        UAudio.Instance.PlayMenuMusic(0.5f);
    }


    private void OnButtonbReplayClicked()
    {
        // Reload the current scene to restart the game
        UAudio.Instance.PlayRATP_ButtonSuccessSound();
        UAudio.Instance.StopMenuMusic();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scenes/MainMenu");
    }
}
