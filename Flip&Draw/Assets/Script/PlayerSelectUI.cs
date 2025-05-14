using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PlayerSelectUI : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_InputField _blackPlayerName;
    [SerializeField] private TMPro.TMP_InputField _whitePlayerName;
    [SerializeField] private Button _startGame;

    private void Awake()
    {
        PlayerPrefs.DeleteAll();
        _blackPlayerName.onEndEdit.AddListener((string val) => { PlayerPrefs.SetString("black-player-name", val); PlayerPrefs.Save(); });
        _whitePlayerName.onEndEdit.AddListener((string val) => { PlayerPrefs.SetString("white-player-name", val); PlayerPrefs.Save(); });
        _startGame.onClick.AddListener(() => { SceneManager.LoadSceneAsync("MainLoop"); });
    }
}