using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed = 10;
    public float mouseSens = 400f;
    public float sprintSpeed = 2f;
    float currentSpeed = 0f;

    float xRotation = 0f;
    float yRotation = 180f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        currentSpeed = speed;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            // Rotation
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            xRotation -= mouseY * Time.deltaTime * mouseSens;
            yRotation += mouseX * Time.deltaTime * mouseSens;

            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localEulerAngles = new Vector3(xRotation, yRotation, 0f);
        }

        // WASD Movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = transform.forward * vertical + transform.right * horizontal;
        transform.Translate(direction * currentSpeed * Time.deltaTime, Space.World);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Sprint, Up & Down
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                currentSpeed = speed * sprintSpeed;
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                currentSpeed = speed;
            }

            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E))
            {
                transform.position += new Vector3(0f, transform.up.y * currentSpeed * Time.deltaTime, 0f);
            }
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.Q))
            {
                transform.position += new Vector3(0f, -transform.up.y * currentSpeed * Time.deltaTime, 0f);
            }
        }      
    }
}
