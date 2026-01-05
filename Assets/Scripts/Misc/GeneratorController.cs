using System;
using EventSystem;
using UnityEngine;

public class GeneratorController : MonoBehaviour
{
    [SerializeField] private GameObject generatorPlating;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventHub.OnGeneratorStart += HandleGeneratorStart;
    }

    private void Awake()
    {
        
    }
    
    private void HandleGeneratorStart()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
