using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AIController2D : MonoBehaviour, IDamageable
{
	[Header("General")]
	[SerializeField, Range(0,250)] public float Health = 100;
	[SerializeField] float speed;
	[Header("Jump")]
	[SerializeField] float jumpHeight;
	[SerializeField] float doubleJumpHeight;
	[SerializeField, Range(1, 5)] float fallRateMultiplier;
	[SerializeField, Range(1, 5)] float lowJumpRateMultiplier;
	[SerializeField, Range(1, 20)] float doublejumpTimer;
	[Header("Ground")]
	[SerializeField] Transform groundTransform;
	[SerializeField] LayerMask groundLayerMask;
	[SerializeField] float groundRadius;
	[Header("Animation")]
	[SerializeField] Animator animator;
	[SerializeField] SpriteRenderer spriteRenderer;
	[Header("AI")]
	[SerializeField] Transform[] Waypoints;
	[SerializeField, Range(0.5f, 5)] float rayDistance = 1;
	[SerializeField, Range(0.5f, 5)] float attackDistance = 1;
	[SerializeField, Range(0.5f, 10)] float waitTime = 0.7f;
	[SerializeField] LayerMask playerLayerMask;
	[SerializeField] string enemyTag;

	//CharacterController characterController;
	Rigidbody2D rb;
	Vector3 velocity = Vector3.zero;
	bool faceRight = true;
	float groundAngle = 0;
	Transform targetWaypoint = null;
	Transform targetLastLocation = null;
	GameObject enemy = null;

	State state = State.IDLE;
	float stateTimer = 0;
	bool attack = false;

	enum State
	{
		IDLE,
		PATROL,
		CHASE,
		ATTACK
	}

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		// update ai
		CheckEnemySeen();
		
		Vector2 direction = Vector2.zero;
		switch (state)
		{
			case State.IDLE:
				if (enemy != null) state = State.CHASE;
				stateTimer += Time.deltaTime;
				if (stateTimer >= waitTime)
				{
					SetNewWaypointTarget();
					state = State.PATROL;
				}
				break;
			case State.PATROL:
				{
					if (enemy != null) state = State.CHASE;

					direction.x = Mathf.Sign(targetWaypoint.position.x - transform.position.x);
					float dx = Mathf.Abs(targetWaypoint.position.x - transform.position.x);
					if (dx <= 0.25f)
					{
						state = State.IDLE;
						stateTimer = 0;
					}
				}
				break;
			case State.CHASE:
				{
					// checks if it can still see the player
					getSeenPlayerLocation();
					// if it cant see the player then return to idle
					if (!CanSeePlayer()) state = State.IDLE;
					// if the player was not seen in a short time then revert back to idle
					if (stateTimer > 5) state = State.IDLE;

					// gets the distance between the current position and last player position
					direction.x = Mathf.Sign(targetLastLocation.position.x - transform.position.x);
					float dx = Mathf.Abs(targetLastLocation.position.x - transform.position.x);

					// if the distance is less then attack distance then switch to attack state
					if (dx <= attackDistance)
					{
						state = State.ATTACK;
						stateTimer = 0;
					}
				}
				break;
			case State.ATTACK:
				// checks if the attack animation has ended
				if (stateTimer > 0.5f)
				{
					state = State.IDLE;
					stateTimer = 0;
					attack = false;
				}
				// makes sure that attacking only happens once
				if (!attack)
				{
					animator.SetTrigger("Attack");
					attack = true;
				}
				//increases the state timer
				stateTimer += Time.deltaTime;
				break;
			default:
				break;
		}


		bool onGround = UpdateGroundCheck();

		// get direction input

		// transform direction to slope space
		direction = Quaternion.AngleAxis(groundAngle, Vector3.forward) * direction;
		Debug.DrawRay(transform.position, direction, Color.green);

		velocity.x = direction.x * speed;

		// set velocity
		if (onGround)
		{
			if (velocity.y < 0) velocity.y = 0;
			//if (Input.GetButtonDown("Jump"))
			//{
			//	velocity.y += Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
			//	StartCoroutine(DoubleJump());
			//	animator.SetTrigger("Jump");
			//}
		}

		// adjust gravity for jump
		float gravityMultiplier = 1;
		if (!onGround && velocity.y < 0) gravityMultiplier = fallRateMultiplier;
		//if (!onGround && velocity.y > 0 && !Input.GetButton("Jump")) gravityMultiplier = lowJumpRateMultiplier;

		velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;

        // move character
        rb.velocity = velocity;

		// flip character to face direction of movement (velocity)
		if (velocity.x > 0 && !faceRight) Flip();
		if (velocity.x < 0 && faceRight) Flip();

		// update animator
		animator.SetFloat("Speed", Mathf.Abs(velocity.x));
		//animator.SetBool("Fall", !onGround && velocity.y < -0.2f);
	}

	private bool UpdateGroundCheck()
	{
		// check if the character is on the ground
		Collider2D collider = Physics2D.OverlapCircle(groundTransform.position, groundRadius, groundLayerMask);
		if (collider != null)
		{
			RaycastHit2D raycastHit = Physics2D.Raycast(groundTransform.position, Vector2.down, groundRadius, groundLayerMask);
			if (raycastHit.collider != null)
			{
				// get the angle of the ground (angle between up vector and ground normal)
				groundAngle = Vector2.SignedAngle(Vector2.up, raycastHit.normal);
				Debug.DrawRay(raycastHit.point, raycastHit.normal, Color.red);
			}
		}

		return (collider != null);
	}

	private void Flip()
	{
		faceRight = !faceRight;
		spriteRenderer.flipX = !faceRight;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(groundTransform.position, groundRadius);
	}

	private void SetNewWaypointTarget()
	{
		Transform waypoint = null;
		do
		{
			waypoint = Waypoints[Random.Range(0, Waypoints.Length)];
		} while (waypoint == targetWaypoint);

		targetWaypoint = waypoint;
	}

	private bool CanSeePlayer()
	{
		RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, ((faceRight) ? Vector2.right : Vector2.left) * rayDistance, rayDistance, playerLayerMask);
		Debug.DrawRay(transform.position, ((faceRight) ? Vector2.right : Vector2.left) * rayDistance);
		
		//Debug.Log(raycastHit.collider.gameObject.name);

		return raycastHit.collider != null && raycastHit.collider.gameObject.CompareTag("Player");
	}

	private void CheckEnemySeen()
	{
		enemy = null;
		RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, ((faceRight) ? Vector2.right : Vector2.left), rayDistance, playerLayerMask);
		if (raycastHit.collider != null && raycastHit.collider.gameObject.CompareTag(enemyTag))
		{
			enemy = raycastHit.collider.gameObject;
			Debug.DrawRay(transform.position, ((faceRight) ? Vector2.right : Vector2.left) * rayDistance, Color.red);
		}
	}

	private void getSeenPlayerLocation()
	{
		// sends a ray to get players last position
		RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, ((faceRight) ? Vector2.right : Vector2.left) * rayDistance, rayDistance, playerLayerMask);

		// if the object is the player tag change the last seen position
		if (raycastHit.collider != null && raycastHit.collider.gameObject.CompareTag("Player"))
		{
			targetLastLocation = raycastHit.collider.transform;
		}
	}

	public void Damage(int damage)
	{
		Health -= damage;
		print(Health);
	}
}