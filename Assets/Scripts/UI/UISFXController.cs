using System;
using UnityEngine;

namespace UI
{
    public class UISFXController : MonoBehaviour
    {
        [SerializeField] private AudioSource typingSFX;
        [SerializeField] private AudioSource endSFX;

        private void Start()
        {
            PlayEndSFX();
        }

        public void PlayTypingSFX()
        {
            if (typingSFX.isPlaying)
            {
                typingSFX.Stop();
            }
            
            typingSFX.Play();
        }
        
        public void PlayEndSFX()
        {
            if (endSFX == null)
            {
                return;
            }
            endSFX.Play();
        }
    }
}
