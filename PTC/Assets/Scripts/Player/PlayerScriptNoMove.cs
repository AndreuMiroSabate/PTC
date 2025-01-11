
using UnityEngine;

public class PlayerScriptNoMove : PlayerScript
{
    private void Update()
    {
        //Update no imput check

        // Update Position and Rotation using Interpolation
        InterpolateMovement();
    }

    void FixedUpdate()
    {
        //FixedUpdate no imput check
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
