using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AgentController : MonoBehaviour {
    public static readonly string LogTag = typeof(AgentController).Name;

    public static Logger logger = new Logger(Debug.unityLogger.logHandler);

    public AudioClip attackClip;
    public AudioClip damageClip;
    public AudioClip dieClip;

    protected AudioSource audioSource;

    public float maxHealth;
    [SerializeField]
    protected float health;
    public float Health {
        get { return health; }
    }

    public float attackRate;
    public float attackDamage;
    public abstract bool IsAttacking {
        get;
    }
    public abstract bool IsAlive {
        get;
    }

    public string team;

    // Start is called before the first frame update
    public virtual void Start() {
        if (health == 0f) health = maxHealth;
        audioSource = GetComponent<AudioSource>();
    }

    protected virtual void OnEnable() {
        Events.gInstance.gEvent += HandleEvent;
    }

    protected virtual void OnDisable() {
        Events.gInstance.gEvent -= HandleEvent;
    }

    protected virtual void HandleEvent(string eventName, params object[] eventParams) {
    }


    public virtual void OnTriggerEnter(Collider other) {
        logger.LogFormat(LogType.Log, "{0} trigger {1} entered {2}", LogTag, other.name, name);
        if (other.GetComponent<Weapon>() != null) {
            AgentController otherAgent = other.GetComponentInParent<AgentController>();
            logger.LogFormat(LogType.Log, "{0} other {1} isattacking {2}", LogTag, other.name, otherAgent.IsAttacking);
            if (otherAgent.IsAttacking) this.OnAttacked(otherAgent);
        }
    }

    public virtual void OnDie(AgentController killer) {

    }

    public virtual void OnAttacked(AgentController attacker) {
        if (attacker.team == team) return;

        health -= attacker.GetComponentInChildren<Weapon>().damage;

        if (health < 0) {
            health = 0f;
            OnDie(attacker);
            // die animation
        } else {
            // damage animation
        }
    }
}
