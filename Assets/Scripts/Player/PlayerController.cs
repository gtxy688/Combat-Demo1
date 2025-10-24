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
        //��ֹ����ʱ�ƶ�
        if (meeleFighter.InAction || meeleFighter.Health<=0)
        {
            targetRotation = transform.rotation;
            animator.SetFloat("forwardSpeed", 0f);//����ʱ�����ٶ�Ϊ0 ���⹥���󻹻�����ƶ�����
            return; 
        }

        //��ȡz���x�������
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        moveAmount =Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        //��λ������ �����ضԽ����ƶ����ڵ����������ƶ��� Ϊʲô��
        moveDir = cameraController.PlanarRotation * (new Vector3(h, 0, v)).normalized;
        InputDir=moveDir;

        //�����������һ: ��������Ƿ��ڵ��� ���ھ��ֶ��������һ���������ٶ�
        //�ŵ�:��Ӧ�ٶȸ���
        if (characterController.isGrounded)
        {
            ySpeed = -0.5f;
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }

        var velocity = moveDir * moveSpeed;
       
        //�������������: ʹ��SimpleMove ֻҪ�����������������ͻ����������ã���ʹ���������Ϊ0
        //��������£�����ˮƽ�ƶ��ر�������֪��Ϊʲô
        //if (!characterController.isGrounded)
        //{
        //    characterController.SimpleMove(Vector3.zero);
        //}

        if (combatController.CombatMode)
        {
            velocity /= 4f;

            //���ŵ������Ҳ�ѡ��
            Vector3 targetVec = combatController.TargetEnemy.transform.position - transform.position;
            targetVec.y = 0;

            if (moveAmount > 0)
            {
                targetRotation = Quaternion.LookRotation(targetVec);
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                targetRotation, rotationSpeed * Time.deltaTime);
            }

            //�����ٶȲ��� �ı䶯��

            //z����ٶ�
            float forwardSpeed = Vector3.Dot(velocity, transform.forward);
            animator.SetFloat("forwardSpeed", forwardSpeed / moveSpeed, 0.2f, Time.deltaTime);

            //x����ٶ�
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

        #region ���� ����
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
