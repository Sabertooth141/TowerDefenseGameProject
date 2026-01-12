using System;
using System.Collections;
using GameEvents;
using UnityEngine;

namespace Misc
{
    public class SFXController : MonoBehaviour
    {
        [Header("BGM")]
        [SerializeField] private AudioSource ambientBGM;
        [SerializeField] private AudioSource alarmNoise;
        [SerializeField] private AudioSource combatMusic;
        [SerializeField] private float fadInDuration = 6f;
        [SerializeField] private float fadOutDuration = 4f;

        [Header("SFX")]
        [SerializeField] private AudioSource hoverSFX;
        [SerializeField] private AudioSource hitAlert;

        private bool _hurtIgnore = true;
        
        private void Start()
        {
            EventHub.OnBGMStart += StartBGM;
            EventHub.OnBGMEnd += StopBGM;
            EventHub.OnAlarmStart += StartAlarm;
            EventHub.OnAlarmEnd += StopAlarm;
            EventHub.OnMusicAllStop += StopAllSounds;
            EventHub.OnPlayerHurt += HandleHurt;
        }

        private void OnDestroy()
        {
            EventHub.OnBGMStart -= StartBGM;
            EventHub.OnBGMEnd -= StopBGM;
            EventHub.OnAlarmStart -= StartAlarm;
            EventHub.OnAlarmEnd -= StopAlarm;
            EventHub.OnMusicAllStop -= StopAllSounds;
            EventHub.OnPlayerHurt -= HandleHurt;
        }

        public void StartBGM()
        {
            StartCoroutine(FadeIn(ambientBGM));
            StartCoroutine(FadeIn(hoverSFX));
        }

        public void StopBGM()
        {
            ambientBGM.Stop();
        }

        public void StartAlarm()
        {
            alarmNoise.Play();
            StartCoroutine(FadeIn(combatMusic));
        }

        public void StopAlarm()
        {
            StartCoroutine(FadeOut(alarmNoise));
            StartCoroutine(FadeOut(combatMusic));
        }

        private IEnumerator FadeIn(AudioSource audioSource)
        {
            float targetVol = audioSource.volume;

            audioSource.volume = 0f;
            audioSource.Play();

            float t = 0f;
            while (t <= fadInDuration)
            {
                t += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(0f, targetVol, t / fadInDuration);
                yield return null;
            }

            audioSource.volume = targetVol;
        }

        private IEnumerator FadeOut(AudioSource audioSource)
        {
            float originalVolume = audioSource.volume;
            
            float t = 0f;
            while (t <= fadOutDuration)
            {
                t += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(originalVolume, 0f, t / fadOutDuration);
                yield return null;
            }

            audioSource.volume = 0;
            audioSource.Stop();
            audioSource.volume = originalVolume;
        }
        
        private void HandleHurt(float dmg)
        {
            if (_hurtIgnore)
            {
                _hurtIgnore = false;
                return;
            }
            
            if (hitAlert.isPlaying)
            {
                hitAlert.Stop();
            }
            
            hitAlert.Play();
        }

        public void StopAllSounds()
        {
            hoverSFX.Stop();
            combatMusic.Stop();
            alarmNoise.Stop();
            ambientBGM.Stop();
        }
    }
}