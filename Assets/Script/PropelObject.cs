using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PropelObject : MonoBehaviour
{
	[SerializeField, Range(100, 500)] float force;
	private void OnCollisionEnter(Collision collision)
	{
		GameObject cBall = collision.gameObject;
		cBall.transform.rotation = transform.rotation;
		Rigidbody otherRb = cBall.GetComponent<Rigidbody>();
		otherRb.AddRelativeForce(otherRb.transform.forward * force, ForceMode.Impulse);
	}
}
