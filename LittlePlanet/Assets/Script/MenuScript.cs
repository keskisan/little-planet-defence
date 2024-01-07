
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class MenuScript : UdonSharpBehaviour
{
    //Networked veriables
    [UdonSynced]
    public int money = 1000;

    [UdonSynced]
    public int destroyed = 0;

    [UdonSynced, HideInInspector]
    public bool gameHasStarted = false;
    //

    [SerializeField]
    ChgairController chairController;

    [SerializeField]
    TowerManager towerManager;

    [SerializeField]
    RadarGun radarGun;

    [SerializeField]
    Rocket rocket;

    [SerializeField]
    SpannerSell spannerSell;

    [SerializeField]
    SpannerUpgrade spannerUpgrade;

    [SerializeField]
    Transform bigMenu;

    [SerializeField]
    Rigidbody rb;

    [SerializeField]
    Transform handMenu;

    [SerializeField]
    GameObject startMenuInstructions;

    VRCPlayerApi localPlayer;

    Vector3 upVector;
    Vector3 downVector;

    private Vector3 downVelocity;

    private Quaternion desiredRotation;

    [SerializeField]
    VRCPickup pickup;

    bool onGround;

    [SerializeField]
    float mainMenuHeightAboveGround = 1.5f;

    [SerializeField]
    float maxPlanetHealth = 4139f;

    [SerializeField]
    GameObject endGamePlatform, endGameCanvas;

    [SerializeField]
    Transform endGameTeleport;

    [SerializeField]
    Text endGameAsteroidsDestroyed;

    [SerializeField]
    GameObject startButton, startButton1;

    [Header("canvas stuff")]
    [SerializeField]
    Text planetHealthText;
    [SerializeField]
    Text planetHealthText1;

    [SerializeField]
    Text moneyText;
    [SerializeField]
    Text moneyText1;

    [SerializeField]
    Text destroyedText;
    [SerializeField]
    Text destroyedText1;

    [SerializeField]
    GameObject soldOutPowerTransfer, soldOutPowerTransfer1, soldOutGuntower, soldOutGuntower1,
        soldOutMissileTower, soldOutMissileTower1, soldOutRailGunTower, soldOutRailGunTower1,
        soldOutHandgun, soldOutHandgun1;

    
    private void Start()
    {
        localPlayer = Networking.LocalPlayer;

        if (localPlayer.IsUserInVR())
        {
            handMenu.gameObject.SetActive(true);
        }
        else
        {
            handMenu.gameObject.SetActive(false);
        }

    }

    public override void PostLateUpdate()
    {
        if (gameHasStarted) startMenuInstructions.SetActive(false);

        moneyText1.text = moneyText.text = money.ToString();
        destroyedText.text = destroyedText1.text = destroyed.ToString();
        if (localPlayer.IsUserInVR())
        {
            handMenu.transform.position = localPlayer.GetBonePosition(HumanBodyBones.LeftHand);
            handMenu.transform.rotation = localPlayer.GetBoneRotation(HumanBodyBones.LeftHand);
        }

        WhenTowerMoves();
        MoveObject();
    }


    public void SpawnPowerTransfer()
    {
        if (!gameHasStarted) return;
        if (towerManager.AmountOfUnspawnedPowerTransferTowers() <= 1)
        {
            soldOutPowerTransfer.SetActive(true);
            soldOutPowerTransfer1.SetActive(true);
        }
        if (!Networking.IsMaster)
        {
            if (money < towerManager.towerCostPowerTransfer) return; //should help prevent negative spending
            money -= towerManager.towerCostPowerTransfer; 
        }
        
        chairController.LocalPlayerChair.chairNetworking.TrySpawnPowerTransferTower(localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position);
    }

    public void powerTransferNowAvailable()
    {
        soldOutPowerTransfer.SetActive(false);
        soldOutPowerTransfer1.SetActive(false);
    }

    public void SpawnGunTower()
    {
        if (!gameHasStarted) return;
        if (towerManager.AmountOfUnspawnedTowerGunTower() <= 1)
        {
            soldOutGuntower.SetActive(true);
            soldOutGuntower1.SetActive(true);
        }
        if (!Networking.IsMaster)
        {
            if (money < towerManager.towerCostTowergun) return; //should help prevent negative spending
            money -= towerManager.towerCostTowergun;
        }

        chairController.LocalPlayerChair.chairNetworking.TrySpawnGunTower(localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position); 
    }

    public void gunTowerNowAvailable()
    {
        soldOutGuntower.SetActive(false);
        soldOutGuntower1.SetActive(false);
    }

    public void SpawnMissileLauncher()
    {
        if (!gameHasStarted) return;
        if (towerManager.AmountOfUnspawnedMissileLaunchers() <= 1)
        {
            soldOutMissileTower.SetActive(true);
            soldOutMissileTower1.SetActive(true);
        }
        if (!Networking.IsMaster)
        {
            if (money < towerManager.towerCostMissileLauncer) return; //should help prevent negative spending
            money -= towerManager.towerCostMissileLauncer;
        }
        
        chairController.LocalPlayerChair.chairNetworking.TrySpawnMissileTower(localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position);
    }

    public void missileLauncherNowAvailable()
    {
        soldOutMissileTower.SetActive(false);
        soldOutMissileTower1.SetActive(false);
    }

    public void SpawnRailGun()
    {
        if (!gameHasStarted) return;
        if (towerManager.AmountOfUnspawnedRailGunTowers() <= 1)
        {
            soldOutRailGunTower.SetActive(true);
            soldOutRailGunTower1.SetActive(true);
        }
        if (!Networking.IsMaster)
        {
            if (money < towerManager.towerCostRailgun) return; //should help prevent negative spending
            money -= towerManager.towerCostRailgun;
        }

        chairController.LocalPlayerChair.chairNetworking.TrySpawnRailgunTower(localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position);
    }

    public void railGunNowAvailable()
    {
        soldOutRailGunTower.SetActive(false);
        soldOutRailGunTower1.SetActive(false);
    }

    public void SpawnHandGun()
    {
        if (!gameHasStarted) return;
        if (towerManager.AmountOfUnspawnedHandgunTowers() <= 1)
        {
            soldOutHandgun.SetActive(true);
            soldOutHandgun1.SetActive(true);
        }
        if (!Networking.IsMaster)
        {
            if (money < towerManager.towerCostHandgun) return; //should help prevent negative spending
            money -= towerManager.towerCostHandgun;
        }
        
        chairController.LocalPlayerChair.chairNetworking.TrySpawnhandgunTower(localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position);
    }

    public void handGunNowAvailable()
    {
        soldOutHandgun.SetActive(false);
        soldOutHandgun1.SetActive(false);
    }

    public void SpawnRadar()
    {
        radarGun.SpawnRadar();
    }

    public void SpawnRocket()
    {
        rocket.SpawnRocket();
    }

    public void SpawnUpgradeSpanner()
    {
        spannerUpgrade.SpawnSpanner();
    }

    public void SpawnSellSpanner()
    {
        spannerSell.SpawnSpanner();
    }

    public void STG()
    {
        gameHasStarted = true;
        chairController.UseStation();
        startButton.SetActive(false);
        startButton1.SetActive(false);
    }

    public void StartGame()
    {
        if (Networking.IsMaster)
        {
            gameHasStarted = true;
            RequestSerialization();
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "STG");
        }   
    }

    public void UpdatePlanetHealth(int planetHealth)
    {
        Debug.Log("menuscript : " +  planetHealth.ToString());
        float planetHealthPercent = planetHealth / maxPlanetHealth * 100f;
        planetHealthText.text = planetHealthText1.text = planetHealthPercent.ToString("0.0");
        if (planetHealthPercent < 10f)
        {
            LostGame();   
        }
    }

    private void LostGame()
    {
        endGamePlatform.SetActive(true);
        endGameCanvas.SetActive(true);
        endGameAsteroidsDestroyed.text = destroyed.ToString();
        chairController.LocalPlayerChair.transform.position = endGameTeleport.transform.position;
    }

    private void MoveObject()
    {
        if (Networking.IsOwner(localPlayer, gameObject))
        {
            rb.velocity = downVelocity;
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
        if (!pickup.IsHeld)
        {
            bigMenu.rotation = Quaternion.Slerp(bigMenu.rotation, desiredRotation, towerManager.rotationsSpeed);
        }
    }

    private void WhenTowerMoves()
    {
        CalculateNewAxis();

        downVelocity += downVector * towerManager.planet.fallSpeed;

        onGround = Physics.Raycast(bigMenu.position, downVector, out RaycastHit hit, towerManager.rayLength + mainMenuHeightAboveGround, towerManager.floorLayer);
        if (onGround)
        {
            downVelocity = Vector3.zero;
        }
    }

    private void CalculateNewAxis()
    {
        upVector = (bigMenu.position - towerManager.planet.gravityPosition).normalized;
        downVector = upVector * -1f;

        Vector3 forwardRaw = bigMenu.forward;

        float projectedOnTransformUpScale = Vector3.Dot(forwardRaw, upVector); //project on imaginary plane
        Vector3 projectedTransformUp = upVector * projectedOnTransformUpScale;
        Vector3 newForward = forwardRaw - projectedTransformUp;

        Quaternion newRotation = Quaternion.LookRotation(newForward, upVector);

        //forward = newRotation * Vector3.forward;
        //right = newRotation * Vector3.right;
        desiredRotation = newRotation;
    }

    public void DestroyedAnAsteroid()
    {
        destroyed++;
        RequestSerialization();
    }

    public void AddMoney(int value)
    {
        money += value;
        RequestSerialization();
    }
}
