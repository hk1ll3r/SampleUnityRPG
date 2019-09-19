using UnityEngine;
using UnityEngine.UI;
using System;

public class HUDController : MonoBehaviour {
    public static HUDController gInstance;

    Slider hpSlider;
    Text wdText;
    GameObject actionHint;

    void Awake() {
        // Check if instance already exists
        if (gInstance == null) gInstance = this;
        // If instance already exists and it's not this:
        if (gInstance != this)
            throw new ApplicationException("Only one HUDController object.");
        Debug.Log("HUDController happened");
    }

    // Start is called before the first frame update
    void Start() {
        hpSlider = transform.Find("HPSlider").GetComponent<Slider>();
        wdText = transform.Find("WeaponText").GetComponent<Text>();
        actionHint = transform.Find("ActionHint").gameObject;
    }

    public void SetHP(int hp) {
        hpSlider.value = hp;
    }

    public void SetWeaponDamage(string wd) {
        wdText.text = wd;
    }

    public void SetActionHint(bool visible) {
        actionHint.SetActive(visible);
    }
}
