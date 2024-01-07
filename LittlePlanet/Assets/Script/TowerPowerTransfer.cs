
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TowerPowerTransfer : Tower
{
    //[HideInInspector]
    public Tower[] towersInRangePower;
    public Tower[] towersInRangeGuns;

    [HideInInspector]
    public int towersInRangePowerCount = 0;
    [HideInInspector]
    public int towersInRangeGunCount = 0;

    private float powerUpdateTimer;

    int slowCharge;

    [SerializeField]
    int slowChargeValue = 4;

    float powerTransferFrequency;

    public override void OverwritableStart()
    {
        towersInRangePower = new Tower[towerManager.towersPowerTransferArrayLength];
        towersInRangeGuns = new Tower[towerManager.towersPowerTransferArrayLength];
    }

    public override void OverwritableUpdate() //update runs on base class this is to add to that
    {
        if (!towerEnabled) return;
        powerUpdateTimer += Time.deltaTime;
        if (powerUpdateTimer > powerTransferFrequency)
        {
            powerUpdateTimer = 0f;
            TransferPowerFast();

            slowCharge++;
            if (slowCharge >= slowChargeValue)
            {
                slowCharge = 0;
                TransferPowerSlow();
            }
        }
    }

    private void TransferPowerSlow()
    {
        for (int i = 0; i < towersInRangeGunCount; i++)
        {
            if (powerAmount > towersInRangeGuns[i].powerAmount)
            {
                if (towersInRangeGuns[i].powerAmount < towersInRangeGuns[i].maxPowerAmount)
                {
                    towersInRangeGuns[i].AddOnePower(); //do a power swap
                    SubtractOnePower();
                }
            }
            else if (powerAmount < towersInRangeGuns[i].powerAmount)
            {
                towersInRangeGuns[i].SubtractOnePower(); //do a power swap
                AddOnePower();
            }
        }
    }

    private void TransferPowerFast()
    {
        for (int i = 0; i < towersInRangePowerCount; i++)
        {
            if (powerAmount > towersInRangePower[i].powerAmount)
            {
                if (powerAmount < towersInRangePower[i].maxPowerAmount)
                {
                    towersInRangePower[i].AddOnePower(); //do a power swap
                    SubtractOnePower();
                }
            }
            else if (powerAmount < towersInRangePower[i].powerAmount)
            {
                towersInRangePower[i].SubtractOnePower(); //do a power swap
                AddOnePower();
            }
        }
    }

    public void AddTowerPowerTower(Tower tower)
    {
        if (towersInRangePowerCount >= towersInRangePower.Length) return;
        towersInRangePower[towersInRangePowerCount] = tower;
        towersInRangePowerCount++;
    }

    public void AddTowerGunTower(Tower tower)
    {
        if (towersInRangeGunCount >= towersInRangeGuns.Length) return;
        towersInRangeGuns[towersInRangeGunCount] = tower;
        towersInRangeGunCount++;
    }

    public void ResetArrays()
    {
        towersInRangePowerCount = 0;
        towersInRangeGunCount = 0;
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
            powerTransferFrequency = towerManager.powerTransferFrequency;
            Health = healthMax = towerManager.HealthPowerTransfer;


            upgrade0.SetActive(true);
            upgrade1.SetActive(false);
            upgrade2.SetActive(false);

        }
        else if (UpgradeLevel == 2)
        {
            towerEnabled = true;
            powerTransferFrequency = towerManager.powerTransferFrequency1;
            Health = healthMax = towerManager.HealthPowerTransfer1;

            upgrade0.SetActive(true);
            upgrade1.SetActive(true);
            upgrade2.SetActive(false);
        }
        else if (UpgradeLevel == 3)
        {
            towerEnabled = true;
            powerTransferFrequency = towerManager.powerTransferFrequency2;
            Health = healthMax = towerManager.HealthPowerTransfer2;

            upgrade0.SetActive(true);
            upgrade1.SetActive(true);
            upgrade2.SetActive(true);
        }
    }

    public override int UpgradeCost()
    {
        int cost = UpgradeLevel == 1 ? towerManager.towerCostPowerTransfer1 :
                       UpgradeLevel == 2 ? towerManager.towerCostPowerTransfer2 : 0;

        return cost;
    }

    public override int SellPrice()
    {
        if (UpgradeLevel == 1)
        {
            return Mathf.RoundToInt(towerManager.towerCostPowerTransfer * 0.8f);
        }
        else if (UpgradeLevel == 2)
        {
            return Mathf.RoundToInt((towerManager.towerCostPowerTransfer + towerManager.towerCostPowerTransfer1) * 0.8f);
        }
        else if (UpgradeLevel == 3)
        {
            return Mathf.RoundToInt((towerManager.towerCostPowerTransfer + towerManager.towerCostPowerTransfer1 +
                towerManager.towerCostPowerTransfer2) * 0.8f);
        }
        return 0;
    }
}

