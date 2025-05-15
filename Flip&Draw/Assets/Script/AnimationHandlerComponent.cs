using System.Collections.Generic;
using UnityEngine;


namespace Lacobus.Animation
{
    public sealed class AnimationHandlerComponent : MonoBehaviour
    {
        // Fields

#if UNITY_EDITOR
        [SerializeField] private int _selection = 0;
#endif

        [SerializeField] private string _animationClipSource = "";
        [SerializeField] private string _animatorControllerSource = "";
        [SerializeField] private ClipData[] _clipDataArray;
        [SerializeField] private Animator _animator = null;

        private int _currentClipIndex = 0;


        // Public methods

        /// <summary>
        /// Method to play an animation
        /// </summary>
        /// <param name="clipName">Target animation clip to be played</param>
        public void PlayState(string clipName)
        {
            tryClip(clipName);
        }

        /// <summary>
        /// Plays animation if the condition is true
        /// </summary>
        /// <param name="condition">Should be true to play animation</param>
        /// <param name="clipName">Target animation clip to be played</param>
        public void PlayStateIf(bool condition, string clipName)
        {
            if (condition)
                tryClip(clipName);
        }

        /// <summary>
        /// Returns all clip names in for this animation handler
        /// </summary>
        public string[] GetAllClipNames()
        {
            List<string> clipNames = new List<string>();
            foreach (var c in _clipDataArray)
                clipNames.Add(c.clipName);
            return clipNames.ToArray();
        }


        // Private methods

        private void tryClip(string newClip)
        {
            if (_animator == null)
                return;

            if (_clipDataArray[_currentClipIndex].clipName == newClip)
            {
                if (_clipDataArray[_currentClipIndex].isLooping)
                    return;
            }
            else
            {
                int newIndex = getValidIndex(newClip);
                if (newIndex == -1)
                    return;

                _currentClipIndex = newIndex;
            }

            _animator.Play(_clipDataArray[_currentClipIndex].clipName);
        }

        private int getValidIndex(string clipName)
        {
            for (int i = 0; i < _clipDataArray.Length; i++)
                if (_clipDataArray[i].clipName == clipName)
                    return i;
            return -1;
        }


        // Nested types

        [System.Serializable]
        private class ClipData
        {
            public string clipName;
            public bool isLooping;
        }
    }
}