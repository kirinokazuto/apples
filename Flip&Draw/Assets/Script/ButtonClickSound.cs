using UnityEngine;
using UnityEngine.UI;


public class ButtonClickSound : MonoBehaviour
{
    [SerializeField] private AudioClip _clip;

    private AudioSource _as;

    // Private methods

    private void OnEnable()
    {
        _as = gameObject.AddComponent<AudioSource>();
        _as.clip = _clip;
        _as.playOnAwake = false;
        gameObject.GetComponent<Button>().onClick.AddListener(() => { _as.Play(); });
    }
}
