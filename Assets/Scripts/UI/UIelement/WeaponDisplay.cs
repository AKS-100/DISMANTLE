using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles updating the high score display
/// </summary>
public class WeaponDisplay : UIelement
{
    [Header("References")]
    [Tooltip("The text that displays the remaining ammo")]
    public Text ammoText = null;
    [Tooltip("The UI Image to display the weapon silouette to")]
    public RawImage gunDisplayImage;
    [Tooltip("The UI Image to display the ammo silouette to")]
    public RawImage ammoPackDisplayImage;

    /// <summary>
    /// Description:
    /// Updates the ammo text with the remaining ammo for the currently equipped weapon if ammo is being used
    /// if ammo is not being used for that gun it shows nothing
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    public void DisplayGunInformation()
    {
        if (GameManager.instance == null || GameManager.instance.player == null) return;
        PlayerController playerController = GameManager.instance.player.GetComponentInChildren<PlayerController>();
        if (playerController == null || playerController.playerShooter == null) return;
        Shooter playerShooter = playerController.playerShooter;

        if (playerShooter.guns == null || playerShooter.guns.Count <= playerShooter.equippedGunIndex) return;
        Gun equippedGun = playerShooter.guns[playerShooter.equippedGunIndex];
        if (equippedGun == null) return;

        if (equippedGun.useAmmo && AmmoTracker._instance)
        {
            if (ammoText != null)
            {
                ammoText.text = AmmoTracker._instance[equippedGun.ammunitionID].ToString();
            }
            if (ammoPackDisplayImage != null && equippedGun.ammoImage != null)
            {
                ammoPackDisplayImage.color = new Color(255, 255, 255, 255);
                ammoPackDisplayImage.texture = equippedGun.ammoImage.texture;
            }
        }
        else
        {
            if (ammoText != null)
            {
                ammoText.text = "";
            }
            if (ammoPackDisplayImage != null)
            {
                ammoPackDisplayImage.color = new Color(0,0,0,0);
            }
        }

        if (equippedGun.weaponImage != null && gunDisplayImage != null)
        {
            gunDisplayImage.texture = equippedGun.weaponImage.texture;
        }
    }

    /// <summary>
    /// Description:
    /// Updates the UI element according to this class
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    public override void UpdateUI()
    {
        // This calls the base update UI function from the UIelement class
        base.UpdateUI();

        // The remaining code is only called for this sub-class of UIelement and not others
        DisplayGunInformation();
    }
}