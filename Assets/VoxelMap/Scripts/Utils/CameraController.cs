using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public float movementSpeed = 5.0f;
    public float sensitivity = 5.0f;

    public float maxX = 90;

    private Vector3 _eulerAngles;

    void Awake (){
        _eulerAngles = transform.eulerAngles;
    }
    // Update is called once per frame
    void Update()
    {
        // Move the camera forward, backward, left, and right
        if ( Input.GetMouseButton(1)){
            
            transform.position += transform.forward * Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
            transform.position += transform.right * Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;

            // Rotate the camera based on the mouse movement
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            _eulerAngles += new Vector3(-mouseY * sensitivity, mouseX * sensitivity, 0);
            _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, -maxX, maxX);

            transform.eulerAngles = _eulerAngles;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }else{
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
