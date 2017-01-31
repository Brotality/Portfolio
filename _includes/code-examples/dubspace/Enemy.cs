using UnityEngine;
using System.Collections;

/*
 * Super Enemy class to be used by all enemy types
 */
[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour {

	public delegate void EnemyEvent(Enemy e);
	public EnemyEvent OnEnemyDeath;

	[Tooltip("Speed at which enemy moves towards player.")]
	[SerializeField]
	protected float fMoveSpeed;
	[Tooltip("Starting/current health.")]
	[SerializeField]
	protected float fHealth;

	// Reference to rigidbody component of this object
	protected Rigidbody rigid;
	// Reference to the target transform to move towards
	protected Transform target;

	/*
	 * Overridable incase sub-classes need to initialise more variables
	 */
	public virtual void Awake() {
		rigid = GetComponent<Rigidbody>();
	}

	/*
	 * Overridable incase sub-classes implement more complex AI behaviour
	 */
	public virtual void FixedUpdate() {
		Vector3 dir = ( target.position - transform.position ).normalized;
		rigid.velocity = new Vector3(dir.x, 0, dir.z) * fMoveSpeed;
	}

	public void setTarget(Transform player) {
		target = player;
	}

	public void Hit(float damage) {
		fHealth -= damage;
		if(fHealth <= 0)
			Die();
	}

	public void Die() {
		if(OnEnemyDeath != null)
			OnEnemyDeath(this);
		Destroy(gameObject);
	}

	/*
	 * Event subscriptions
	 */

	public void OnEnable() {
		GameControl.OnPlayerDeath += Die;
	}

	public void OnDisable() {
		GameControl.OnPlayerDeath -= Die;
	}
}
