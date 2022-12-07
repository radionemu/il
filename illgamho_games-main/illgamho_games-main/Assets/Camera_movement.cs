using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_movement : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform target;

    //when this get close to 0 moveset gets more smooth
    public float smoothing;

    private void FixedUpdate()
    {
        Vector3 targetPosition = new Vector3
    (target.position.x, target.position.y,
    transform.position.z);

        transform.position = Vector3.Lerp
        (transform.position,
        targetPosition, smoothing * Time.deltaTime);
    }
}
