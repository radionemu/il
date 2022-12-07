using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_controller : MonoBehaviour
{
    /*
     ���ǻ��� 
    �÷��̾� rigidbody�� ���� 0�� physics material 2d �ϳ� �߰��ϱ�

    ���� water tag, layer�ް�, �� ������Ʈ�� collider�� is Trigger, used by effector üũ
    ������Ʈ�� buoyancy effector 2d �߰� ��, collider mask�� player��, density�� 1�� ����
     */


    [Header("components")]
    [SerializeField] protected Rigidbody2D player_rigid2d;

    [Header("horizontal_move_variables")]
    [SerializeField] protected float max_speed = 24f;
    protected float get_axis_hor = 0f;
    protected Vector2 lerp_speed = Vector2.zero;
    
    

    [Header("Key_Settings")]
    [SerializeField] protected KeyCode Jump = KeyCode.Space;

    [Header("gravity_variables")]
    [SerializeField] protected float default_gravity_scale = 1f;
    [SerializeField] protected float falling_gravity_scale = 2f;
    [SerializeField] protected float max_falling_speed = 10f;
    
    [Header("jump_variables")]
    [SerializeField] protected bool is_ground;
    [SerializeField] protected bool is_jump = false;
    [SerializeField] protected Vector2 jump_vector = Vector2.zero;
    [SerializeField] protected float jump_time = 0f;
    [SerializeField] protected float jump_amount = 50;
    [SerializeField] protected float button_time = 0.3f;
    [SerializeField] protected float coyote_time = .1f;
    [SerializeField] protected float coyote_time_counter;
    protected bool can_jump => Input.GetButtonDown("Jump") && (coyote_time_counter > 0f) && !is_jump; //������ ������, �ڿ��� Ÿ�� ������ ������ �� ��������


    
    [Header("water_jump_variables")]
    [SerializeField] private float button_time_water = 0.1f;
    [SerializeField] private bool is_water_jump = false;
    [SerializeField] private Vector2 water_jump_vector = Vector2.zero;
    [SerializeField] private float water_jump_time = 0f;
    [SerializeField] private float water_jump_amount = 50;
    [SerializeField] private float water_jump_cool_time = 0.5f;
    private bool can_water_jump => Input.GetKeyDown(Jump) && (water_jump_cool_time>=0.5f);

    [Header("Coner_Correct_variables")]
    [SerializeField] protected float _topRaycastLength;
    [SerializeField] protected Vector3 _edgeRaycastOffset;
    [SerializeField] protected Vector3 _innerRaycastOffset;
    protected bool _canCornerCorrect;

    [Header("ground_collision_variables")]
    [SerializeField] protected Vector3 ground_Raycast_Offest = Vector3.zero;
    [SerializeField] protected float ground_Raycast_Length;


    [Header("Layer_Mask")]
    [SerializeField] protected LayerMask ground_layer; //ground ���̾�� �ν����Ϳ��� �ٲپ����
    [SerializeField] protected LayerMask _cornerCorrectLayer; //�̰͵� ����
    [SerializeField] protected LayerMask water_layer; //Water ���̾�� �ν����Ϳ��� �ٲپ����

    [Header("Robot")]
    [SerializeField] private GameObject robot_obj; //�κ� ������Ʈ

    [Header("water_movement")]
    [SerializeField] private bool in_water = false;
    [SerializeField] private float max_water_y_spped = 2f;
    protected Vector2 water_lerp_speed = Vector2.zero;
    private float in_water_time = 0f;


    // Start is called before the first frame update
    void Start()
    {
        player_rigid2d = gameObject.GetComponent<Rigidbody2D>();
        //hit2D = new Ray2D(gameObject.transform.position,Vector2.down);
    }

    // Update is called once per frame
    void Update()
    {

        get_axis_hor = Input.GetAxis("Horizontal");
        jump_vector.x = player_rigid2d.velocity.x;
        
        jump();
        
        
    }

    private void FixedUpdate()
    {
        Collision_Check(); //�浹 üũ raycast���

        if (_canCornerCorrect) CornerCorrect();


        //move();

        
        if (in_water)
        {
            water_move();
        }
        else
        {
            move();
        }

    }

    protected void move()
    {
        if (is_ground)
        {
            //coyote_time_count
            coyote_time_counter = coyote_time;
            lerp_speed.x = Mathf.Lerp(lerp_speed.x, get_axis_hor * max_speed, 1f); //�÷��̾� �¿��̵�
            //lerp_speed.x = Mathf.Lerp(get_axis_hor, max_speed, 0.7f);
            lerp_speed.y = player_rigid2d.velocity.y;
            player_rigid2d.velocity = lerp_speed;
        }
        else
        {
            coyote_time_counter -= Time.deltaTime; // �ٴھƴҶ� coyote counter ���̱� 0���� �۾����� ��������
            //lerp_speed.x = Mathf.Lerp(lerp_speed.x, get_axis_hor * max_speed * , 1f);
            // ���߿��� velocity +, - �ϴ� Ȯ��
            // �Է��� +  velocity�� Mathf.Sign(��ȣȮ���ϴ� �Լ�) == veocity.x
            // �н��ϼ�
            // veocity.x => �ִ�ӷ��ΰ�?
            // �°�, Sign ������ �н�
            // Ʋ����, Sign�� ������ ������ ���� �־���
            // �ΰ��� �޶�
            // �׷� ������ �ؾ���
            if (get_axis_hor != 0)
            {
                lerp_speed.x = Mathf.Lerp(lerp_speed.x, get_axis_hor * max_speed * 1.2f, 0.04f);
            }
            else if (get_axis_hor == 0)
            {

            }


            lerp_speed.y = player_rigid2d.velocity.y;

            player_rigid2d.velocity = lerp_speed;
        }
    }
    protected void jump()
    {
        //���� ��Ŀ����
        if (!in_water)
        {
            if (can_jump) //��������
            {
                is_jump = true;
                jump_time = 0f;
            }

            if (is_jump) //��������
            {
                jump_vector.y = jump_amount;
                player_rigid2d.velocity = jump_vector; //velocity�ٲپ��־� ��������

                coyote_time_counter = 0f; //coyote time counter �ʱ�ȭ

                jump_time += Time.deltaTime; //jump�ð� ���
            }

            if (Input.GetKeyUp(KeyCode.Space) | jump_time > button_time) //(����Ű�� ��ų� Ű�ٿ� �ð��� �ʰ��Ǹ� ���� ���� (�̸� ���� ���� ��� ���� �� �� ���� �����ϰ� ����)
            {
                is_jump = false;
            }

            if (player_rigid2d.velocity.y >= 0) //������ �� �߷� ����
            {
                player_rigid2d.gravityScale = default_gravity_scale;
            }
            else if (player_rigid2d.velocity.y < 0)
            {
                player_rigid2d.gravityScale = falling_gravity_scale;
                if (player_rigid2d.velocity.y <= max_falling_speed)
                {
                    Debug.Log("max_falling");
                    jump_vector.y = Mathf.Lerp(player_rigid2d.velocity.y, max_falling_speed, 1f);
                    player_rigid2d.velocity = jump_vector;
                }
            }
        }
        


        //���� ����
        if (in_water)
        {
            if (can_water_jump)
            {
                is_water_jump = true;
                water_jump_time = 0f;
                water_jump_cool_time = 0.4f;
            }

            if (is_water_jump)
            {
                water_jump_vector.y = water_jump_amount;
                player_rigid2d.velocity = water_jump_vector;

                water_jump_time += Time.deltaTime;
            }

            if(Input.GetKeyUp(KeyCode.Space) | water_jump_time > button_time_water)
            {
                is_water_jump = false;
            }
        }

        if(water_jump_cool_time < 0.5f)
        {
            water_jump_cool_time -= Time.deltaTime; //��Ÿ�� ����
        }

        if(water_jump_cool_time < 0f)
        {
            water_jump_cool_time = 0.5f;
        }
        
    }


    protected void Collision_Check()
    {
        //ground_Check
        is_ground = (Physics2D.Raycast(transform.position + ground_Raycast_Offest, Vector2.down, ground_Raycast_Length, ground_layer)
                    || Physics2D.Raycast(transform.position - ground_Raycast_Offest, Vector2.down, ground_Raycast_Length, ground_layer));


        //corner_check
        _canCornerCorrect = Physics2D.Raycast(transform.position + _edgeRaycastOffset, Vector2.up, _topRaycastLength, _cornerCorrectLayer) &&
                            !Physics2D.Raycast(transform.position + _innerRaycastOffset, Vector2.up, _topRaycastLength, _cornerCorrectLayer) ||
                            Physics2D.Raycast(transform.position - _edgeRaycastOffset, Vector2.up, _topRaycastLength, _cornerCorrectLayer) &&
                            !Physics2D.Raycast(transform.position - _innerRaycastOffset, Vector2.up, _topRaycastLength, _cornerCorrectLayer);

    }

    protected void CornerCorrect()
    {
        //Push player to the right
        RaycastHit2D _hit = Physics2D.Raycast(transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength, Vector3.left, _topRaycastLength, _cornerCorrectLayer);
        if (_hit.collider != null)
        {
            //Debug.Log("Corner_Correct_active");
            float _newPos = Vector3.Distance(new Vector3(_hit.point.x, transform.position.y, 0f) + Vector3.up * _topRaycastLength,
                transform.position - _edgeRaycastOffset + Vector3.up * _topRaycastLength);
            transform.position = new Vector3(transform.position.x + _newPos, transform.position.y, transform.position.z);
            //player_rigid2d.velocity = new Vector2(player_rigid2d.velocity.x, 0f);
            //_rb.velocity = new Vector2(_rb.velocity.x, Yvelocity);
            return;
            
        }

        //Push player to the left
        _hit = Physics2D.Raycast(transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength, Vector3.right, _topRaycastLength, _cornerCorrectLayer);
        if (_hit.collider != null)
        {
            //Debug.Log("Corner_Correct_active");
            float _newPos = Vector3.Distance(new Vector3(_hit.point.x, transform.position.y, 0f) + Vector3.up * _topRaycastLength,
                transform.position + _edgeRaycastOffset + Vector3.up * _topRaycastLength);
            transform.position = new Vector3(transform.position.x - _newPos, transform.position.y, transform.position.z);
            //player_rigid2d.velocity = new Vector2(player_rigid2d.velocity.x, 0f);
            //_rb.velocity = new Vector2(_rb.velocity.x, Yvelocity);
        }
    }

    protected void water_move()
    {
        //player_rigid2d.velocity = Vector2.zero;

        /*
        if(player_rigid2d.velocity.y < max_water_y_spped * (-1) * 0.7f)//���� �Լ� ������ֱ�
        {
            water_lerp_speed.y = Mathf.Lerp(player_rigid2d.velocity.y,max_water_y_spped,1f);
            //player_rigid2d.velocity = water_lerp_speed;
        }
        else if(player_rigid2d.velocity.y > max_water_y_spped * 0.7f * 2)
        {
            water_lerp_speed.y = Mathf.Lerp(player_rigid2d.velocity.y,(-1)* max_water_y_spped , 1f);
            //player_rigid2d.velocity = water_lerp_speed;
        }*/
        //player_rigid2d.gravityScale = 0;
        //in_water_time += 0.2f;
        //water_lerp_speed.y = Mathf.Sin(in_water_time) * -4 - 3; // y�� ���� ����


        if (get_axis_hor != 0)
        {
            water_lerp_speed.x = Mathf.Lerp(water_lerp_speed.x, get_axis_hor * max_speed * 1.2f, 0.04f);
            //water_lerp_speed.y = player_rigid2d.velocity.y;
        }
        else if (get_axis_hor == 0)
        {
            //water_lerp_speed.y = player_rigid2d.velocity.y;
        }

        water_lerp_speed.y = player_rigid2d.velocity.y;

        player_rigid2d.velocity = water_lerp_speed;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "water" && 
            (Physics2D.Raycast(transform.position + ground_Raycast_Offest, Vector2.down, ground_Raycast_Length, water_layer)
            && Physics2D.Raycast(transform.position - ground_Raycast_Offest, Vector2.down, ground_Raycast_Length, water_layer)) &&!is_ground)
        {
            in_water = true;
            player_rigid2d.gravityScale = 1;
            coyote_time_counter = coyote_time;
            lerp_speed = Vector2.zero;
            water_lerp_speed = Vector2.zero;//���� ���͵� �ʱ�ȭ
        }

        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "water" &&
            (Physics2D.Raycast(transform.position + ground_Raycast_Offest, Vector2.down, ground_Raycast_Length, water_layer)
            && Physics2D.Raycast(transform.position - ground_Raycast_Offest, Vector2.down, ground_Raycast_Length, water_layer)) && !is_ground)
        {
            in_water = true;
            player_rigid2d.gravityScale = 1;
            coyote_time_counter = coyote_time;
            lerp_speed = Vector2.zero;//���� ���͵� �ʱ�ȭ
        }

        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "water"//&&
            /*!(Physics2D.Raycast(transform.position + ground_Raycast_Offest, Vector2.down, ground_Raycast_Length, water_layer)
                    && Physics2D.Raycast(transform.position - ground_Raycast_Offest, Vector2.down, ground_Raycast_Length, water_layer))*/)
        {
            in_water = false;
            lerp_speed = Vector2.zero;
            lerp_speed.x = player_rigid2d.velocity.x; //�� �ӿ��� ������ �� ���ڿ������� ���� �ʱ�����
            water_lerp_speed = Vector2.zero; //���� ���͵� �ʱ�ȭ
        }
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
