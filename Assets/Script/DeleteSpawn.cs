using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteSpawn : MonoBehaviour
{
	[SerializeField] GameObject Prefab;
	 GameObject activeObj;
	private void Start()
	{
		activeObj = Instantiate(Prefab, transform.position, transform.rotation);
	}
	void Update()
	{
			if (Input.GetKeyDown(KeyCode.V))
			{
				Destroy(activeObj);
				activeObj = Instantiate(Prefab, transform.position, transform.rotation);
			}
	}
}
