using System;
using System.Collections;
using EventSystem;
using UnityEngine;
using UnityEngine.Serialization;

public class GeneratorController : MonoBehaviour
{
    [SerializeField] private GameObject generatorPlating;
    [SerializeField] private Light generatorLighting;
    [SerializeField] private float platingMoveSpeed = 10f;
    [SerializeField] private Transform platingOpenPosition;

    public bool IsGeneratorRunning { get; private set; }

    private Vector3 _platingClosedPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventHub.OnGeneratorStart += HandleGeneratorStart;
        EventHub.OnGeneratorTurnOff += HandleGeneratorTurnOff;
    }

    private void OnDestroy()
    {
        EventHub.OnGeneratorStart -= HandleGeneratorStart;
        EventHub.OnGeneratorTurnOff -= HandleGeneratorTurnOff;
    }

    private void Awake()
    {
        if (generatorPlating == null)
        {
            Debug.LogError("LampController: Generator plating is missing");
        }

        if (generatorLighting == null)
        {
            Debug.LogError("LampController: Generator lighting is missing");
        }

        if (platingOpenPosition == null)
        {
            Debug.LogError("LampController: Generator plating is missing");
        }

        generatorPlating.SetActive(true);
        generatorLighting.enabled = false;
        IsGeneratorRunning = false;

        _platingClosedPosition = generatorPlating.transform.position;
    }

    private void HandleGeneratorStart()
    {
        StartCoroutine(OpenGeneratorPlating());

        generatorLighting.enabled = true;
    }

    private void HandleGeneratorTurnOff()
    {
        StartCoroutine(CloseGeneratorPlating());
        
        generatorLighting.enabled = false;
    }
    
    private IEnumerator OpenGeneratorPlating()
    {
        Transform platingTransform = generatorPlating.transform;

        while (Vector3.Distance(platingTransform.position, platingOpenPosition.position) > 0.01f)
        {
            platingTransform.position = Vector3.MoveTowards(
                platingTransform.position,
                platingOpenPosition.position,
                platingMoveSpeed * Time.deltaTime
            );

            yield return null;
        }

        IsGeneratorRunning = true;
        platingTransform.position = platingOpenPosition.position;
    }

    private IEnumerator CloseGeneratorPlating()
    {
        Transform platingTransform = generatorPlating.transform;

        while (Vector3.Distance(platingTransform.position, _platingClosedPosition) > 0.01f)
        {
            platingTransform.position = Vector3.MoveTowards(platingTransform.position, _platingClosedPosition, platingMoveSpeed * Time.deltaTime);
            
            yield return null;
        }
        
        IsGeneratorRunning = false;
        platingTransform.position = _platingClosedPosition;
    }
}