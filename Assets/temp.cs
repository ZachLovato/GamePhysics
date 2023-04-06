using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class temp : MonoBehaviour
{
    bool input = false;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X)) 
        {
            input = true;
        }
    }

	private void FixedUpdate()
	{
		
	}
}
