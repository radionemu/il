using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class robot_controller : player_controller
{
    [Header("robot_controller")]
    [SerializeField]
    private bool control_robot = false;
    [SerializeField]
    private GameObject plalyer_obj;
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private KeyCode ride_key_code = KeyCode.Z;
    [SerializeField]
    private float distance = 0.5f;

    [Header("player_info")]
    [SerializeField] private player_controller player_controller_cs;

    // Start is called before the first frame update
    void Start()
    {
        player_controller_cs = plalyer_obj.GetComponent<player_controller>();
        player_rigid2d = gameObject.GetComponent<Rigidbody2D>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

        if(control_robot == true)
        {
            if (Input.GetKey(ride_key_code))
            {
                detach();
            }

            get_axis_hor = Input.GetAxis("Horizontal");
            jump_vector.x = player_rigid2d.velocity.x;
            jump();

            

        }else if(control_robot == false)
        {
            if (Mathf.Abs(Vector2.Distance(plalyer_obj.transform.position, gameObject.transform.position)) <= distance)
            {
                spriteRenderer.color = Color.green;
                if (Input.GetKey(ride_key_code) && player_controller_cs.grab_block == null) //block grab할때 못타게 막기
                {
                    ride();
                }
            }
            else
            {
                spriteRenderer.color = Color.red;
            }
        }
    }

    private void FixedUpdate()
    {
        if (control_robot)
        {
            Collision_Check(); //충돌 체크 raycast기반

            if (_canCornerCorrect) CornerCorrect();


            move();
        }
        
    }

    private void ride()
    {
        plalyer_obj.transform.SetParent(gameObject.transform);
        plalyer_obj.SetActive(false);
        control_robot = true;
    }

    private void detach()
    {
        control_robot = false;
        plalyer_obj.transform.SetParent(null);
        plalyer_obj.SetActive(true);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        //ground_ray
        Gizmos.DrawLine(transform.position + ground_Raycast_Offest, transform.position + ground_Raycast_Offest + Vector3.down * ground_Raycast_Length);
        Gizmos.DrawLine(transform.position - ground_Raycast_Offest, transform.position - ground_Raycast_Offest + Vector3.down * ground_Raycast_Length);

        //corner_ray
        Gizmos.DrawLine(transform.position + _edgeRaycastOffset, transform.position + _edgeRaycastOffset + Vector3.up * _topRaycastLength);
        Gizmos.DrawLine(transform.position - _edgeRaycastOffset, transform.position - _edgeRaycastOffset + Vector3.up * _topRaycastLength);
        Gizmos.DrawLine(transform.position + _innerRaycastOffset, transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength);
        Gizmos.DrawLine(transform.position - _innerRaycastOffset, transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength);


        //corner_distance_check
        Gizmos.DrawLine(transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength,
                        transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength + Vector3.left * _topRaycastLength);
        Gizmos.DrawLine(transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength,
                        transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength + Vector3.right * _topRaycastLength);
    }
}
