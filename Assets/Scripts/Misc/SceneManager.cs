using System;
using EventSystem;
using UnityEngine;

namespace Misc
{
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            EventHub.TriggerGameStart();
        }

    }
}