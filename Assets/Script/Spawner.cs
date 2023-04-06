using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject Prefab;

    [Header("Timer")]
    [SerializeField] bool timed;
    [SerializeField, Range(.2f, 5)] float shotDelay;

    float timer = 1;

	void Update()
    {
        if (timed)
        {
            if (timer < 0)
            {
                Instantiate(Prefab, transform.position, transform.rotation);
                timer = timer = UnityEngine.Random.Range(.2f, shotDelay); ;
            }

            else timer -= Time.deltaTime;
		}
        else
        {
			if (Input.GetKeyDown(KeyCode.Z))
			{
				Instantiate(Prefab, transform.position, transform.rotation);
			}
		}
		
    }
}

//[CustomEditor(typeof(Spawner))]
//public class TestOnInspector : Editor
//{
	
//	public override void OnInspectorGUI()
//	{
//		serializedObject.Update();
//		//[SerializeField] bool yes;
//		GUIContent yesLabel = new GUIContent();
//	}
//}
