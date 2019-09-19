using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DragonCamp : MonoBehaviour
{
    public static readonly string LogTag = typeof(DragonCamp).Name;

    public static Logger logger = new Logger(Debug.unityLogger.logHandler);

    public Material[] materials;
    public int campCapacity = 5;
    public float campRange;
    public GameObject prefab;
    public GameObject boss;
    public float coInterval = 5f;

    private bool _alive = true;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("CampCo");
    }

    // Update is called once per frame
    void Update() {
        
    }

    private IEnumerator CampCo() {
        logger.LogFormat(LogType.Log, "{0} co init...", LogTag);
        while (_alive) {
            if (boss == null) {
                _alive = false;
                foreach (Transform c in transform) {
                    AgentController ac = c.GetComponent<AgentController>();
                    ac.OnDie(ac);
                }
            } else if (transform.childCount < campCapacity) {
                CreateDragon();
            }
            yield return new WaitForSeconds(coInterval);
        }
        logger.LogFormat(LogType.Log, "{0} co end...", LogTag);
        Destroy(this);
    }

    public Vector3 GetRandomPositionInCamp() {
        Vector3 randomDirection = Random.insideUnitSphere * campRange;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, campRange, 1);
        return hit.position + 2 * Vector3.up;
    }

    private void CreateDragon() {
        Vector3 p = GetRandomPositionInCamp();
        Quaternion q = Quaternion.Euler(0f, Random.Range(0f, 180f), 0f);
        GameObject n = Instantiate(prefab, p, q, transform);
        SkinnedMeshRenderer smr = n.GetComponentInChildren<SkinnedMeshRenderer>();
        Material m = materials[Random.Range(0, materials.Length - 1)];
        smr.material = m;
    }
}
