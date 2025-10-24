using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float h = 0f;
    float v = 0f;
    float moveAmount = 0f;
    Vector3 moveDir;
    //float speedSize = 0.5f;

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 500f;

    float ySpeed;
    Quaternion targetRotation;

    public Vector3 InputDir { get; private set; }

    CameraController cameraController;
    Animator animator;
    CharacterController characterController;
    MeeleFighter meeleFighter;
    CombatController combatController;

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        meeleFighter = GetComponent<MeeleFighter>();
        combatController = GetComponent<CombatController>();
    }
    private void Update()
    {
        //防止攻击时移动
        if (meeleFighter.InAction || meeleFighter.Health<=0)
        {
            targetRotation = transform.rotation;
            animator.SetFloat("forwardSpeed", 0f);//攻击时设置速度为0 避免攻击后还会进行移动动画
            return; 
        }

        //获取z轴和x轴的输入
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        moveAmount =Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        //单位化向量 避免沿对角线移动比在单个方向上移动快 为什么？
        moveDir = cameraController.PlanarRotation * (new Vector3(h, 0, v)).normalized;
        InputDir=moveDir;

        //添加重力方法一: 检测物体是否在地面 不在就手动给物体加一个重力加速度
        //优点:响应速度更快
        if (characterController.isGrounded)
        {
            ySpeed = -0.5f;
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }

        var velocity = moveDir * moveSpeed;
       
        //添加重力方法二: 使用SimpleMove 只要调用这个方法，物体就会受重力作用，即使传入的向量为0
        //这个方法下，人物水平移动特别慢，不知道为什么
        //if (!characterController.isGrounded)
        //{
        //    characterController.SimpleMove(Vector3.zero);
        //}

        if (combatController.CombatMode)
        {
            velocity /= 4f;

            //绕着敌人向右侧选择
            Vector3 targetVec = combatController.TargetEnemy.transform.position - transform.position;
            targetVec.y = 0;

            if (moveAmount > 0)
            {
                targetRotation = Quaternion.LookRotation(targetVec);
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                targetRotation, rotationSpeed * Time.deltaTime);
            }

            //设置速度参数 改变动画

            //z轴分速度
            float forwardSpeed = Vector3.Dot(velocity, transform.forward);
            animator.SetFloat("forwardSpeed", forwardSpeed / moveSpeed, 0.2f, Time.deltaTime);

            //x轴分速度
            float angle = Vector3.SignedAngle(transform.forward, velocity, Vector3.up);
            float strafeSpeed = Mathf.Sin(angle * Mathf.Deg2Rad);
            animator.SetFloat("strafeSpeed", strafeSpeed, 0.2f, Time.deltaTime);
        }
        else
        {
            if (moveAmount > 0)
            {
                targetRotation = Quaternion.LookRotation(moveDir);
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                targetRotation, rotationSpeed * Time.deltaTime);

            animator.SetFloat("forwardSpeed", moveAmount, 0.2f, Time.deltaTime);
        }

        velocity.y = ySpeed;
        characterController.Move(velocity * Time.deltaTime);

        #region 疾跑 遗弃
        //if (Input.GetKeyDown(KeyCode.LeftShift))
        //{
        //    speedSize = 1f;
        //}
        //if (Input.GetKeyUp(KeyCode.LeftShift))
        //{
        //    speedSize = 0.5f;
        //}
        #endregion
    }

    public Vector3 GetIntentDirection()
    {
        return InputDir != Vector3.zero ? InputDir : transform.forward;
    }
}
