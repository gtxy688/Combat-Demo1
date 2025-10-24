using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] Vector2 timeRangeBetweenAttacks = new Vector2(1, 4);

    [SerializeField] CombatController player;

    [field:SerializeField] public LayerMask EnemyLayer { get; private set; }

    float findEnemyTimer = 0f;

    public static EnemyManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    List<EnemyController> enemiesInRange = new List<EnemyController>();
    float notAttackingTimer = 2;

    public void AddEnemyInRange(EnemyController enemy)
    {
        if (!enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
        }
    }
    public void RemoveEnemyInRange(EnemyController enemy)
    {
        enemiesInRange.Remove(enemy);
        if (enemy == player.TargetEnemy)
        {
            enemy.MeshHighlighter.HighlightMesh(false);
            player.TargetEnemy = null;
        }
    }

    private void Update()
    {
        if (enemiesInRange.Count == 0)
            return;
        //如果不存在处于攻击状态的敌人,就开始挑选攻击对象
        if (!enemiesInRange.Any(e => e.IsInState(EnemyStates.Attack)))
        {
            if (notAttackingTimer > 0)
            {
                notAttackingTimer -= Time.deltaTime;
            }
            if (notAttackingTimer <= 0)
            {
                //选择攻击者
                EnemyController attackingEnemy = SelectEnemyForAttack();
                if (attackingEnemy != null)
                {
                    attackingEnemy.ChangeState(EnemyStates.Attack);
                    notAttackingTimer = Random.Range(timeRangeBetweenAttacks.x, timeRangeBetweenAttacks.y);
                }
            }
        }

        //降低调用频率 
        if (findEnemyTimer > 0.1f)
        {
            findEnemyTimer = 0f;
            EnemyController closetEnemy = GetClosestEnemyToPlayerDir();
            if (closetEnemy == null)
                return;

            if (player.TargetEnemy == null)
            {
                player.TargetEnemy = closetEnemy;
                player.TargetEnemy.MeshHighlighter.HighlightMesh(true);
            }
            else if (closetEnemy != player.TargetEnemy && player.TargetEnemy != null)
            {
                EnemyController preEnemy = player.TargetEnemy;
                preEnemy.MeshHighlighter.HighlightMesh(false);
                player.TargetEnemy = closetEnemy;
                player.TargetEnemy.MeshHighlighter.HighlightMesh(true);
            }
        }
        findEnemyTimer += Time.deltaTime;
    }

    EnemyController SelectEnemyForAttack()
    {
        //返回等待攻击时间最长且此时有攻击目标的
        return enemiesInRange.OrderByDescending(e => e.CombatMovementTimer)
            .FirstOrDefault(e => e.Target != null && e.IsInState(EnemyStates.CombatMovement));
    }

    public EnemyController GetAttackingEnemy()
    {
        //返回数组中满足条件的第一个元素，没有就返回null
        return enemiesInRange.FirstOrDefault(e => e.IsInState(EnemyStates.Attack));
    }

    public EnemyController GetClosestEnemyToPlayerDir()
    {
        Vector3 targetingDir = player.GetTargetingDir();

        float minDistance = Mathf.Infinity;
        EnemyController closestEnemy = null;

        //寻找索敌范围覆盖玩家的敌人
        foreach (EnemyController enemy in enemiesInRange)
        {
            Vector3 vecToEnemy = enemy.transform.position - player.transform.position;
            vecToEnemy.y = 0;

            float angle = Vector3.Angle(targetingDir, vecToEnemy);
            float distance = vecToEnemy.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy;
            }
        }
        return closestEnemy;
    }

    public EnemyController GetClosestEnemyToPlayerDir(Vector3 direction)
    {
        float minDistance = Mathf.Infinity;
        EnemyController closestEnemy = null;

        //寻找索敌范围覆盖玩家的敌人
        foreach (EnemyController enemy in enemiesInRange)
        {
            Vector3 vecToEnemy = enemy.transform.position - player.transform.position;
            vecToEnemy.y = 0;

            float angle = Vector3.Angle(direction, vecToEnemy);
            float distance = vecToEnemy.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy;
            }
        }
        return closestEnemy;
    }
}
