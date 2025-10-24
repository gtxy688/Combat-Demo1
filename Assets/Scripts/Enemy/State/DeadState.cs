using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DeadState : State<EnemyController>
{
    MeeleFighter meeleFighter;
    private void Awake()
    {
        meeleFighter=GetComponent<MeeleFighter>();
    }
    public override void Enter(EnemyController owner)
    {
        owner.VisionSensor.gameObject.SetActive(false);
        EnemyManager.Instance.RemoveEnemyInRange(owner);

        owner.NavAgent.enabled = false;
        owner.CharacterController.enabled = false;
        //meeleFighter.PlayDeathAnimation(meeleFighter);
        Destroy(gameObject, 4f);
    }
}
