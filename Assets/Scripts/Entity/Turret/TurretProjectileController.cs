using System;
using UnityEngine;

public class TurretProjectileController : MonoBehaviour
{
    public float projectileSpeed = 10.0f;
    public float projectileDamage = 10.0f;
    public float projectileLifetime = 10.0f;

    private float _destroyTimer = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _destroyTimer += Time.deltaTime;
        if (_destroyTimer >= projectileLifetime)
        {
            Destroy(gameObject);
        }

        transform.position += transform.forward * (projectileSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("HIT");
        }
    }
}
