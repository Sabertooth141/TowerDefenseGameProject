using System;
using GameEvents;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;
using Random = UnityEngine.Random;

namespace Entity.Player
{
    public class WeaponSFXController : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource gunshot;

        public void StartFiring()
        {
            if (gunshot.isPlaying)
            {
                gunshot.Stop();
            }
            
            gunshot.pitch = Random.Range(0.8f, 1.2f);
            gunshot.Play();
        }
    }
}
