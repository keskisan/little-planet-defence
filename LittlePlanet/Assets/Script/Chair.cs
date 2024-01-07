

using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class Chair : UdonSharpBehaviour
{
    [SerializeField]
    ChgairController chairController;

    public ChairNetworking chairNetworking;

    private VRCPlayerApi ownerPlayer = null; //owner if object in game

    [SerializeField]
    VoxelPlanet planet;

    [SerializeField]
    Rigidbody rb;

    [SerializeField]
    float rayLength = 0.3f;

    [SerializeField]
    LayerMask floorLayer;

    [SerializeField]
    CapsuleCollider capsuleCollider;

    [SerializeField]
    float movementSpeedForward = 4f;

    [SerializeField]
    float movementSpeedSideways = 4f;

    [SerializeField]
    VRCStation vrcStation;

    private float forwardMovement = 0f;
    private float sidewayMovement = 0f;

    VRCPlayerApi localPlayer;

    [SerializeField]
    float rotationsSpeed = 5f;

    [SerializeField]
    float maxSpeed = 10f;

    [SerializeField]
    float jumpImpulse = 30f;

    [SerializeField]
    float rotationSpeed = 360f;

    bool isJumping = false;
    bool hasAddedJumpImpulse = false;
    bool hasAddedDoubleJumpImpulse = false;

    private Vector3 downVelocity;
    private Vector3 totalVelocity;
    private Vector3 forwardVelocity;
    private Vector3 rocketVelocity;

    Vector3 forward;
    Vector3 right;


    private Vector3 startPosition;
    private Quaternion startRotation;

    bool onGround;
    bool onSteep;

    private Quaternion desiredRotation;

    Vector3 upVector;
    Vector3 downVector;

    private void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        desiredRotation = transform.rotation;
        localPlayer = Networking.LocalPlayer;
        vrcStation.disableStationExit = true;
        SetOwnerPlayer();
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        SetOwnerPlayer();
    }

    public void SetOwnerPlayer()
    {
        ownerPlayer = Networking.GetOwner(gameObject);
        if (ownerPlayer.isMaster)
        {
            if (this != chairController.chairs[0]) //returns to pool
            {
                ownerPlayer = null;
            }
        }
        else
        {
            //code for when someone gets it
        }
    }

    public void AddVelocity(Vector3 value)
    {
        rocketVelocity = value;
        downVelocity = Vector3.zero;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (Vector3.Dot(collision.GetContact(0).normal, upVector) > 0.4f) //can jump of steep or small ledge but not climb walls
        {
            onSteep = true;
        }
    }

    private void FixedUpdate()
    {
        if (!Networking.IsOwner(gameObject)) return;

        CalculateNewAxis();
        
        //using physics spherecast is too funky
        onGround = Physics.Raycast(transform.position, downVector, out RaycastHit hit, rayLength, floorLayer);
        if (onGround || onSteep)
        {
            hasAddedDoubleJumpImpulse = false;

            forwardVelocity = forward * forwardMovement * movementSpeedForward;
            forwardVelocity += right * sidewayMovement * movementSpeedSideways;

            if (isJumping)
            {
                if (!hasAddedJumpImpulse)
                {
                    downVelocity += upVector * jumpImpulse;
                    hasAddedJumpImpulse = true;
                }
                isJumping = false;
            }
            else
            {
                if (hit.distance > rayLength * 0.5f) //allows jump and movement somewhat above surface, avoid getting stuck on steep slopes ie craters
                {
                    downVelocity += downVector * planet.fallSpeed;
                }
                else if (Vector3.Dot(downVelocity, downVector) > 0) //falling down
                {
                    downVelocity = Vector3.zero;
                }
                
            }
            
        }
        else //above ground
        {
            if (isJumping)
            {
                if (!hasAddedDoubleJumpImpulse)
                {
                    downVelocity += upVector * jumpImpulse;
                    hasAddedDoubleJumpImpulse = true;
                }
                isJumping = false;
            }
            downVelocity += downVector * planet.fallSpeed;
        }
        totalVelocity = Vector3.zero;

        totalVelocity += forwardVelocity;

        totalVelocity += downVelocity;
        totalVelocity += rocketVelocity;
        rocketVelocity = Vector3.zero;
        LimitMaxSpeed();

        rb.velocity = totalVelocity;
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationsSpeed);

        onSteep = false;
    }

    private void LimitMaxSpeed()
    {
        if (totalVelocity.magnitude > maxSpeed) //limits max velocity
        {
            totalVelocity = totalVelocity.normalized * maxSpeed;
        }
    }

    private void CalculateNewAxis() //walk on walls script causes double rotation to be applied for some reason
    {
        upVector = (transform.position - planet.gravityPosition).normalized;
        downVector = upVector * -1f;

        //calculate chair rotstion based on chair

        Quaternion playerRotation = transform.rotation; 
        Vector3 forwardRaw = playerRotation * Vector3.forward; 

        float projectedOnTransformUpScale = Vector3.Dot(forwardRaw, upVector); //project on imaginary plane
        Vector3 projectedTransformUp = upVector * projectedOnTransformUpScale;
        Vector3 newForward = forwardRaw - projectedTransformUp;

        Quaternion newRotation = Quaternion.LookRotation(newForward, upVector);
        desiredRotation = newRotation; //chair rotation space
        
        //calculate movement rotation based on head space

        playerRotation = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
        forwardRaw = playerRotation * Vector3.forward; //player head rotation space

        projectedOnTransformUpScale = Vector3.Dot(forwardRaw, upVector); //project on imaginary plane
        projectedTransformUp = upVector * projectedOnTransformUpScale;
        newForward = forwardRaw - projectedTransformUp;

        newRotation = Quaternion.LookRotation(newForward, upVector);

        forward = newRotation * Vector3.forward; //player head rotation space
        right = newRotation * Vector3.right; //player head rotation space    
    }

    public override void InputLookHorizontal(float value, UdonInputEventArgs args)
    {
        if (!Networking.IsOwner(gameObject)) return;
        float angle = value * Time.fixedDeltaTime * rotationSpeed;

        if (onGround)
        {
            transform.RotateAround(transform.position, transform.up, angle);
        }
    }

    public override void InputMoveVertical(float value, UdonInputEventArgs args)
    {
        if (ownerPlayer == localPlayer)
        {
            forwardMovement = value;
        }
    }

    public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
    {
        if (ownerPlayer == localPlayer)
        {
            sidewayMovement = value;
        }
    }

    public override void InputJump(bool value, UdonInputEventArgs args)
    {
        if (ownerPlayer == localPlayer)
        {
            if (value)
            {
                isJumping = true;
                hasAddedJumpImpulse = false;
            }
        }   
    } 

    public override void OnPlayerRespawn(VRCPlayerApi player)
    {
        if (ownerPlayer == localPlayer)
        {
            if (player == localPlayer)
            {
                WalkOnPlanet();
            }
        }      
    }

    public void UseStation()
    {
        if (ownerPlayer == localPlayer)
        {
            WalkOnPlanet();
        }
    }

    public void WalkOnPlanet()
    {
        RescaleCapsuleCollider();

        totalVelocity = Vector3.zero;
        transform.position = startPosition;
        transform.rotation = startRotation;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        vrcStation.UseStation(localPlayer);
        isJumping = false;
    }

    private void RescaleCapsuleCollider()
    {
        Vector3 feetPos = localPlayer.GetPosition();
        Vector3 EyePos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;

        Vector3 height = EyePos - feetPos;

        capsuleCollider.height = height.magnitude;
        capsuleCollider.center = new Vector3(0f, height.magnitude * 0.5f, 0f);
    }
}
