using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AICombatStates { Idle, Chase, Circling }

public class CombatMovementState : State<EnemyController>
{
    [SerializeField] float distanceToStand = 3f;
    [SerializeField] float adjustDistanceThreshold = 1f;
    [SerializeField] Vector2 idleTimeRange = new Vector2(2, 5);
    [SerializeField] Vector2 circlingTimeRange = new Vector2(3, 6);

    float idleTimer = 0f;
    float circlingTimer = 0f;

    int circlingDir = 1;

    AICombatStates state;

    EnemyController enemy;

    public override void Enter(EnemyController owner)
    {
        enemy = owner;
        enemy.Animator.SetBool("combatMode", true);
        enemy.NavAgent.stoppingDistance = distanceToStand;
        enemy.CombatMovementTimer = 0f;
        idleTimer = Random.Range(idleTimeRange.x, idleTimeRange.y);
        circlingTimer = Random.Range(circlingTimeRange.x, circlingTimeRange.y);
    }

    public override void Execute()
    {
        if (enemy.Target == null)
        {
            enemy.Target = enemy.FindTarget();
            if (enemy.Target == null)
            {
                enemy.ChangeState(EnemyStates.Idle);
                return;
            }
        }

        if(enemy.Target.Health<=0)
        {
            enemy.Target = null;
            enemy.ChangeState(EnemyStates.Idle);
            return;
        }

        if (Vector3.Distance(enemy.Target.transform.position, enemy.transform.position) > distanceToStand + adjustDistanceThreshold)
            StartChase();

        switch (state)
        {
            case AICombatStates.Idle:
                if (idleTimer <= 0)
                {
                    //静止了一段时间后，开始转圈
                    StartCircling();
                }
                else if (idleTimer > 0 && enemy.Target != null)
                    idleTimer -= Time.deltaTime;
                break;

            case AICombatStates.Chase:
                if (Vector3.Distance(enemy.Target.transform.position, enemy.transform.position) <= distanceToStand + 0.03f)
                {
                    StartIdle();
                    return;
                }
                enemy.NavAgent.SetDestination(enemy.Target.transform.position);
                break;

            case AICombatStates.Circling:
                if (circlingTimer <= 0)
                {
                    //转了一会，重新静止
                    StartIdle();
                    return;
                }
                else if (circlingTimer > 0 && enemy.Target != null)
                    circlingTimer -= Time.deltaTime;

                var vecToTarget = enemy.transform.position - enemy.Target.transform.position;
                var rotatePos = Quaternion.Euler(0, 20 * circlingDir * Time.deltaTime, 0) * vecToTarget;
                enemy.NavAgent.Move(rotatePos - vecToTarget);
                enemy.transform.rotation = Quaternion.LookRotation(-rotatePos);
                break;
        }

        if (enemy.Target != null)
            enemy.CombatMovementTimer += Time.deltaTime;
    }

    void StartChase()
    {
        state = AICombatStates.Chase;
    }

    void StartIdle()
    {
        state = AICombatStates.Idle;
        idleTimer = Random.Range(idleTimeRange.x, idleTimeRange.y);
    }

    void StartCircling()
    {
        state = AICombatStates.Circling;

        enemy.NavAgent.ResetPath();
        circlingTimer = Random.Range(circlingTimeRange.x, circlingTimeRange.y);
        //随机选择方向
        circlingDir = Random.Range(0, 2) == 0 ? 1 : -1;

    }

    public override void Exit()
    {
        enemy.CombatMovementTimer = 0f;
    }
}