using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject Prefab;
	void Update()
    {
		if (Input.GetKeyDown(KeyCode.Z))
        {
            Instantiate(Prefab, transform.position, transform.rotation);
        }
    }
}
