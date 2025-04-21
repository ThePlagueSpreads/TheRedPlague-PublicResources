using UnityEngine;

namespace TheRedPlague.Mono.StoryContent;

public class GrabGunTrigger : MonoBehaviour
{
    public GrabGunAnimation grabGunAnimation;

    private void Start()
    {
        if (grabGunAnimation.GetIsGunDisabled())
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (grabGunAnimation.GetIsGunDisabled()) return;
        var vehicle = other.GetComponent<Vehicle>();
        var subRoot = other.GetComponent<SubRoot>();
        if (other.gameObject == Player.main.gameObject || (vehicle && Player.main.GetVehicle() == vehicle) ||
            (subRoot && Player.main.GetCurrentSub() == subRoot))
        {
            grabGunAnimation.DisableGun();
        }
    }
}