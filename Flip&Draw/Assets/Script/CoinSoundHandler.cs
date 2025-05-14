using System.Collections.Generic;
using UnityEngine;


public class CoinSoundHandler : MonoBehaviour
{
    // Fields

    [SerializeField] private List<AudioClip> _placingSounds = new List<AudioClip>();
    [SerializeField] private AudioClip _flippingSound;

    private AudioSource _audioSource;


    // Public methods

    public void PlayCoinPlaceSound()
    {
        var index = getRandomFrom(0, _placingSounds.Count);

        _audioSource.clip = _placingSounds[index];
        _audioSource.Play();
    }

    public void PlayCoinFlipSound()
    {
        _audioSource.clip = _flippingSound;
        _audioSource.Play();
    }


    // Private methods

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private int getRandomFrom(int min, int max)
    {
        Random.InitState(Time.frameCount);
        return Random.Range(min, max);
    }
}
