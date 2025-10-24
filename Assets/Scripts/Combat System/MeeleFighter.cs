using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 攻击状态枚举
/// </summary>
public enum AttackStates
{
    /// <summary>
    /// 未攻击
    /// </summary>
    Idle,
    /// <summary>
    /// 攻击抬手
    /// </summary>
    Windup,
    /// <summary>
    /// 伤害判定
    /// </summary>
    Impact,
    /// <summary>
    /// 攻击结束
    /// </summary>
    Cooldown
}

public class MeeleFighter : MonoBehaviour
{
    [field:SerializeField] public float Health { get; private set; } = 25f;

    [SerializeField] float rotationSpeed = 550f;

    [SerializeField] List<AttackData> attacks;
    [SerializeField] List<AttackData> longRangeAttacks;
    //[SerializeField] float longRangeAttackThreshold = 1.5f;

    [SerializeField] GameObject sword;
    public event UnityAction<MeeleFighter> OnGoHit;
    public event UnityAction OnHitComplete;

    BoxCollider swordCollider;
    SphereCollider leftHandCollider,rightHandCollider,leftFootCollider,rightFootCollider;
    Animator animator;
    MeeleFighter currTarget;
    
    bool doCombo;
    int comboCount = 0;
    
    public AttackStates AttackState { get; private set; }
    public List<AttackData> Attacks => attacks;
    public bool InAction { get; private set; } = false;
    public bool InCounter { get; set; } = false;
    public bool IsCounterable => AttackState == AttackStates.Windup && comboCount == 0;
    public bool IsTakingHit { get; private set; } = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    private void Start()
    {
        leftHandCollider = animator.GetBoneTransform(HumanBodyBones.LeftHand).GetComponent<SphereCollider>();
        rightHandCollider = animator.GetBoneTransform(HumanBodyBones.RightHand).GetComponent<SphereCollider>();
        leftFootCollider = animator.GetBoneTransform(HumanBodyBones.LeftFoot).GetComponent<SphereCollider>();
        rightFootCollider = animator.GetBoneTransform(HumanBodyBones.RightFoot).GetComponent<SphereCollider>();
        if (sword != null)
        {
            //失活武器碰撞器
            swordCollider = sword.GetComponent<BoxCollider>();
        }
        DisableAllHitboxes();
    }

    public void TryToAttack(MeeleFighter target = null , bool isLongAttack = false)
    {
        //判断inAction条件，避免攻击打断攻击
        if (!InAction)
        {
            //在协程中实现攻击逻辑
            StartCoroutine(Attack(target,isLongAttack));
        }
        //只有普通攻击能连招
        else if((AttackState== AttackStates.Impact || AttackState == AttackStates.Cooldown) && !isLongAttack)
        {
            doCombo = true;
        }
    }

    IEnumerator Attack(MeeleFighter target=null,bool isLongAttack=false)
    {
        InAction = true;
        currTarget=target;
        AttackState = AttackStates.Windup;
        AttackData attack = isLongAttack && longRangeAttacks.Count>0 ? longRangeAttacks[0] : attacks[comboCount];
        Vector3 attackDir = transform.forward;
        Vector3 startPos=transform.position;
        Vector3 targetPos = Vector3.zero;

        if (target != null)
        {
            Vector3 vecToTarget =  target.transform.position- transform.position ;
            vecToTarget.y = 0;

            attackDir = vecToTarget.normalized;
            float distance=vecToTarget.magnitude-attack.DistanceFromTarget;

            //if (distance > longRangeAttackThreshold)
            //{
            //    attack = longRangeAttacks[0];
            //}

            if(attack.MoveToTarget)
            { 
                if (distance < attack.MaxMoveDistance)
                    targetPos = target.transform.position - attackDir * attack.DistanceFromTarget;
                else
                    targetPos = startPos + attackDir * attack.MaxMoveDistance;
            }
        }
        //没有敌人，就朝面朝向移动
        else
        {
            if (attack.MoveToTarget)
            {
                targetPos = startPos + attackDir * attack.MaxMoveDistance;
            }
        }

        //使用CrossFade来实现状态转换
        animator.CrossFade(attack.AnimName, 0.2f); //Slash动画的前20%参与过渡

        yield return null; //为什么这里要等一帧

        //获取要播放的动画状态信息
        var animState = animator.GetNextAnimatorStateInfo(1);

        float timer = 0f;
        while (timer <= animState.length)
        {
            if (IsTakingHit) break;
            timer += Time.deltaTime;
            
            if(target!=null && attack.MoveToTarget)
            {
                float percTime= (timer / animState.length-attack.MoveStartTime)/(attack.MoveEndTime-attack.MoveStartTime);
                transform.position = Vector3.Lerp(startPos, targetPos,percTime);
            }

            //调整玩家方向为攻击方向
            if (attackDir != null)
            {
               transform.rotation = Quaternion.RotateTowards(transform.rotation, 
                   Quaternion.LookRotation(attackDir), rotationSpeed * Time.deltaTime);
            }

            switch (AttackState)
            {
                case AttackStates.Windup:
                    if (timer >= attack.ImpactStartTime * animState.length)
                    {
                        if (InCounter)
                            break;
                        AttackState = AttackStates.Impact;
                        EnableHitbox(attack);
                    }
                    break;
                case AttackStates.Impact:
                    if (timer >= attack.ImpactEndTime * animState.length)
                    {
                        AttackState = AttackStates.Cooldown;
                        DisableAllHitboxes();
                    }
                    break;
                case AttackStates.Cooldown:
                    //连击相关
                    if (doCombo)
                    {
                        doCombo = false;
                        //3次一循环
                        comboCount = ++comboCount % attacks.Count;
                        //进行下一招
                        StartCoroutine(Attack(target));

                        yield break;
                    }
                    break;
            }
            yield return null;
        }

        //重置连击数
        comboCount = 0;
        AttackState=AttackStates.Idle;
        InAction = false;
        currTarget = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hitbox") && !IsTakingHit && !InCounter) 
        {
            MeeleFighter attacker = other.GetComponentInParent<MeeleFighter>();
            if (attacker.currTarget == this)
            {
                TakeDamage(5f);
                OnGoHit?.Invoke(attacker);
                if (Health>0)
                    StartCoroutine(PlayHitReaction(attacker));
                else
                {
                    PlayDeathAnimation(attacker);
                    GetComponent<EnemyController>().ChangeState(EnemyStates.Dead);
                }
            }
            else
                return;
        }
    }

