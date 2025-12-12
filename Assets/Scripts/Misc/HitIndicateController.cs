using UnityEngine;

namespace Misc
{
    public class HitIndicateController : MonoBehaviour
    {
        public float timeToLive = 2f;

        private float _liveTimer;
        // Update is called once per frame
        void Update()
        {
            _liveTimer += Time.deltaTime;
            if (_liveTimer >= timeToLive)
            {
                Destroy(gameObject);
            }
        }
    }

}