using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonBall : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag.Equals("Plank"))
		{
			collision.gameObject.GetComponent<Rigidbody>().useGravity = true;
		}
	}
}
