
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Rocket : UdonSharpBehaviour
{
    [SerializeField]
    ChgairController chairController;

    [SerializeField]
    float velocity = 3f;

    bool useRocket = false;

    VRCPlayerApi localPlayer;

    Rigidbody rb;
    private void Start()
    {
        localPlayer = Networking.LocalPlayer;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (useRocket)
        {
            chairController.LocalPlayerChair.AddVelocity(transform.forward * velocity * Time.deltaTime);
        }
    }

    public override void OnPickupUseDown()
    {
        useRocket = true;
    }

    public override void OnPickupUseUp()
    {
        useRocket = false;
    }

    public void SpawnRocket()
    {
        transform.position = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
        rb.velocity = Vector3.zero; 
        rb.angularVelocity = Vector3.zero;
    }
}
