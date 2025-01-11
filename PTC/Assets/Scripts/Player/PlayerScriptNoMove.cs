
using UnityEngine;

public class PlayerScriptNoMove : PlayerScript
{
    private void Update()
    {
        //Update no imput check
    }

    void FixedUpdate()
    {
        //FixedUpdate no imput check

        // Interpolate position and rotation
        transform.position = Vector3.Lerp(transform.position, targetPosition, positionSmoothness);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothness);
    }

    public override void ReceiveDamage(PlayerPacket playerPacket)
    {
        //Does nothing to avoid double hits

        //check if power up shield
        if (HasPowerUp(PowerUps.SHIELD))
        {
            Destroy(shieldSpawned);
            RemovePowerUp(PowerUps.SHIELD);
            return;
        }
    }
}
