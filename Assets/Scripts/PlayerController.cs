using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class PlayerController : AgentController {
    CharacterController characterController;

    public AudioClip jumpClip;
    Animator animator;
    public float movementSpeed = 5.0f;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f, rotationY = 0f;

    private HighlightManager highlightManager;

    private Transform cameraTarget;

    private bool moving = false;
    private float lastJumpTime;
    public const float jumpDelay = 3f;

    private float lastAttackTime;

    bool readyToAttack = true;
    private float prevY;
    private float vY;
    public const float maxFallVelocity = 5f;

    private bool _isDamaging = false;
    private bool _isAttacking = false;
    private bool _isImmune = false;
    public bool IsTalking {
        get { return highlightManager.InInteraction; }
    }

    public override bool IsAttacking {
        get {
            return _isAttacking;
        }
    }

    private bool _isDead = false;
    public override bool IsAlive {
        get {
            return !_isDead;
        }
    }


    public override void Start() {
        base.Start();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        HUDController.gInstance.SetHP((int)health);
        HUDController.gInstance.SetWeaponDamage(attackDamage.ToString());
        highlightManager = GetComponentInChildren<HighlightManager>();
        cameraTarget = transform.Find("cameraTarget");
    }

    void Update() {
        if (IsTalking) {
            return;
        } else if (_isDead) {
            return;
        }  else if (_isDamaging) {
            if (animator.IsInTransition(0)
                && animator.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.damage")
            ) {
                _isDamaging = false;
            }
            return;
        } else if (Input.GetButton("Action") && highlightManager.target != null && !IsTalking) {
            highlightManager.InInteraction = true;
            Cursor.lockState = CursorLockMode.None;
            animator.SetInteger("state", (int)DevilAnimationState.Idle);
            return;
        }
        // logger.LogFormat(LogType.Log, "{0} mouse ({1}, {2})", LogTag, Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Cursor.lockState = CursorLockMode.Locked;

        moveDirection = Input.GetAxis("Horizontal") * transform.TransformDirection(Vector3.right)
            + Input.GetAxis("Vertical") * transform.TransformDirection(Vector3.forward);
        moveDirection = moveDirection * movementSpeed;
        rotationY += 10f * Input.GetAxis("Rotation");
        rotationY += 5f * Input.GetAxis("Mouse X");
        rotationX += 5f * Input.GetAxis("Mouse Y");
        rotationX = Mathf.Clamp(rotationX, -30, +40);
        vY = (transform.position.y - prevY) / Time.deltaTime;
        vY = Mathf.Clamp(vY - 10f * Time.deltaTime, -maxFallVelocity, 10f); // apply gravity
        if (Input.GetAxis("Jump") > 0.5f && Time.time - lastJumpTime > jumpDelay) {
            lastJumpTime = Time.time;
            vY = 20f;
            audioSource.clip = jumpClip;
            audioSource.Play();
        }
        moveDirection.y = vY;

        bool newMoving = moveDirection.sqrMagnitude > 1f;

        //Debug.Log(string.Format("vy: {0}, prevY: {1}, y: {2}", vY, prevY, transform.position.y));
        prevY = transform.position.y;

        bool refreshAnimation = newMoving != moving;

        if (Time.time - lastAttackTime > (1 / attackRate)) {
            readyToAttack = true;
            refreshAnimation |= readyToAttack;
            // animator.SetInteger("state", (int) AnimationState.Idle);
        }

        if (Input.GetAxis("Fire1") > 0.5f && readyToAttack) {
            readyToAttack = false;
            lastAttackTime = Time.time;
            animator.SetInteger("state", (int)DevilAnimationState.Attack);
            _isAttacking = true;
            audioSource.clip = attackClip;
            audioSource.PlayDelayed(0.3f);
            GetComponentInChildren<Weapon>().GetComponent<Collider>().enabled = true;
        }

        if (_isAttacking
            && animator.IsInTransition(0)
            && animator.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("Base Layer.attack1")
            ) {

            GetComponentInChildren<Weapon>().GetComponent<Collider>().enabled = false;
            _isAttacking = false;
        }

        characterController.Move(moveDirection * Time.deltaTime);
        characterController.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
        cameraTarget.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        logger.LogFormat(LogType.Log, "{0} rotationX {1}", LogTag, rotationX);

        if (refreshAnimation) {
            moving = newMoving;
            if (readyToAttack) {
                animator.SetInteger("state", (int)(moving ? DevilAnimationState.Fly : DevilAnimationState.Idle));
            }
        }

    }

    public override void OnDie(AgentController killer) {
        animator.SetInteger("state", (int)(DevilAnimationState.Die));
        _isAttacking = false;
        highlightManager.InInteraction = false;
        GetComponentInChildren<Weapon>().GetComponent<Collider>().enabled = false;
        _isDead = true;
        audioSource.clip = dieClip;
        audioSource.Play();
    }

    private IEnumerator ImmuneCo() {
        yield return new WaitForSeconds(.5f);
        _isImmune = false;
    }

    public override void OnAttacked(AgentController attacker) {
        if (attacker.team == this.team || _isDead || _isImmune) return;

        logger.LogFormat(LogType.Log, "{0} player attacked by {1}", LogTag, attacker.name);

        health -= attacker.GetComponentInChildren<Weapon>().damage;
        _isAttacking = false;
        GetComponentInChildren<Weapon>().GetComponent<Collider>().enabled = false;
        _isImmune = true;
        StartCoroutine("ImmuneCo");

        if (health < 0) {
            health = 0f;
            OnDie(attacker);
        } else {
            // damage animation
            GetComponent<Animator>().SetInteger("state", (int)DragonAnimationState.Damage);
            _isDamaging = true;
            audioSource.clip = damageClip;
            audioSource.Play();
        }

        HUDController.gInstance.SetHP((int)health);
    }

    protected override void HandleEvent(string eventName, params object[] eventParams) {
        if (eventName == "player.fullhp") {
            health = maxHealth;
            HUDController.gInstance.SetHP((int)health);
        } else if (eventName == "player.updamage") {
            attackDamage *= 2;
            HUDController.gInstance.SetWeaponDamage("" + attackDamage);
        }
    }
}