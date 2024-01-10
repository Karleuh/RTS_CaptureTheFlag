using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTest : MonoBehaviour
{
    private float speed = 2.0f;
    private float rotationSpeed = 100f;
    void Update()
    {

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(0,rotationSpeed * Time.deltaTime,0);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(0,-rotationSpeed * Time.deltaTime,0);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position -= transform.forward * speed * Time.deltaTime;
        }
    }
}
