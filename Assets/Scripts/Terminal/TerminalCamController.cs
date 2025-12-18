using System;
using UnityEngine;

namespace Terminal
{
    public class TerminalCamController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int textureWidth = 1920;
        [SerializeField] private int textureHeight = 1080;
        [SerializeField] private int textureDepth = 24;
        
        [Header("Auto assign to screen")]
        [SerializeField] private MeshRenderer screenMeshRenderer;
        [SerializeField] private string materialPropertyName = "_MainTex";

        private Camera _terminalCam;
        private RenderTexture _renderTexture;

        private void Awake()
        {
            _terminalCam = GetComponent<Camera>();
            if (_terminalCam == null)
            {
                Debug.LogError("No terminal cam found!");
            }
            
            _renderTexture = new RenderTexture(textureWidth, textureHeight, textureDepth);
            _renderTexture.name = $"RT_{gameObject.name}_{GetInstanceID()}";
            
            _terminalCam.targetTexture = _renderTexture;

            if (screenMeshRenderer != null)
            {
                Material materialInstance = new  Material(screenMeshRenderer.material);
                materialInstance.SetTexture(materialPropertyName, _renderTexture);
                screenMeshRenderer.material = materialInstance;
            }
            else
            {
                Debug.LogError($"{gameObject.name}: No screen mesh renderer found!");
            }
        }

        private void OnDestroy()
        {
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
            }
        }
    }
}