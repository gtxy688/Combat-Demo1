using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    EnemyController targetEnemy;
    PlayerController playController;
    public EnemyController TargetEnemy 
    {
        get => targetEnemy;
        set
        {
            targetEnemy = value;
            if(targetEnemy ==null)
                combatMode = false;
        }
    }

    bool combatMode = false; 
    public bool CombatMode
    {
        get => combatMode;
        set
        {
            combatMode = value;
            if (TargetEnemy == null)
                combatMode = false;
            animator.SetBool("combatMode", combatMode);
        }
    }

    MeeleFighter meeleFighter;
    Animator animator;
    CameraController cam;

    private void Awake()
    {
        meeleFighter = GetComponent<MeeleFighter>();
        animator = GetComponent<Animator>();
        cam = Camera.main.GetComponent<CameraController>();
        playController= GetComponent<PlayerController>();
    }

    private void Start()
    {
        meeleFighter.OnGoHit += (MeeleFighter attacker) =>
        {
            if(CombatMode && attacker!=TargetEnemy.Fighter)
            {
                TargetEnemy = attacker.GetComponent<EnemyController>();
            }

        };
    }

    private void Update()
    {
        if (Input.GetButtonDown("Attack") && !meeleFighter.IsTakingHit)
        {
            EnemyController enemy = EnemyManager.Instance.GetAttackingEnemy();
            if (enemy != null && enemy.Fighter.IsCounterable && !meeleFighter.InAction)  //����
            {
                StartCoroutine(meeleFighter.PerformCounterAttack(enemy));
            }
            else                                                                         //��ͨ����
            {
                CombatMode = true;

                EnemyController enemyToAttack = EnemyManager.Instance.GetClosestEnemyToPlayerDir(playController.GetIntentDirection());
                if (enemyToAttack == null)
                    meeleFighter.TryToAttack();                         //���泯���a
                else
                    meeleFighter.TryToAttack(enemyToAttack.Fighter);    //���������������
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && !meeleFighter.IsTakingHit)
        {
            EnemyController enemy = EnemyManager.Instance.GetAttackingEnemy();

            CombatMode = true;

            EnemyController enemyToAttack = EnemyManager.Instance.GetClosestEnemyToPlayerDir(playController.InputDir);
            if (enemyToAttack == null)
                meeleFighter.TryToAttack(null,true);                    //���泯���a
            else
                meeleFighter.TryToAttack(enemyToAttack.Fighter, true);  //���������������
        }

        if (Input.GetButtonDown("LockOn"))
        {
            CombatMode = !CombatMode;
        }
    }

    //�Զ�����˶�����

    private void OnAnimatorMove()
    {
        if(!meeleFighter.InCounter)
            transform.position += animator.deltaPosition;
        transform.rotation *= animator.deltaRotation;
    }

    //��ȡ������߷���
    public Vector3 GetTargetingDir()
    {
        //��ս��״̬ ����(���λ��-�����λ��),����������泯��
        if (!CombatMode)
        {
            Vector3 vecFromCam = transform.position - cam.transform.position;
            vecFromCam.y = 0f;
            return vecFromCam.normalized;
        }
        //ս��״̬�� ��������泯��(ս��״̬�� ���������泯��ı䣬��������泯��δ�ı�����)
        else
        {
            return transform.forward;
        }
    }
}
