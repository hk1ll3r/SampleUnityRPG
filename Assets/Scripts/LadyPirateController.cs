using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LadyPirateController : AgentController
{
    public static readonly string LogTag = typeof(LadyPirateController).Name;

    public static Logger logger = new Logger(Debug.unityLogger.logHandler);

    enum AgentState {
        None = 0,
        Idle = 1,
        Walk = 2,
        Attack = 3,
    }

    public float scanRange;
    public float wanderRange;
    public float attackRange;
    bool _isAttacking = false;
    bool _isWandering = false;
    bool _isDead = false;
    private float lastAttackTime;

    public override bool IsAttacking {
        get { return _isAttacking; }
    }
    public override bool IsAlive {
        get { return !_isDead; }
    }
    private bool _isDamaging = false;

    NavMeshAgent navAgent;
    Animator animator;
    Vector3 _target;
    GameObject camp;

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_target, 0.2f);
        Gizmos.DrawLine(transform.position, _target);
    }

    // Start is called before the first frame update
    public override void Start() {
        base.Start();
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        camp = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        float playerDist = (transform.position - player.transform.position).magnitude;
        if (_isDead) {
            transform.position += 0.25f * Vector3.down * Time.deltaTime;
        } else if (_isDamaging) {
            if (animator.IsInTransition(0)
                && animator.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.damage")
            ) {
                _isDamaging = false;
            }
        } else if (_isAttacking) {
            if (animator.IsInTransition(0)
                && animator.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.attack1")
            ) {
                GetComponentInChildren<Weapon>().GetComponent<Collider>().enabled = false;
                _isAttacking = false;
            }
        } else if (playerDist < scanRange && player.IsAlive) {
            _isWandering = false;
            // logger.LogFormat(LogType.Log, "{0} player dist {1}", LogTag, playerDist);
            if (playerDist < attackRange) {
                if (Time.time - lastAttackTime > (1 / attackRate))
                {
                    // attack
                    animator.SetInteger("state", (int)DragonAnimationState.Attack);
                    _isAttacking = true;
                    GetComponentInChildren<Weapon>().GetComponent<Collider>().enabled = true;
                    audioSource.clip = attackClip;
                    audioSource.Play();
                    logger.LogFormat(LogType.Log, "{0} attacking...", LogTag);
                    lastAttackTime = Time.time;
                } else {
                    // idle
                    animator.SetInteger("state", (int)DragonAnimationState.Fly);
                }
            } else {
                // chase
                _target = player.transform.position;
                navAgent.SetDestination(_target);
                animator.SetInteger("state", (int)DragonAnimationState.Fly);
                logger.LogFormat(LogType.Log, "{0} chasing...", LogTag);
            }
        } else {
            if (!_isWandering) {
                _isWandering = true;
                logger.LogFormat(LogType.Log, "{0} begin wondering...", LogTag);
                animator.SetInteger("state", (int)DragonAnimationState.Fly);
                StartNewWander();
            } else { // already wandering
                if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance && (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)) {
                    // reached target, re-wander
                    logger.LogFormat(LogType.Log, "{0} reached dest, wandering again...", LogTag);
                    StartNewWander();
                }
            }
        }
    }

    public override void OnDie(AgentController killer) {
        _isDead = true;
        navAgent.enabled = false;
        // die animation
        GetComponent<Animator>().SetInteger("state", (int) DragonAnimationState.Die);
        StartCoroutine("CleanUp");
        audioSource.clip = dieClip;
        audioSource.Play();
    }

    private IEnumerator CleanUp() {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

    private void StartNewWander() {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRange;
        randomDirection += camp.transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, wanderRange, 1);
        _target = hit.position;
        navAgent.SetDestination(_target);
    }

    public override void OnAttacked(AgentController attacker) {
        if (attacker.team == this.team || _isDead || _isDamaging) return;

        Debug.Log("dragon attacked! "  + attacker.name);

        health -= attacker.GetComponentInChildren<Weapon>().damage;
        GetComponentInChildren<Weapon>().GetComponent<Collider>().enabled = false;
        _isAttacking = false;

        if (health < 0) {
            health = 0f;
            OnDie(attacker);
        } else {
            // damage animation
            audioSource.clip = damageClip;
            audioSource.Play();
            GetComponent<Animator>().SetInteger("state", (int) DragonAnimationState.Damage);
            _isDamaging = true;
        }
    }
}
