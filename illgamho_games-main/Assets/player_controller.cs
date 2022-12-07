using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_controller : MonoBehaviour
{
    /*
     유의사항 
    플레이어 rigidbody에 마찰 0인 physics material 2d 하나 추가하기

    물은 water tag, layer달고, 물 오브젝트의 collider는 is Trigger, used by effector 체크
    컴포넌트로 buoyancy effector 2d 추가 뒤, collider mask는 player만, density는 1로 설정
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
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float jumpHeight = 5f;
    protected bool can_jump => Input.GetButtonDown("Jump") && (coyote_time_counter > 0f) && !is_jump; //점프가 눌리고, 코요테 타임 기준이 충족될 시 점프가능


    
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
    [SerializeField] protected LayerMask ground_layer; //ground 레이어로 인스펙터에서 바꾸어놓기
    [SerializeField] protected LayerMask _cornerCorrectLayer; //이것도 같이
    [SerializeField] protected LayerMask water_layer; //Water 레이어로 인스펙터에서 바꾸어놓기

    [Header("Robot")]
    [SerializeField] private GameObject robot_obj; //로봇 오브젝트

    [Header("water_movement")]
    [SerializeField] private bool in_water = false;
    [SerializeField] private float max_water_y_spped = 2f;
    protected Vector2 water_lerp_speed = Vector2.zero;
    private float in_water_time = 0f;

    [Header("block_translation")]
    [SerializeField] public GameObject grab_block;
    [SerializeField] private KeyCode block_grab_key = KeyCode.G;
    [SerializeField] private Vector3 block_local_offset = new Vector3(0.5f, 0, 0);
    private Vector3 flip_vector = Vector3.zero;
    [SerializeField] private bool can_grab => (Input.GetKey(block_grab_key) && grab_block == null && !grab_cor);
    private bool can_grab_off = false;
    private bool grab_cor = false; //코루틴 on 인지 판별해줌

    private Vector2 velocity;
    
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
        
        if(grab_block != null)
        {
            if (get_axis_hor < 0)
            {
                flip_vector.x = gameObject.transform.position.x - block_local_offset.x;
                flip_vector.y = gameObject.transform.position.y + block_local_offset.y;
                flip_vector.z = gameObject.transform.position.z + block_local_offset.z;
                grab_block.transform.position = flip_vector;
            }
            else if(get_axis_hor >= 0)
            {
                grab_block.transform.position = gameObject.transform.position + block_local_offset;
            }
            
            if (Input.GetKey(block_grab_key) && can_grab_off == true)
            {
                Debug.Log("out block");
                grab_block.transform.SetParent(null);
                grab_block = null;
                if (!grab_cor)
                {
                    StartCoroutine(block_out_cool());
                }
            }
        }
    }

    private void FixedUpdate()
    {
        Collision_Check(); //충돌 체크 raycast기반

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
            lerp_speed.x = Mathf.Lerp(lerp_speed.x, get_axis_hor * max_speed, 1f); //플레이어 좌우이동
            //lerp_speed.x = Mathf.Lerp(get_axis_hor, max_speed, 0.7f);
            lerp_speed.y = player_rigid2d.velocity.y;
            player_rigid2d.velocity = lerp_speed;
        }
        else
        {
            coyote_time_counter -= Time.deltaTime; // 바닥아닐때 coyote counter 줄이기 0보다 작아지면 점프못함
            //lerp_speed.x = Mathf.Lerp(lerp_speed.x, get_axis_hor * max_speed * , 1f);
            // 공중에서 velocity +, - 일단 확인
            // 입력이 +  velocity의 Mathf.Sign(부호확인하는 함수) == veocity.x
            // 패스하셈
            // veocity.x => 최대속력인가?
            // 맞고, Sign 같으면 패스
            // 틀리고, Sign이 같으면 가속을 조금 넣어줘
            // 두개가 달라
            // 그럼 감속을 해야지
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
        //점프 메커니즘
        if (!in_water)
        {
            if (can_jump) //점프시작
            {
                is_jump = true;
                jump_time = 0f;
            }

            if (is_jump) //점프진행
            {
                jump_vector.y = jump_amount;
                player_rigid2d.velocity = jump_vector; //velocity바꾸어주어 점프진행

                coyote_time_counter = 0f; //coyote time counter 초기화

                jump_time += Time.deltaTime; //jump시간 계산
            }

            if (Input.GetKeyUp(KeyCode.Space) | jump_time > button_time) //(점프키를 띄거나 키다운 시간이 초과되면 점프 해제 (이를 통해 점프 길게 누를 시 더 높이 점프하게 가능)
            {
                is_jump = false;
            }

            if (player_rigid2d.velocity.y >= 0) //떨어질 때 중력 조절
            {
                player_rigid2d.gravityScale = default_gravity_scale;
            }
            else if (player_rigid2d.velocity.y < 0)
            {
                player_rigid2d.gravityScale = default_gravity_scale;
                if (player_rigid2d.velocity.y <= max_falling_speed)
                {
                    //Debug.Log("max_falling");
                    jump_vector.y = Mathf.Lerp(player_rigid2d.velocity.y, max_falling_speed, 1f);
                    player_rigid2d.velocity = jump_vector;
                }
            }
        }
        


        //물속 점프
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
            water_jump_cool_time -= Time.deltaTime; //쿨타임 구현
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
        if(player_rigid2d.velocity.y < max_water_y_spped * (-1) * 0.7f)//진동 함수 만들어주기
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
        //water_lerp_speed.y = Mathf.Sin(in_water_time) * -4 - 3; // y축 진동 구현


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
            water_lerp_speed = Vector2.zero;//각종 벡터들 초기화
        }

        if(collision.tag == "block_trans")
        {
            
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
            lerp_speed = Vector2.zero;//각종 벡터들 초기화
        }

        if (collision.tag == "block_trans" && can_grab) //grab block eanble
        {
            //Debug.Log("block stay");
            
                Debug.Log("block_on");
                collision.transform.parent.SetParent(gameObject.transform);
                grab_block = collision.transform.parent.gameObject;
            if (!grab_cor)
            {
                Debug.Log("call cor");
                StartCoroutine(block_out_cool());
            }
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
            lerp_speed.x = player_rigid2d.velocity.x; //물 속에서 나왔을 때 부자연스럽게 하지 않기위해
            water_lerp_speed = Vector2.zero; //각종 벡터들 초기화
        }

        if (collision.tag == "block_trans")
        {

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

    IEnumerator block_out_cool()
    {
        grab_cor = true;
        can_grab_off = false;
        Debug.Log("grab cor on");
        yield return new WaitForSeconds(0.4f);
        can_grab_off = true;
        grab_cor = false;
    }
}
