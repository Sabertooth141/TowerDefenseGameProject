using UnityEngine;
using UnityEngine.Serialization;

namespace Entity.Player
{
    public class WeaponSFXController : MonoBehaviour
    {
        [FormerlySerializedAs("fireLoop")]
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
