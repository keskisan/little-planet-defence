
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Tower : UdonSharpBehaviour
{
    //Networked veriables
    [UdonSynced]
    Vector3 currentPosition;

    [UdonSynced]
    Quaternion currentRotation;

    [UdonSynced]
    public bool towerEnabled;

    [UdonSynced, FieldChangeCallback(nameof(Health))]
    private float _health;

    [UdonSynced]
    public int powerAmount;

    public float Health
    {
        set
        {
            if (value < 0)
            {
                _health = 0;
                towerEnabled = false;
            }
            else
            {
                _health = value;
            }
        }
        get => _health;
    }

    

    [UdonSynced, FieldChangeCallback(nameof(UpgradeLevel))]
    private int _upgradeLevel;

    public int UpgradeLevel
    {
        set
        {
            _upgradeLevel = value;
            UpgradeLevelHasChanged(value);
        }
        get => _upgradeLevel;
    }
    //

    public TowerManager towerManager;

    public Rigidbody rb;

    [HideInInspector]
    public int maxPowerAmount = int.MaxValue;

    public Transform laserPosition;

    Vector3 lastposition;
    Quaternion lastRotation;

    [HideInInspector]
    public Vector3 upVector;
    Vector3 downVector;

    //Vector3 forward;
    //Vector3 right;

    public Vector3 downVelocity;

    private Quaternion desiredRotation;

    bool onGround;

    

    VRCPickup pickup;

    [SerializeField]
    Text powerDisplay;

    [SerializeField]
    Text levelDisplay;

    [SerializeField]
    Text healthDisplay;

    [HideInInspector]
    public float SecondsToShoot;

    [HideInInspector]
    public int shotCostAmount;

    public GameObject upgrade0;
    public GameObject upgrade1;
    public GameObject upgrade2;

    [SerializeField]
    int asteroidLayer = 23;

    [HideInInspector]
    public float healthMax;

    public VRCPlayerApi localPlayer;

    Vector3 veryFarAway;
    
    private void Start()
    {
        veryFarAway = new Vector3(10000f, 10000f, 10000f);
        pickup = GetComponent<VRCPickup>();
        localPlayer = Networking.LocalPlayer;
        towerEnabled = false;
        OverwritableStart();
    }
    public virtual void OverwritableStart()
    {
    }


    private void Update()
    {
        OverwritableUpdate();

        if (towerEnabled)
        {
            ObjectMoving();
            MoveObject();
            UpdateDisplay();   
        }
        else
        { 
            transform.position = veryFarAway;
        }
        
    }

    private void UpdateDisplay()
    {
        if (powerDisplay != null)
        {
            powerDisplay.text = powerAmount.ToString();
        }
        if (levelDisplay != null)
        {
            levelDisplay.text = UpgradeLevel.ToString();
        }
        if (healthDisplay != null)
        {
            healthDisplay.text = Health.ToString("#.0");
        }
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
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, towerManager.rotationsSpeed);
        }
    }

    /*private void ReturnToMasterIfNotHeld()//try keep objects with master so spawning is not messy
    {
        if (Networking.IsMaster)
        {
            if (!pickup.IsHeld)
            {
                if (Networking.GetOwner(gameObject) != localPlayer)
                {
                    Networking.SetOwner(localPlayer, gameObject); 
                }
            }
            
        }
    }*/

    public override void OnPickup()
    {
        if (!Networking.IsMaster)
        {
            Networking.SetOwner(localPlayer, gameObject);
        }
    }

    public override void OnDrop()
    {
        
        OverWriteOnDrop();
        if (!Networking.IsMaster)
        {
            Networking.SetOwner(Networking.GetOwner(towerManager.gameObject), gameObject); //towermanager should always belong to master
        }
    }

    public virtual void OverWriteOnDrop()
    {

    }

    public virtual void UpgradeLevelHasChanged(int value)
    {

    }

    public virtual void OverwritableUpdate()
    {
    }

    public void AddOnePower()
    {
        powerAmount++;
        if (Networking.IsOwner(gameObject))
        {
            RequestSerialization();
        }
    }

    public void SubtractOnePower()
    {
        powerAmount--;
        if (Networking.IsOwner(gameObject))
        {
            RequestSerialization();
        }
    }


    //--
    private void ObjectMoving()
    {
        if (Networking.IsOwner(localPlayer, gameObject))
        {
            if (lastposition != transform.position || lastRotation != currentRotation)
            {    
                WhenTowerMoves();

                currentPosition = lastposition = transform.position;
                currentRotation = lastRotation = transform.rotation;
                RequestSerialization();
            } 
        }
        else
        {
            if (lastposition != currentPosition || lastRotation != currentRotation)
            {
                transform.position = lastposition = currentPosition;
                transform.rotation = lastRotation = currentRotation;
                WhenTowerMoves();
            } 
        }     
    }

    private void WhenTowerMoves()
    {
        towerManager.UpdateConnections();
        CalculateNewAxis();

        downVelocity += downVector * towerManager.planet.fallSpeed;

        onGround = Physics.Raycast(transform.position, downVector, out RaycastHit hit, towerManager.rayLength, towerManager.floorLayer);
        if (onGround)
        {
            downVelocity = Vector3.zero;
        }
    }

    private void CalculateNewAxis()
    {
        upVector = (transform.position - towerManager.planet.gravityPosition).normalized;
        downVector = upVector * -1f;

        Vector3 forwardRaw = transform.forward;

        float projectedOnTransformUpScale = Vector3.Dot(forwardRaw, upVector); //project on imaginary plane
        Vector3 projectedTransformUp = upVector * projectedOnTransformUpScale;
        Vector3 newForward = forwardRaw - projectedTransformUp;

        Quaternion newRotation = Quaternion.LookRotation(newForward, upVector);

        //forward = newRotation * Vector3.forward;
        //right = newRotation * Vector3.right;
        desiredRotation = newRotation;
    }

    public virtual void UpgradeGun(int level) //look into protection bet not supported in udon
    {
    }

    //--
    public void KGN()
    {
        Health = 0;
        UpgradeLevel = 0;
        towerManager.UpdateConnections();
        UpdateSoldOutTowerDisplayInMenu();
    }

    

    //--
    public virtual void KillGun()
    {  
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "KGN");
    }

    public virtual int UpgradeCost() //look into protection bet not supported in udon
    {
        return 0;
    }

    public virtual int SellPrice() //look into protection bet not supported in udon
    {
        return 0;
    }


    //--
    private void OnCollisionEnter(Collision collision) //kill tower if asteroid crashes into it
    {
        if (!Networking.IsOwner(localPlayer, gameObject)) return;

        if (collision.gameObject.layer == asteroidLayer) //asteroid layer
        {
            Health -= collision.gameObject.transform.localScale.x * 1000f;
            RequestSerialization();
        }
    }

    private void UpdateSoldOutTowerDisplayInMenu()
    {
        if (GetComponent<TowerPowerTransfer>() != null)
        {
            towerManager.menuScript.powerTransferNowAvailable();
            return;
        }
        if (GetComponent<TowerGunTower>() != null)
        {
            towerManager.menuScript.gunTowerNowAvailable();
            return;
        }
        if (GetComponent<TowerMissileLauncher>() != null)
        {
            towerManager.menuScript.missileLauncherNowAvailable();
            return;
        }
        if (GetComponent<TowerRailGun>() != null)
        {
            towerManager.menuScript.railGunNowAvailable();
            return;
        }
        if (GetComponent<TowerHandGun>() != null)
        {
            towerManager.menuScript.handGunNowAvailable();
            return;
        }
    }

    public void fallDownIfGroundDissapear()
    {
        transform.position += Vector3.up * 0.1f;
    }
}
