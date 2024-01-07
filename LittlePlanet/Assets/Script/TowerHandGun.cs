
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TowerHandGun : Tower
{
    //Networked
    [HideInInspector, UdonSynced]
    int localPowerAmount = 0;

    //

    [SerializeField]
    Transform gunFirePosition;

    float timer = 0f;
    float gunVelocity;
    bool shooting;

    [SerializeField]
    Text chargeDisplay;


    public override void OverwritableStart()
    {
    }

    public override void OverwritableUpdate()
    {
        if (!towerEnabled) return;
        timer += Time.deltaTime;

        if (chargeDisplay != null) //just a copy of the function in towergun doesnt inherit from that
        {
            chargeDisplay.text = localPowerAmount.ToString();
        }

        if (powerAmount > 0)
        {
            if (localPowerAmount < towerManager.maxPowerAmounthandgun)
            {
                localPowerAmount += 1;
                powerAmount -= 1;
                if (Networking.IsOwner(gameObject))
                {
                    RequestSerialization();
                }
            }
        }
        if (shooting )
        {
            ShootIfPossible();
        }
    }

    public override void OverWriteOnDrop()
    {
        downVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
    }

    public void ShootIfPossible(float distance = 0f)
    {
        if (timer > towerManager.SecondsToShootHandgun)
        {
            if (localPowerAmount >= shotCostAmount)
            {
                localPowerAmount -= shotCostAmount;
                timer = 0f;
                if (localPlayer.IsUserInVR())
                {
                    towerManager.bullets.FireAbullet(gunFirePosition.position, gunFirePosition.rotation, gunVelocity, distance, localDamage: true);
                }
                else
                {
                    towerManager.bullets.FireAbullet(gunFirePosition.position, localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation, gunVelocity, distance, localDamage: true);
                }

            }
        }
    }

    public override void OnPickupUseDown()
    {
        shooting = true;
    }

    public override void OnPickupUseUp()
    {
        shooting = false;
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
            maxPowerAmount = towerManager.maxPowerAmounthandgun;
            gunVelocity = towerManager.velocityHandgun;
            SecondsToShoot = towerManager.SecondsToShootHandgun;
            shotCostAmount = towerManager.shotCostAmounthandgun;
            Health = healthMax = towerManager.HealthHandgun;
            upgrade0.SetActive(true);
            upgrade1.SetActive(false);
            upgrade2.SetActive(false);

        }
        else if (UpgradeLevel == 2)
        {
            towerEnabled = true;
            maxPowerAmount = towerManager.maxPowerAmounthandgun1;
            gunVelocity = towerManager.velocityHandgun1;
            SecondsToShoot = towerManager.SecondsToShootHandgun1;
            shotCostAmount = towerManager.shotCostAmounthandgun1;
            Health = healthMax = towerManager.HealthHandgun1;

            upgrade0.SetActive(false);
            upgrade1.SetActive(true);
            upgrade2.SetActive(false);
        }
        else if (UpgradeLevel == 3)
        {
            towerEnabled = true;
            maxPowerAmount = towerManager.maxPowerAmounthandgun2;
            gunVelocity = towerManager.velocityHandgun2;
            SecondsToShoot = towerManager.SecondsToShootHandgun2;
            shotCostAmount = towerManager.shotCostAmounthandgun2;
            Health = healthMax = towerManager.HealthHandgun2;

            upgrade0.SetActive(false);
            upgrade1.SetActive(false);
            upgrade2.SetActive(true);
        }
    }
    public override int UpgradeCost()
    {
        int cost = UpgradeLevel == 1 ? towerManager.towerCostHandgun1 :
                       UpgradeLevel == 2 ? towerManager.towerCostHandgun2 : 0;

        return cost;
    }

    public override int SellPrice()
    {
        if (UpgradeLevel == 1)
        {
            return Mathf.RoundToInt(towerManager.towerCostHandgun * 0.8f);
        } 
        else if (UpgradeLevel == 2)
        {
            return Mathf.RoundToInt((towerManager.towerCostHandgun + towerManager.towerCostHandgun1) * 0.8f);
        }
        else if (UpgradeLevel == 3)
        {
            return Mathf.RoundToInt((towerManager.towerCostHandgun + towerManager.towerCostHandgun1 + 
                towerManager.towerCostHandgun2) * 0.8f);
        }
        return 0;
    }
}
