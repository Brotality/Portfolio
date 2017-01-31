using UnityEngine;
using System.Collections;

/*
 * Main class used to control the player.
 * Will in future contain health and other variables.
 */
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {

	[Tooltip("Speed at which player moves.")]
	[SerializeField]
	protected float fMoveSpeed;

	private float inputX, inputY;
	private Rigidbody rigid;

	void Awake() {
		rigid = GetComponent<Rigidbody>();
	}

	void Update() {
		inputX = Input.GetAxis("Horizontal");
		inputY = Input.GetAxis("Vertical");
	}

	void FixedUpdate() {
		rigid.velocity = new Vector3(inputX, 0, inputY) * fMoveSpeed;
	}

	private void Die() {
		GameControl.PlayerDied();
		Destroy(gameObject);
	}
}
