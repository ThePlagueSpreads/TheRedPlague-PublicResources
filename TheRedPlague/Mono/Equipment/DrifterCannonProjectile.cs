using Nautilus.Utility;
using TheRedPlague.Data;
using UnityEngine;

namespace TheRedPlague.Mono.Equipment;

public class DrifterCannonProjectile : MonoBehaviour
{
    public float normalDamage = 100f;
    public float plagueDamage = 100f;

    private bool _struck;

    private void Start()
    {
        Destroy(gameObject, 15);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_struck) return;
        if (other.rigidbody != null)
        {
            var lm = other.rigidbody.gameObject.GetComponent<LiveMixin>();
            if (lm != null)
            {
                lm.TakeDamage(plagueDamage, transform.position, CustomDamageTypes.PlagueCutting);
                lm.TakeDamage(normalDamage, transform.position);
            }
        }

        Destroy(gameObject);
        _struck = true;
    }

    private void OnDestroy()
    {
        var vfx = Instantiate(Plugin.AssetBundle.LoadAsset<GameObject>("DrifterCannonProjectileShatterPrefab"), transform.position, Quaternion.identity);
        MaterialUtils.ApplySNShaders(vfx);
        vfx.AddComponent<SkyApplier>().renderers = new Renderer[] {vfx.GetComponent<Renderer>()};
    }
}