
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TowerGun : Tower
{
    //Networked
    [HideInInspector, UdonSynced]
    public int localPowerAmount = 0;

    //

    [HideInInspector]
    public float timer = 0f;

    public Text chargeDisplay;

    public Transform gunRotate;

    [HideInInspector]
    public float gunVelocity = 100f;

    [HideInInspector]
    public Vector3 gunAsteroidVector;

    [HideInInspector]
    public float maxShootDistance;


    public int currentIndex = 0;
    public Asteroid currentAsteroid;


    public void ChargeGun()
    {
        if (chargeDisplay != null)
        {
            chargeDisplay.text = localPowerAmount.ToString();
        }

        if (powerAmount > 0)
        {
            if (localPowerAmount < maxPowerAmount)
            {
                localPowerAmount += 1;
                powerAmount -= 1;
                if (Networking.IsOwner(gameObject))
                {
                    RequestSerialization();
                }
            }
        }
    }

    public void FindAnAsteroidToFireAt()
    {
        for (int i = 0; i < towerManager.asteroids.asteroidPool.Length; i++)
        {
            int index = (i + currentIndex) % towerManager.asteroids.asteroidPool.Length;
            if (towerManager.asteroids.asteroidPool[index].GetActive()) //if in game
            {
                gunAsteroidVector = transform.position - towerManager.asteroids.asteroidPool[index].transform.position;
                //is asteroid in sky above horizon ie above gun
                if (Vector3.Dot(upVector, gunAsteroidVector.normalized) < 0f) //upVector points down?
                {
                    if ((gunAsteroidVector).sqrMagnitude < maxShootDistance)
                    {
                        currentAsteroid = towerManager.asteroids.asteroidPool[index];
                        currentIndex = index;
                        return;
                    }
                }
            }
        }
    }
}
