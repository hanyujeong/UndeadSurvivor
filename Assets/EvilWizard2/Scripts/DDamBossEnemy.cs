using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(1)]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class DDamBossEnemy : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public float damage = 3f;
    public Rigidbody2D target;
    public GameObject speechBubble;

    public float closeDistance = 5f;
    private float curCloseDistance = 99999;

    private bool isRun = false;
    private bool canAttack = true;
    private bool isLive = true;
    private bool canChangeHitColor = true;
    public Color hitColor = Color.white;

    private Rigidbody2D rigid;
    private Collider2D coll;
    private Animator anim;
    private SpriteRenderer spriter;
    private WaitForFixedUpdate wait;
    private WaitForSeconds waitPointFiveSec = new WaitForSeconds(0.5f);

    private const string BulletString = "Bullet";
    
    private const string RunString = "Run";
    private const string HitString = "Hit";
    private const string Attack1String = "Attack1";
    private const string Attack2String = "Attack2";
    private const string DeadString = "Dead";
    
    private readonly int RunHash = Animator.StringToHash(RunString);
    private readonly int Attack1Hash = Animator.StringToHash(Attack1String);
    private readonly int Attack2Hash = Animator.StringToHash(Attack2String);
    private readonly int DeadHash = Animator.StringToHash(DeadString);

    private void Awake()
    {
        TryGetComponent(out rigid);
        TryGetComponent(out coll);
        TryGetComponent(out anim);
        TryGetComponent(out spriter);
        
        wait = new WaitForFixedUpdate();
    }
    
    private void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 3;
        // anim.SetBool(DeadHash, false);
        health = maxHealth;
        
        isRun = false;
        canAttack = true;
        canChangeHitColor = true;
        speechBubble.SetActive(false);
        closeDistance *= closeDistance;
    }
    
    private void FixedUpdate()
    {
        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName(HitString)) { return; }

        if (IsEnoughCloseToPlayer())
        {
            Attack();
            return;
        }

        Run();
        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.angularVelocity = 0;
    }

    private void LateUpdate()
    {
        if (!isLive) { return; }

        spriter.flipX = target.position.x < rigid.position.x;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(BulletString)|| !isLive) { return; }

        health -= collision.GetComponent<Bullet>().damage;
        StartCoroutine(KnockBack());
        
        if (health > 0)
        {
            Hit();
        }
        else
        {
            isLive = false;
            coll.enabled = false;
            rigid.simulated = false;
            spriter.sortingOrder = 0;
            anim.SetTrigger(DeadHash);
            GameManager.instance.kill++;
            // GameManager.instance.GetExp();

            Dead();
        }
    }
    
    private IEnumerator KnockBack() 
    {
        yield return wait;
        
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);

        yield return waitPointFiveSec;
        rigid.linearVelocity = Vector2.zero;
    }

    private void Hit()
    {
        StartCoroutine(HitColorChanging());
    }

    private IEnumerator HitColorChanging()
    {
        if(!canChangeHitColor) { yield break; }

        canChangeHitColor = false;
        spriter.material.color = hitColor;

        yield return waitPointFiveSec;

        canChangeHitColor = true;
        spriter.material.color = Color.white;
    }

    private void Run()
    {
        if(!isRun) { anim.SetBool(RunHash, true); }
        
        isRun = true;
    }

    private void Attack()
    {
        if(isRun) { anim.SetBool(RunHash, false); }
        isRun = false;
        
        if (!canAttack) { return; }
        canAttack = false;
        StartCoroutine(DelayAttack(curCloseDistance < closeDistance * 0.5f ? 1 : 0));
    }
    
    private IEnumerator DelayAttack(int type) 
    {
        yield return wait;
        
        switch (type)
        {
            case 0:
                anim.SetTrigger(Attack1Hash);
                break;
            case 1:
                anim.SetTrigger(Attack2Hash);
                break;
        }

        StartCoroutine(DelayDamageToPlayer());
    }

    private IEnumerator DelayDamageToPlayer()
    {
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.8f) { yield return null; }

        canAttack = true;
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if(!(stateInfo.IsName(Attack1String) || stateInfo.IsName(Attack2String))) { yield break; }
        
        GameManager.instance.health -= (int)damage;
    }
    
    private void Dead()
    { 
        isRun = false;
        isLive = false;

        StartCoroutine(DelayShowSpeechBubble());
    }

    private IEnumerator DelayShowSpeechBubble()
    {
        yield return waitPointFiveSec;
        yield return waitPointFiveSec;

        speechBubble.SetActive(true);
    }

    private bool IsEnoughCloseToPlayer()
    {
        curCloseDistance = (target.position - rigid.position).sqrMagnitude;
        return curCloseDistance < closeDistance;
    }
}
