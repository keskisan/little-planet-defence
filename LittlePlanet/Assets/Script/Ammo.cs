
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Ammo : UdonSharpBehaviour
{
    [HideInInspector]
    public Rigidbody rb;

    public float timer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameObject.SetActive(false);
    }

    public virtual void FireAmmo(float startVelocity, float distance, bool localDamage = false)
    {
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > 2f)
        {
            timer = 0f;
            gameObject.SetActive(false);
        }
        UpdateAmmo();
    }

    public virtual void UpdateAmmo()
    {

    }
}
