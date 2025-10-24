using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GettingHitState : State<EnemyController>
{
    [SerializeField] float stunnTime = 0.5f;
    EnemyController enemy;

    public override void Enter(EnemyController owner)
    {
        //ֹ֮ͣǰ��Э�� ���⿨��
        StopAllCoroutines();

        enemy = owner;
        enemy.Fighter.OnHitComplete += () =>
        {
            StartCoroutine(GoToCombatMoveMent());
        };
    }

    IEnumerator GoToCombatMoveMent()
    {
        yield return new WaitForSeconds(stunnTime);
        if (enemy.IsInState(EnemyStates.Dead))
            yield return null;
        else
            enemy.ChangeState(EnemyStates.CombatMovement);
    }
}
