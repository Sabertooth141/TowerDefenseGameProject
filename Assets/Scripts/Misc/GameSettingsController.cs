using UnityEngine;

namespace Misc
{
    public class GameSettingsController : MonoBehaviour
    {
        private void Awake()
        {
            Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        }
    }
}
