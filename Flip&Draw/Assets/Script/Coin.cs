using Lacobus.Animation;
using UnityEngine;


public class Coin : MonoBehaviour
{
    // Fields

    [SerializeField] private CoinFace _currentFace;

    private AnimationHandlerComponent _animationHandler;
    private CoinSoundHandler _soundHandler;


    // Public methods

    public void FlipFace()
    {
        switch (_currentFace)
        {
            case CoinFace.black:
                _currentFace = CoinFace.white;
                break;
            case CoinFace.white:
                _currentFace = CoinFace.black;
                break;
        }

        updateRenderer();
        playSound();
    }

    public CoinFace GetFace()
    {
        return _currentFace;
    }


    // Private methods

    private void Awake()
    {
        _animationHandler = GetComponent<AnimationHandlerComponent>();
        _soundHandler = GetComponent<CoinSoundHandler>();
    }

    private void Start()
    {
        _soundHandler.PlayCoinPlaceSound();
    }

    private void updateRenderer()
    {
        switch (_currentFace)
        {
            case CoinFace.black:
                _animationHandler.PlayState("white_to_black");
                break;
            case CoinFace.white:
                _animationHandler.PlayState("black_to_white");
                break;
        }
    }

    private void playSound()
    {
        _soundHandler.PlayCoinFlipSound();
    }
}

public enum CoinFace
{
    black,
    white
}