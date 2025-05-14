using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _exitButton;

    private void Awake()
    {
        _startButton.onClick.AddListener(() => { SceneManager.LoadScene("PlayerSelect"); });
        _exitButton.onClick.AddListener(() => { Application.Quit(); });
    }
}