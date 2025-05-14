using UnityEngine;


public class GameDirector : MonoBehaviour
{
    // Fields

    [SerializeField] private Board _board;

    private bool _playerSelector = false;
    private bool _isGameOver = false;


    // Public methods

    public bool IsGameOver() => _isGameOver;


    // Private methods

    private void Update()
    {
        if (!_isGameOver)
            if (_board.CanPlay())
            {
                if (_board.UpdateEligiblePositions(getFace()) && !_board.IsFull())
                {
                    if (getInput() && _board.PlaceCoinOnBoard(getFace()))
                    {
                        _playerSelector = !_playerSelector;
                    }
                }
                else
                    _isGameOver = true;
            }
    }

    private bool getInput()
    {
        return Input.GetMouseButtonDown(0);
    }

    private CoinFace getFace()
    {
        return _playerSelector ? CoinFace.white : CoinFace.black;
    }
}