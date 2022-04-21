using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShadowMovment : MonoBehaviour
{

    Rigidbody rb;

    PlayerInput pInput;

    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    InputAction sprintAction;
    InputAction crouchAction;


    private void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
    }
}
