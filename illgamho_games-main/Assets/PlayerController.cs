using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Player_Contoller _inputAction;


    private Rigidbody2D rbody;

    
    private float directionX;
    private Vector2 desiredVelocity;

    //speed
    private Vector2 velocity;
    
    private float maxSpeed = 5f;
    private float acceleration, deceleration, turnsSpeed;
    [SerializeField]private float maxAcceleration = 5f;
    [SerializeField]private float maxDeceleration = -5f;
    [SerializeField]private float maxAirAcceleration = 3f;
    [SerializeField]private float maxAriDeceleration = -3f;
    [SerializeField] private float maxTurnSpeed = 2f;
    [SerializeField]private float maxAirTurnSpeed = 1f;

    private float maxSpeedChange;
    
    //Ground Check Variables
    private bool onGround;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        desiredVelocity = new Vector2(directionX, 0) * maxSpeed;
    }

    private void FixedUpdate()
    {
        //추후 추가 true 아님!!
        onGround = true;
        
        acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        deceleration = onGround ? maxDeceleration : maxAriDeceleration;
        turnsSpeed = onGround ? maxTurnSpeed : maxAirTurnSpeed;

        if (directionX != 0)
        {
            if (Mathf.Sign(directionX) != Mathf.Sign(velocity.x))
            {
                maxSpeedChange = turnsSpeed * Time.deltaTime;
            }
            else
            {
                maxSpeedChange = acceleration * Time.deltaTime;
            }
        }
        else
        {
            maxSpeedChange = deceleration * Time.deltaTime;
        }

        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        rbody.velocity = velocity;
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        directionX = context.ReadValue<float>();
    }

}
