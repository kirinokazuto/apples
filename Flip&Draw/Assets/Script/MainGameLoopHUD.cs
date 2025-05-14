using UnityEngine;
using TMPro;


public class MainGameLoopHUD : MonoBehaviour
{
    // Fields

    [SerializeField] private Board _board;
    [SerializeField] private TMP_Text _blackPlayer;
    [SerializeField] private TMP_Text _whitePlayer;

    private string _blackPlayerName;
    private string _whitePlayerName;


    // Private methods

    private void Awake()
    {
        _blackPlayerName = PlayerPrefs.GetString("black-player-name", "Black Player");
        _whitePlayerName = PlayerPrefs.GetString("white-player-name", "White Player");
    }

    private void Update()
    {
        if (_board.CanPlay())
        {
            var c = _board.GetCoinCount();
            _blackPlayer.text = $"{_blackPlayerName} : {c.x}";
            _whitePlayer.text = $"{_whitePlayerName} : {c.y}";
        }
    }
}