/////////////////////////////////////////////////////////////////////////////////
//
//	vp_CustomFPWeaponShooter.cs
//	© Alex Bennett. All Rights Reserved.
//
//	description:	A simple extension of vp_FPWeaponShooter overriding weapon firing
//					and uses custom SoundCue type for audio
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

[RequireComponent(typeof(vp_FPWeapon))]

public class vp_CustomFPWeaponShooter : vp_FPWeaponShooter {

	public SoundCue FireSounds = null;

	/// <summary>
	/// plays a random fire sound from soundcue
	/// </summary>
	protected override void PlayFireSound()
	{
		
		if (Audio == null)
			return;
		
		Audio.pitch = Random.Range(SoundFirePitch.x, SoundFirePitch.y) * Time.timeScale;
		Audio.clip = FireSounds.GetAudioClip();
		Audio.Play();
		// LORE: we must use 'Play' rather than 'PlayOneShot' for the
		// AudioSource to be regarded as 'isPlaying' which is needed
		// for 'vp_Component:DeactivateWhenSilent'
		
	}

	protected override void SpawnProjectiles()
	{
		
		if (ProjectilePrefab == null)
			return;
		
		// will only trigger on local player in multiplayer
		if (m_SendFireEventToNetworkFunc != null)
			m_SendFireEventToNetworkFunc.Invoke();
		
		m_CurrentFirePosition = ProjectileSpawnPoint.transform.position; //MuzzleFlash.transform.position; //GetFirePosition();
		m_CurrentFireRotation = ProjectileSpawnPoint.transform.rotation; //MuzzleFlash.transform.rotation; //GetFireRotation();
		m_CurrentFireSeed = GetFireSeed();
		
		// when firing a single projectile per discharge (pistols, machineguns)
		// this loop will only run once. if firing several projectiles per
		// round (shotguns) the loop will iterate several times. the fire seed
		// is the same for every iteration, but is multiplied with the number
		// of iterations to get a unique, deterministic seed for each projectile.
		for (int v = 0; v < ProjectileCount; v++)
		{
			
			GameObject p = null;
			p = (GameObject)vp_Utility.Instantiate(ProjectilePrefab, m_CurrentFirePosition, m_CurrentFireRotation);
			
			// TIP: uncomment this to debug-draw bullet paths and points of impact
			//DrawProjectileDebugInfo(v);
			
			p.SendMessage("SetSource", (ProjectileSourceIsRoot ? Root : Transform), SendMessageOptions.DontRequireReceiver);
			p.transform.localScale = new Vector3(ProjectileScale, ProjectileScale, ProjectileScale);	// preset defined scale
			
			SetSpread(m_CurrentFireSeed * (v + 1), p.transform);
			
		}
		
	}

}
