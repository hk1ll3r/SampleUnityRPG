using UnityEngine;
public class Init : MonoBehaviour {
    void Awake() {
        PersistentVariable.Clear();

        GameObject.Find("DragonCamp/MicroDragonBoss").GetComponent<DragonController>().OnDieDelegate += () => {
            Events.gInstance.RaiseEvent("dragon.dead");
            PersistentVariable.Set("dragon.dead", 1f);
        };

        // Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {

    }
}