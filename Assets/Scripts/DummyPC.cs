using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPC : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
	    Vector3 movement = new Vector3();
	    if (Input.GetKey(KeyCode.A))
		    movement.x += -4;
	    if (Input.GetKey(KeyCode.D))
		    movement.x += 4;
	    if (Input.GetKey(KeyCode.W))
		    movement.z += 4;
	    if (Input.GetKey(KeyCode.S))
		    movement.z -= 4;
	    transform.position += movement * Time.deltaTime;
    }
}
