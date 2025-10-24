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
            if (enemy != null && enemy.Fighter.IsCounterable && !meeleFighter.InAction)  //反击
            {
                StartCoroutine(meeleFighter.PerformCounterAttack(enemy));
            }
            else                                                                         //普通攻击
            {
                CombatMode = true;

                EnemyController enemyToAttack = EnemyManager.Instance.GetClosestEnemyToPlayerDir(playController.GetIntentDirection());
                if (enemyToAttack == null)
                    meeleFighter.TryToAttack();                         //朝面朝向空a
                else
                    meeleFighter.TryToAttack(enemyToAttack.Fighter);    //攻击距离最近敌人
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && !meeleFighter.IsTakingHit)
        {
            EnemyController enemy = EnemyManager.Instance.GetAttackingEnemy();

            CombatMode = true;

            EnemyController enemyToAttack = EnemyManager.Instance.GetClosestEnemyToPlayerDir(playController.InputDir);
            if (enemyToAttack == null)
                meeleFighter.TryToAttack(null,true);                    //朝面朝向空a
            else
                meeleFighter.TryToAttack(enemyToAttack.Fighter, true);  //攻击距离最近敌人
        }

        if (Input.GetButtonDown("LockOn"))
        {
            CombatMode = !CombatMode;
        }
    }

    //自定义根运动规则

    private void OnAnimatorMove()
    {
        if(!meeleFighter.InCounter)
            transform.position += animator.deltaPosition;
        transform.rotation *= animator.deltaRotation;
    }

    //获取玩家视线方向
    public Vector3 GetTargetingDir()
    {
        //非战斗状态 返回(玩家位置-摄像机位置),本质摄像机面朝向
        if (!CombatMode)
        {
            Vector3 vecFromCam = transform.position - cam.transform.position;
            vecFromCam.y = 0f;
            return vecFromCam.normalized;
        }
        //战斗状态下 返回玩家面朝向(战斗状态下 会出现玩家面朝向改变，但摄像机面朝向还未改变的情况)
        else
        {
            return transform.forward;
        }
    }
}
