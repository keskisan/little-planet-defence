
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)] //master calculates all else it network heavy. local player just eyecandy
public class Missile : Ammo
{
    [SerializeField]
    ExplosionManager explosionManager;

    [SerializeField]
    TowerManager towerManager;

    int asteroidLayer = 23;
    
    float explosionRadius = 30f;

    float distanceToTravelSqrd = 0f;
    Vector3 startPosition;

    Asteroid asteroid;

    public override void UpdateAmmo()
    {
        if (gameObject.activeSelf)
        {
            if ((transform.position - startPosition).sqrMagnitude > distanceToTravelSqrd)
            {
                Explode();
            }
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    private void Explode()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in hitColliders)
        {
            if (collider != null)
            {
                if (collider.gameObject.layer == asteroidLayer)
                {
                    asteroid = collider.gameObject.GetComponent<Asteroid>();
                    if (asteroid != null)
                    {
                        Vector3 distanceFromCentre = (transform.position - asteroid.transform.position);
                        float damage =  Mathf.Min(explosionRadius, explosionRadius / distanceFromCentre.magnitude);
                        asteroid.TakeDamage(damage * towerManager.damageScaleMissile, damage * 0.05f);
                    }

                }
            }
            
        }
        explosionManager.AddExplosion(transform.position, 0.05f);
        timer = 0f;
        gameObject.SetActive(false);
    }

    public override void FireAmmo(float startVelocity, float distance, bool localDamage = false)
    {
        distanceToTravelSqrd = distance * distance;
        startPosition = transform.position;
        rb.velocity = transform.forward * startVelocity;
    }
}
