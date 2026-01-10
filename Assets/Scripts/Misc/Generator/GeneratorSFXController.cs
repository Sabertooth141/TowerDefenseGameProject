using System;
using GameEvents;
using UnityEngine;

namespace Misc.Generator
{
    public class GeneratorSFXController : MonoBehaviour
    {
        [SerializeField] private AudioSource genStartup;
        [SerializeField] private AudioSource genRunning;
        [SerializeField] private AudioSource genStop;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            EventHub.OnGeneratorStarting += StartUpGenerator;
            EventHub.OnGeneratorTurnOff += StopGenerator;
        }

        private void OnDestroy()
        {
            EventHub.OnGeneratorStarting -= StartUpGenerator;
            EventHub.OnGeneratorTurnOff -= StopGenerator;
        }

        private void StartUpGenerator()
        {
            double startTime = AudioSettings.dspTime;
            
            genStartup.loop = false;
            genRunning.loop = true;
            
            genStartup.PlayScheduled(startTime);
            
            double clipDuration = genStop.clip.length;
            genRunning.PlayScheduled(clipDuration + startTime);
        }

        private void StopGenerator()
        {
            genRunning.Stop();
            
            genStop.loop = false;
            genStop.Play();
        }
    }
}
