
using System;
using System.Reflection;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TowerRailGun : TowerGun
{
    float timerLaser = 0f;
    


    [SerializeField]
    LayerMask layerAsteroids;

    [SerializeField]
    GameObject laser;

    [SerializeField]
    GameObject laser1;

    [SerializeField]
    GameObject laser2;

    RaycastHit[] hits;

    Asteroid asteroid;

    float damageRailgun;


    public override void OverwritableUpdate()
    {
        if (!towerEnabled) return;
        timer += Time.deltaTime;

        ChargeGun();

        if (!ShotAtCurrentAsteroid())
        {
            FindAnAsteroidToFireAt();
        }


        LaserTurnoffTimer();
    }

    private bool ShotAtCurrentAsteroid()
    {
        if (currentAsteroid != null)
        {
            if (currentAsteroid.GetActive()) //if in game
            {
                gunAsteroidVector = transform.position - currentAsteroid.transform.position;
                //is asteroid in sky above horizon ie above gun
                if (Vector3.Dot(upVector, gunAsteroidVector.normalized) < 0.2f) //upVector points down?
                {
                    if ((gunAsteroidVector).sqrMagnitude < maxShootDistance)
                    {
                        gunRotate.LookAt(currentAsteroid.transform.position, upVector);
                        ShootIfPossible();

                        return true;
                    }
                }
            }
        }
        return false;     
    }

    private void LaserTurnoffTimer()
    {
        if (timerLaser > 0f)
        {
            timerLaser -= Time.deltaTime;
            if (timerLaser <= 0f)
            {
                laser.SetActive(false);
                laser1.SetActive(false);
                laser2.SetActive(false);
            }
        }
    }

    public void ShootIfPossible(float distance = 0f)
    {
        if (timer > SecondsToShoot)
        {
            if (localPowerAmount >= shotCostAmount)
            {
                localPowerAmount -= shotCostAmount;
                timer = 0f;

                if (UpgradeLevel == 1)
                {
                    laser.SetActive(true);
                }
                else if (UpgradeLevel == 2)
                {
                    laser1.SetActive(true);
                }
                else
                {
                    laser2.SetActive(true);
                }

                DamageAsteroids();
                timerLaser = 0.3f;
            }  
        }
    }

    private void DamageAsteroids()
    {
        hits = Physics.SphereCastAll(transform.position, damageRailgun * 0.05f, gunRotate.transform.forward, 300f, layerAsteroids);
        for (int i = 0; i < hits.Length; i++)
        {
            asteroid = hits[i].collider.gameObject.GetComponent<Asteroid>();
            if (asteroid != null)
            {
                asteroid.TakeDamage(damageRailgun, 0.2f);
            }
        }
    }

   


    //--
    public override void UpgradeGun(int level)
    {
        UpgradeLevel = level;
        RequestSerialization();
    }

    public override void UpgradeLevelHasChanged(int value)
    {
        if (value == 0)
        {
            towerEnabled = false;
        }
        if (UpgradeLevel == 1)
        {
            towerEnabled = true;
            maxShootDistance = towerManager.maxShootDistanceRailgunSqrd;//use as laser width
            maxPowerAmount = towerManager.maxPowerAmountRailgun;
            gunVelocity = towerManager.velocityRailgun;
            SecondsToShoot = towerManager.SecondsToShootRailgun;
            shotCostAmount = towerManager.shotCostAmountRailgun;
            damageRailgun = towerManager.damageRailgun;
            Health = healthMax = towerManager.HealthRailgun;

            upgrade0.SetActive(true);
            upgrade1.SetActive(false);
            upgrade2.SetActive(false);

        }
        else if (UpgradeLevel == 2)
        {
            towerEnabled = true;
            maxShootDistance = towerManager.maxShootDistanceRailgun1Sqrd;
            maxPowerAmount = towerManager.maxPowerAmountRailgun1;
            gunVelocity = towerManager.velocityRailgun1;
            SecondsToShoot = towerManager.SecondsToShootRailgun1;
            shotCostAmount = towerManager.shotCostAmountRailgun1;
            damageRailgun = towerManager.damageRailgun1;
            Health = healthMax = towerManager.HealthRailgun1;

            upgrade0.SetActive(false);
            upgrade1.SetActive(true);
            upgrade2.SetActive(false);
        }
        else if (UpgradeLevel == 3)
        {
            towerEnabled = true;
            maxShootDistance = towerManager.maxShootDistanceRailgun2Sqrd;
            maxPowerAmount = towerManager.maxPowerAmountRailgun2;
            gunVelocity = towerManager.velocityRailgun2;
            SecondsToShoot = towerManager.SecondsToShootRailgun2;
            shotCostAmount = towerManager.shotCostAmountRailgun2;
            damageRailgun = towerManager.damageRailgun2;
            Health = healthMax = towerManager.HealthRailgun2;

            upgrade0.SetActive(false);
            upgrade1.SetActive(false);
            upgrade2.SetActive(true);
        }
    }

    public override int UpgradeCost()
    {
        int cost = UpgradeLevel == 1 ? towerManager.towerCostRailgun1 :
                       UpgradeLevel == 2 ? towerManager.towerCostRailgun2 : 0;

        return cost;
    }

    public override int SellPrice()
    {
        if (UpgradeLevel == 1)
        {
            return Mathf.RoundToInt(towerManager.towerCostRailgun * 0.8f);
        }
        else if (UpgradeLevel == 2)
        {
            return Mathf.RoundToInt((towerManager.towerCostRailgun + towerManager.towerCostRailgun1) * 0.8f);
        }
        else if (UpgradeLevel == 3)
        {
            return Mathf.RoundToInt((towerManager.towerCostRailgun + towerManager.towerCostRailgun1 +
                towerManager.towerCostRailgun2) * 0.8f);
        }
        return 0;
    }
}
