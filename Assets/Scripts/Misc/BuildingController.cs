using System;
using System.Collections.Generic;
using Entity.Player;
using EventSystem;
using UnityEngine;
using UnityEngine.Serialization;

public class BuildingController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private Camera playerCam;
    [SerializeField] private Vector3 turretSize = new Vector3(2.0f, 2.0f, 2.0f);

    [Header("Placement Settings")]
    public GameObject[] turretPrefabs;
    public LayerMask groundLayer;
    public float maxSlopeAngle = 30.0f;
    public float minClearanceDistance = 5.0f;
    public float placementRange = 50.0f;

    [Header("Preview Settings")]
    public Material validMaterial;
    public Material invalidMaterial;

    private GameObject _ghostTurret;
    private List<Vector3> _placedTurretPositions;
    private bool _isValidPlacement = false;
    private Renderer[] _ghostRenderers;
    private int _selectedTurret = 0;
    private bool _isBuilding = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _placedTurretPositions = new List<Vector3>();

        if (turretPrefabs.Length == 0)
        {
            Debug.LogError("No turret prefab inputted");
        }

        if (inputReader == null)
        {
            Debug.LogError("PlayerInputReader not inputted");
        }

        if (playerCam == null)
        {
            Debug.LogError("BuildingController no PlayerCam selected");
        }

        CreateGhostTurret();
    }

    private void OnEnable()
    {
        EventHub.OnBuildingPressed += ToggleIsBuilding;
        EventHub.OnBuildingConfirmed += ConfirmBuilding;
    }

    private void OnDisable()
    {
        EventHub.OnBuildingPressed -= ToggleIsBuilding;
        EventHub.OnBuildingConfirmed -= ConfirmBuilding;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isBuilding)
        {
            UpdateGhostTurretPosition();
        }
    }

    private void CreateGhostTurret()
    {
        if (turretPrefabs.Length == 0)
        {
            return;
        }

        _ghostTurret = Instantiate(turretPrefabs[_selectedTurret]);
        _ghostTurret.name = "GhostTurret";

        foreach (Collider col in _ghostTurret.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        foreach (MonoBehaviour script in _ghostTurret.GetComponents<MonoBehaviour>())
        {
            if (script != null && script.GetType() != typeof(BuildingController))
            {
                script.enabled = false;
            }
        }

        _ghostRenderers = _ghostTurret.GetComponentsInChildren<Renderer>();
        _ghostTurret.SetActive(false);
    }

    private void UpdateGhostTurretPosition()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0.0f);

        Ray buildingRay = playerCam.ScreenPointToRay(screenCenter);
        RaycastHit buildingHit;

        if (Physics.Raycast(buildingRay, out buildingHit, placementRange, groundLayer))
        {
            _ghostTurret.SetActive(true);

            _ghostTurret.transform.position = buildingHit.point;

            Quaternion SurfaceRotation = Quaternion.FromToRotation(Vector3.up, buildingHit.normal);

            Vector3 cameraForward = playerCam.transform.forward;
            Vector3 surfaceForward = Vector3.ProjectOnPlane(cameraForward, buildingHit.normal).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(surfaceForward, buildingHit.normal);

            _ghostTurret.transform.rotation = targetRotation;

            _isValidPlacement = ValidatePlacement(buildingHit);

            UpdateGhostMaterial(_isValidPlacement);
        }
        else
        {
            _ghostTurret.SetActive(false);
            _isValidPlacement = false;
        }
    }

    private void ToggleIsBuilding()
    {
        if (_isBuilding)
        {
            _ghostTurret.SetActive(false);
        }

        _isBuilding = !_isBuilding;
    }

    private void ConfirmBuilding()
    {
        if (_isValidPlacement)
        {
            PlaceTurret();
        }
    }

    private void PlaceTurret()
    {
        GameObject turret = Instantiate(turretPrefabs[_selectedTurret], _ghostTurret.transform.position,
            _ghostTurret.transform.rotation);
        turret.name = "turret";

        _placedTurretPositions.Add(turret.transform.position);
    }

    public void ClearAllTurrets()
    {
        _placedTurretPositions.Clear();
    }

    private void UpdateGhostMaterial(bool isValidPlacement)
    {
        Material materialToUse = isValidPlacement ? validMaterial : invalidMaterial;

        if (materialToUse != null && _ghostRenderers != null)
        {
            foreach (Renderer rend in _ghostRenderers)
            {
                rend.material = materialToUse;
            }
        }
    }

    private bool ValidatePlacement(RaycastHit buildingHit)
    {
        float slopeAngle = Vector3.Angle(buildingHit.normal, Vector3.up);
        if (slopeAngle > maxSlopeAngle)
        {
            return false;
        }

        foreach (Vector3 turretPos in _placedTurretPositions)
        {
            float distance = Vector3.Distance(buildingHit.point, turretPos);
            if (distance < minClearanceDistance)
            {
                return false;
            }
        }

        Collider[] buildingColliders = Physics.OverlapBox(buildingHit.point + Vector3.up * 0.5f, turretSize / 2,
            Quaternion.LookRotation(_ghostTurret.transform.forward));
        
        foreach (Collider other in buildingColliders)
        {
            if (other.CompareTag("Ground") || other.CompareTag("Turret"))
            {
                continue;
            }

            return false;
        }

        return true;
    }
}