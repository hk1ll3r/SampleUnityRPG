using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatLiteController : AgentController {
    public static readonly string LogTag = typeof(CatLiteController).Name;

    public static Logger logger = new Logger(Debug.unityLogger.logHandler);

    public float wanderRange;

    bool _isWandering = false;
    bool _isFollowing = false;
    bool _isIdling = true;
    float _idleEndTime = 0f;

    HighlightTarget highlightTarget;
    NavMeshAgent navAgent;
    Animator animator;
    Vector3 _target;
    public Vector3 home;

    public override bool IsAttacking {
        get {
            return false;
        }
    }

    public override bool IsAlive {
        get {
            return true;
        }
    }

    public bool IsTalking {
        get { return highlightTarget.InInteraction; }
    }

    void OnDrawGizmos() {
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
        highlightTarget = GetComponentInChildren<HighlightTarget>();
    }

    // Update is called once per frame
    void Update() {
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        float playerDist = (transform.position - player.transform.position).magnitude;
        if (IsTalking && player.IsAlive) {
            navAgent.isStopped = true;
            animator.SetInteger("state", (int)CatLiteAnimationState.Idle);
        } else if (_isFollowing && player.IsAlive) {
            _target = player.transform.position;
            float dist = (transform.position - _target).magnitude;
            if (dist < 1f) {
                navAgent.isStopped = true;
                animator.SetInteger("state", (int)CatLiteAnimationState.Idle);
            } else if (dist < 10f) {
                navAgent.SetDestination(_target);
                navAgent.isStopped = false;
                animator.SetInteger("state", (int)CatLiteAnimationState.Walk);
                navAgent.speed = 3.5f;
            } else {
                navAgent.SetDestination(_target);
                navAgent.isStopped = false;
                animator.SetInteger("state", (int)CatLiteAnimationState.Run);
                navAgent.speed = 6f;
            }
        } else {
            if (_isWandering && !navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance && (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
                || _isIdling && Time.time > _idleEndTime) {
                // reached target, decide what to do
                _isIdling = false;
                _isWandering = false;
                StartNewIdle();
            } else if (_isIdling
                && animator.IsInTransition(0)
                && animator.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.idleB")) {
                    animator.SetInteger("state", (int)CatLiteAnimationState.Idle);
                }
        }
    }

    private void StartNewIdle() {
        if (Random.Range(0, 2) == 0) {
            navAgent.isStopped = true;
            animator.SetInteger("state", (int)CatLiteAnimationState.Idle2);
            _idleEndTime = Time.time + Random.Range(3f, 10f);
            _isIdling = true;
        } else {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRange;
            randomDirection += home;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, wanderRange, 1);
            _target = hit.position;
            navAgent.SetDestination(_target);
            navAgent.isStopped = false;
            navAgent.speed = 3.5f;
            _isWandering = true;
            animator.SetInteger("state", (int)CatLiteAnimationState.Walk);
        }
    }

    public override void OnAttacked(AgentController attacker) {
        return;
    }

    protected override void HandleEvent(string eventName, params object[] eventParams) {
        logger.LogFormat(LogType.Log, "{0} event {1}", LogTag, eventName);
        if (eventName == "cat.follow") {
            _isFollowing = true;
            highlightTarget.enabled = false;
        } else if (eventName == "quests.cat.state" && (float) eventParams[0] == 2f) {
            _isFollowing = false;
            home = transform.position;
        }
    }
}
