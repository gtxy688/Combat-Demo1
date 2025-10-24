using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { Idle, CombatMovement, Attack, RetreatAfterAttack, Dead,GettingHit }

public class EnemyController : MonoBehaviour
{
    [field: SerializeField] public float Fov { get; private set; } = 180f;

    [field: SerializeField] public float AlertNearbyEnemiesInRange { get; private set; } = 20f;
    public float CombatMovementTimer { get; set; } = 0f;

    Dictionary<EnemyStates, State<EnemyController>> stateDic;

    public List<MeeleFighter> TargetsInRange { get; set; }=new List<MeeleFighter>();
    [field: SerializeField] public MeeleFighter Target { get; set; }
    public SkinnedMeshHighlighter MeshHighlighter { get; set; }
    public CharacterController CharacterController {get; private set;}
    public StateMachine<EnemyController> StateMachine { get; private set; }
    public NavMeshAgent NavAgent { get; private set; }
    public Animator Animator { get; private set; }
    public MeeleFighter Fighter { get; private set; }
    public VisionSensor VisionSensor { get; set; }

   
    private void Start()
    {
        NavAgent = GetComponent<NavMeshAgent>();
        CharacterController = GetComponent<CharacterController>();
        Animator = GetComponent<Animator>();
        Fighter = GetComponent<MeeleFighter>();
        MeshHighlighter = GetComponent<SkinnedMeshHighlighter>();

        stateDic=new Dictionary<EnemyStates, State<EnemyController>>();
        stateDic[EnemyStates.Idle]=GetComponent<IdleState>();
        stateDic[EnemyStates.CombatMovement]=GetComponent<CombatMovementState>();    
        stateDic[EnemyStates.Attack]=GetComponent<AttackState>();    
        stateDic[EnemyStates.RetreatAfterAttack]=GetComponent<RetreatAfterAttackState>();    
        stateDic[EnemyStates.Dead]=GetComponent<DeadState>();    
        stateDic[EnemyStates.GettingHit]=GetComponent<GettingHitState>();    

        StateMachine = new StateMachine<EnemyController>(this);
        StateMachine.ChangeState(stateDic[EnemyStates.Idle]);

        Fighter.OnGoHit +=(MeeleFighter attacker)=>
        {
            if (Fighter.Health > 0)
            {
                if (Target == null)
                {
                    Target = attacker;
                    AlertNearbyEnemies();
                }
                ChangeState(EnemyStates.GettingHit); 
            }
            else
                ChangeState(EnemyStates.Dead);
        };
    }
    public void ChangeState(EnemyStates state)
    {
        StateMachine.ChangeState(stateDic[state]);
    }

    public bool IsInState(EnemyStates state)
    {
       return StateMachine.CurrentState == stateDic[state];
    }

    Vector3 prevPos;
    Vector3 deltaPos = Vector3.zero;

    private void Update()
    {
        StateMachine.Execute();

        //位移
        //没有目标赋值为0 使用根运动时赋值为0
        if (Target == null || Animator.applyRootMotion)
            deltaPos = Vector3.zero;
        else
            deltaPos = transform.position - prevPos;
        //合速度
        Vector3 velocity = deltaPos / Time.deltaTime;

        //根据分速度大小播放动画

        //z轴分速度
        float forwardSpeed = Vector3.Dot(velocity, transform.forward);
        Animator.SetFloat("forwardSpeed", forwardSpeed/ NavAgent.speed,0.2f,Time.deltaTime);

        //x轴分速度
        float angle = Vector3.SignedAngle(transform.forward, velocity, Vector3.up);
        float strafeSpeed = Mathf.Sin(angle*Mathf.Deg2Rad);
        Animator.SetFloat("strafeSpeed", strafeSpeed, 0.2f, Time.deltaTime);

        if(Target!=null && Target.Health<=0)
        {
            TargetsInRange.Remove(Target);
            EnemyManager.Instance.RemoveEnemyInRange(this);
        }

        prevPos = transform.position;
    }

    public MeeleFighter FindTarget()
    {
        foreach (var target in TargetsInRange)
        {
            Vector3 vecToTarget = target.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, vecToTarget);

            if (angle <= Fov / 2)
            {
                return target;
            }
        }
        return null;
    } 

    public void AlertNearbyEnemies()
    {
        var colliders = Physics.OverlapBox(transform.position,new Vector3(AlertNearbyEnemiesInRange/2,1f, AlertNearbyEnemiesInRange / 2),
            Quaternion.identity,EnemyManager.Instance.EnemyLayer);
        foreach (var collider in colliders)
        {
            if (collider.gameObject == gameObject) continue;

            EnemyController nearbyEnemy = collider.GetComponent<EnemyController>();
            if(nearbyEnemy!=null && nearbyEnemy.Target == null)
            {
                nearbyEnemy.ChangeState(EnemyStates.CombatMovement);
                nearbyEnemy.Target = Target;
            }
        }
    }
}
