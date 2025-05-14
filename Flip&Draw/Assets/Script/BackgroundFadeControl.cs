using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class BackgroundFadeControl : MonoBehaviour
{
    [SerializeField] private GameDirector _gameDirector;
    [SerializeField] private Board _board;
    [SerializeField] private GameObject _whiteBackground;
    [SerializeField] private GameObject _blackBackground;
    [SerializeField] private GameObject _greyBackground;
    [SerializeField] private GameObject _whiteButton;
    [SerializeField] private GameObject _blackButton;
    [SerializeField] private GameObject _greyButton;

    private bool _hasTriggered = false;


    // Public methods

    public void RestartLevel() => SceneManager.LoadSceneAsync("MainLoop");

    public void GoToMainMenu() => SceneManager.LoadSceneAsync("MainMenu");


    // Private methods

    private void Update()
    {
        if (!_hasTriggered && _gameDirector.IsGameOver())
        {
            _hasTriggered = true;
            StartCoroutine(showEndScreen());
        }
    }

    private IEnumerator showEndScreen()
    {
        yield return new WaitForSeconds(1);

        var c = _board.GetCoinCount();

        if (c.x > c.y)
        {
            string player = PlayerPrefs.GetString("black-player-name");
            _blackBackground.GetComponentInChildren<TMP_Text>().text = $"{player} Wins!!!";
            _blackBackground.SetActive(true);
            _blackButton.SetActive(true);
        }
        else if (c.x < c.y)
        {
            string player = PlayerPrefs.GetString("white-player-name");
            _whiteBackground.GetComponentInChildren<TMP_Text>().text = $"{player} Wins!!!";
            _whiteBackground.SetActive(true);
            _whiteButton.SetActive(true);
        }
        else
        {
            _greyBackground.GetComponentInChildren<TMP_Text>().text = $"It's a draw";
            _greyBackground.SetActive(true);
            _greyButton.SetActive(true);
        }
    }
}