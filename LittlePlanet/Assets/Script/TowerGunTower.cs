
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TowerGunTower : TowerGun
{
    private Vector3 aimPosition;

    [Header("gun shoot positions 0")]
    [SerializeField]
    Transform posLevel0pos1;

    [Header("gun shoot positions 1")]
    [SerializeField]
    Transform posLevel1pos1;
    [SerializeField]
    Transform posLevel1pos2;

    [Header("gun shoot positions 2")]
    [SerializeField]
    Transform posLevel2pos1;
    [SerializeField]
    Transform posLevel2pos2;
    [SerializeField]
    Transform posLevel2pos3;

    int counter = 0;

    public override void OverwritableUpdate()
    {
        if (!towerEnabled) return;
        timer += Time.deltaTime;


        ChargeGun();
        if (!ShotAtCurrentAsteroid())
        {
            FindAnAsteroidToFireAt();
        }
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
                        aimPosition = currentAsteroid.transform.position +
                            currentAsteroid.rb.velocity *
                            gunAsteroidVector.magnitude / gunVelocity;

                        gunRotate.LookAt(aimPosition, upVector);
                        ShootIfPossible();
                        return true;
                    }
                }
            }
        }
        return false;
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
                    towerManager.bullets.FireAbullet(posLevel0pos1.position, gunRotate.rotation, gunVelocity, distance);
                } 
                else if (UpgradeLevel == 2)
                {
                    if (counter == 0)
                    {
                        counter = 1;
                        towerManager.bullets.FireAbullet(posLevel1pos1.position, gunRotate.rotation, gunVelocity, distance);
                    }
                    else
                    {
                        counter = 0;
                        towerManager.bullets.FireAbullet(posLevel1pos2.position, gunRotate.rotation, gunVelocity, distance);
                    }
                }
                else if (UpgradeLevel == 3)
                {
                    if (counter == 0)
                    {
                        counter++;
                        towerManager.bullets.FireAbullet(posLevel2pos1.position, gunRotate.rotation, gunVelocity, distance);
                    }
                    else if(counter == 1)
                    {
                        counter++;
                        towerManager.bullets.FireAbullet(posLevel2pos2.position, gunRotate.rotation, gunVelocity, distance);
                    }
                    else
                    {
                        counter = 0;
                        towerManager.bullets.FireAbullet(posLevel2pos3.position, gunRotate.rotation, gunVelocity, distance);
                    }
                }


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
        if (value == 1)
        {
            towerEnabled = true;
            maxShootDistance = towerManager.maxShootDistanceTowergunSqrd;
            maxPowerAmount = towerManager.maxPowerAmountTowergun;
            gunVelocity = towerManager.velocityTowergun;
            SecondsToShoot = towerManager.SecondsToShootTowergun;
            shotCostAmount = towerManager.shotCostAmountTowergun;
            Health = healthMax = towerManager.HealthTowergun;

            upgrade0.SetActive(true);
            upgrade1.SetActive(false);
            upgrade2.SetActive(false);

        }
        else if (value == 2)
        {
            towerEnabled = true;
            maxShootDistance = towerManager.maxShootDistanceTowergun1Sqrd;
            maxPowerAmount = towerManager.maxPowerAmountTowergun1;
            gunVelocity = towerManager.velocityTowergun1;
            SecondsToShoot = towerManager.SecondsToShootTowergun1;
            shotCostAmount = towerManager.shotCostAmountTowergun1;
            Health = healthMax = towerManager.HealthTowergun1;

            upgrade0.SetActive(false);
            upgrade1.SetActive(true);
            upgrade2.SetActive(false);
        }
        else if (value == 3)
        {
            towerEnabled = true;
            maxShootDistance = towerManager.maxShootDistanceTowergun2Sqrd;
            maxPowerAmount = towerManager.maxPowerAmountTowergun2;
            gunVelocity = towerManager.velocityTowergun2;
            SecondsToShoot = towerManager.SecondsToShootTowergun2;
            shotCostAmount = towerManager.shotCostAmountTowergun2;
            Health = healthMax = towerManager.HealthTowergun2;

            upgrade0.SetActive(false);
            upgrade1.SetActive(false);
            upgrade2.SetActive(true);
        }
    }

    public override int UpgradeCost()
    {
        int cost = UpgradeLevel == 1 ? towerManager.towerCostTowergun1 :
                       UpgradeLevel == 2 ? towerManager.towerCostTowergun2 : 0;

        return cost;
    }

    public override int SellPrice()
    {
        if (UpgradeLevel == 1)
        {
            return Mathf.RoundToInt(towerManager.towerCostTowergun * 0.8f);
        }
        else if (UpgradeLevel == 2)
        {
            return Mathf.RoundToInt((towerManager.towerCostTowergun + towerManager.towerCostTowergun1) * 0.8f);
        }
        else if (UpgradeLevel == 3)
        {
            return Mathf.RoundToInt((towerManager.towerCostTowergun + towerManager.towerCostTowergun1 +
                towerManager.towerCostTowergun2) * 0.8f);
        }
        return 0;
    }
}
