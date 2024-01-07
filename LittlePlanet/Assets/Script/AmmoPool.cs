
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class AmmoPool : UdonSharpBehaviour
{
    [SerializeField]
    Ammo[] ammo;

    void Start()
    {
        for (int i = 0; i < ammo.Length; i++)
        {
            ammo[i].gameObject.SetActive(false);
        }
    }

    public void FireAbullet(Vector3 startPosition, Quaternion startRotation, float startVelocity, float distance, bool localDamage = false)
    {
        for (int i = 0; i < ammo.Length; i++)
        {
            if (!ammo[i].gameObject.activeSelf)
            {
                ammo[i].transform.position = startPosition;
                ammo[i].transform.rotation = startRotation;
                ammo[i].gameObject.SetActive(true);
                ammo[i].FireAmmo(startVelocity, distance, localDamage);
                return;
            }
        }
    }
}
