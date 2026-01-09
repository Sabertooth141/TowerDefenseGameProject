using UnityEngine;

namespace Entity.Player
{
    public class RecoilController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraController camController;
        
        [Header("Recoil Settings")]
        [SerializeField] private float verticalRecoil = 1.2f;
        [SerializeField] private float horizontalRecoil = 0.4f;
        
        [Header("Auto Fire")]
        [SerializeField] private float maxVerticalRecoil = 12f;
        [SerializeField] private float recoilMultiplier = 1.05f;

        private float _currRecoil = 1;

        public void ApplyRecoil()
        {
            _currRecoil *= recoilMultiplier;
            _currRecoil = Mathf.Min(_currRecoil, maxVerticalRecoil);
            
            float vertical = verticalRecoil * _currRecoil;
            float horizontal = Random.Range(-horizontalRecoil, horizontalRecoil);
            
            camController.recoilOffset += new Vector2(horizontal, vertical);
        }

        public void ResetRecoil()
        {
            _currRecoil = 1;
        }
    }
}
