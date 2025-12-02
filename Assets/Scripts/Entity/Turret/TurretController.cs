using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    public float rotationSpeed = 2.0f;
    public float timeToReset = 10.0f;

    [SerializeField] private Transform turretHead;
    [SerializeField] private Transform turretFiringPoint;

    private bool _hasTargets;
    private float _resetTimer;
    private GameObject _currTarget;

    private List<GameObject> _targets;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _targets = new List<GameObject>();

        if (turretHead == null)
        {
            Debug.LogError("TurretHead field is missing");
        }

        if (turretFiringPoint == null)
        {
            Debug.LogError("TurretFiringPoint field is missing");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!_hasTargets)
        {
            _resetTimer += Time.deltaTime;
            if (_resetTimer >= timeToReset)
            {
                ResetRotation();
            }

            return;
        }

        _resetTimer = 0;
        
        _currTarget = _targets[0];

        TurnToTarget();
    }

    private void ResetRotation()
    {
        Vector3 dir = Vector3.forward;

        turretHead.rotation = Quaternion.Slerp(turretHead.rotation, Quaternion.LookRotation(dir),
            rotationSpeed * Time.deltaTime);
    }

    private void TurnToTarget()
    {
        if (!_hasTargets)
        {
            return;
        }

        Vector3 dir = _currTarget.transform.position - turretFiringPoint.position;
        dir.y = 0;

        Quaternion rotAngle = Quaternion.LookRotation(dir);
        turretHead.rotation = Quaternion.Slerp(turretHead.rotation, rotAngle, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            _targets.Add(other.gameObject);
            _hasTargets = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            _targets.Remove(other.gameObject);
            _hasTargets = _targets.Count > 0;
        }
    }
}