using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowController : MonoBehaviour
{
    public GameObject target;

    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // place camera 2 units behind and .5 units above, looking at target 
        cam.transform.position = target.transform.position
            - 2 * target.transform.TransformDirection(Vector3.forward);
            //+ target.transform.TransformDirection(Vector3.up);
        cam.transform.LookAt(target.transform);
    }
}