    void TakeDamage(float damage)
    {
        Health = Mathf.Clamp(Health-damage,0,Health);
    }

    IEnumerator PlayHitReaction(MeeleFighter attacker)
    {
        InAction = true;
        IsTakingHit = true;

        //受击改变面朝向
        Vector3 dispVec = attacker.transform.position - transform.position;
        dispVec.y = 0;
        transform.rotation=Quaternion.LookRotation(dispVec);

        animator.CrossFade("SwordImpact", 0.2f);
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(1);

        yield return new WaitForSeconds(animState.length*0.8f);

        OnHitComplete?.Invoke();
        InAction = false;
        IsTakingHit = false;
    }

    public void PlayDeathAnimation(MeeleFighter attacker)
    {
        animator.CrossFade("FallBackDeath",0.2f);
    }

    public IEnumerator PerformCounterAttack(EnemyController opponent)
    {
        InAction = true;
        InCounter = true;
        opponent.Fighter.InCounter = true;
        opponent.ChangeState(EnemyStates.Dead);
        Destroy(opponent.gameObject, 4f);

        Vector3 dispVec = opponent.transform.position - transform.position;
        dispVec.y = 0f;
        
        transform.rotation=Quaternion.LookRotation(dispVec);
        opponent.transform.rotation = Quaternion.LookRotation(-dispVec);

        Vector3 targetPos = opponent.transform.position - dispVec.normalized * 1f;

        animator.CrossFade("CounterAttack", 0.2f);
        opponent.Animator.CrossFade("CounterAttackVictim", 0.2f);
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(1);

        float timer = 0f;
        while (timer <= animState.length)
        {
            transform.position=Vector3.MoveTowards(transform.position,targetPos, 5 * Time.deltaTime);

            yield return null;

            timer += Time.deltaTime;
        }
      
        InAction = false;
        InCounter = false;
        opponent.Fighter.InCounter = false;
    }

    void EnableHitbox(AttackData attack)
    {
        switch (attack.HitboxToUse)
        {
            case AttackHitbox.LeftHand:
                if (leftHandCollider != null)
                    leftHandCollider.enabled = true;
                break;
            case AttackHitbox.RightHand:
                if (rightHandCollider != null)
                    rightHandCollider.enabled = true;
                break;
            case AttackHitbox.LeftFoot:
                if (leftFootCollider != null)
                    leftFootCollider.enabled = true;
                break;
            case AttackHitbox.RightFoot:
                if (rightFootCollider != null)
                    rightFootCollider.enabled = true;
                break;
            case AttackHitbox.Sword:
                if (swordCollider != null)
                    swordCollider.enabled = true;
                break;
            default:
                break;
        }
    }

    void DisableAllHitboxes()
    {
        if(leftFootCollider != null)
            leftHandCollider.enabled = false;
        if (leftFootCollider != null)
            leftFootCollider.enabled = false;
        if (rightHandCollider != null)
            rightHandCollider.enabled = false;
        if(rightFootCollider != null)
            rightFootCollider.enabled = false;
        if (swordCollider != null)
            swordCollider.enabled = false;
    }
}




#region 状态转换事件(无用)
////一开始，我选择在动画中添加事件调用来实现状态转换
////而不是视频中的协程
////但到后面的连招系统，就必须使用协程来实现了
////等学完Unitask后来进行一个优化
//public void SetImpact()
//{
//    if(AttackState == AttackStates.Windup)
//    {
//        AttackState = AttackStates.Impact;
//        swordCollider.enabled = true;
//    }
//}

//public void SetCoolDown()
//{
//    if (AttackState == AttackStates.Impact)
//    { 
//        AttackState = AttackStates.Cooldown;
//        swordCollider.enabled = false;
//    }
//}

//public void SetInAction()
//{
//    //设置攻击状态为未攻击
//    AttackState = AttackStates.Idle;
//    //攻击动画结束后调用事件 设置inAction为false
//    inAction = false;
//}
#endregion