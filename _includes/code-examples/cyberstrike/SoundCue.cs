using UnityEngine;
using System.Collections;

/*
	A simple class replicating the SoundCue feature of UnrealEngine.
	This allows for a random sound to be played for a single event
	without having to create arrays and randomly choose on every object
	that needs sounds.
	Example use case is a gun firing having multiple similar sound effects.
*/
[System.Serializable]
public class SoundCue {

	public AudioClip[] AudioClips = null;

	public AudioClip GetAudioClip(){
		return AudioClips[Random.Range(0, AudioClips.Length-1)];
	}
}
