using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour {

    Button questButton;
    Text questText;

    bool _visible = false;

    void Awake() {
        questButton = GetComponent<Button>();
        questButton.onClick.AddListener(() => {
            ToggleVisible();
        });
        questText = transform.Find("QuestText").GetComponent<Text>();
        questText.gameObject.SetActive(true);
    }

    private void ToggleVisible() {
        _visible = !_visible;
        questText.gameObject.SetActive(_visible);
    }

    void Start() {
        UpdateUI();
    }
    private void UpdateUI() {
        string t = "";
        if (PersistentVariable.Get("quests.cat.state", 0f) == 0f) {
            t = "Talk to Lady Pirate in the village.";
        } else if (PersistentVariable.Get("quests.cat.state", 0f) == 1f) {
            t = "Find Kitty and take her back to Lady Pirate.";
        } else if (PersistentVariable.Get("quests.dragon.state", 0f) == 0f) {
            t = "Talk to Lady Pirate.";
        } else if (PersistentVariable.Get("quests.dragon.state", 0f) == 1f) {
            t = "Kill the Queen Micro Dragon (large orange one).";
        }

        if (t == "") {
            _visible = false;
            questText.gameObject.SetActive(_visible);
            questButton.enabled = false;
        } else {
            questText.text = t;
        }
    }

    void Update() {
        if (Input.GetKey(KeyCode.Q) && questButton.enabled) {
            ToggleVisible();
        }
    }
    void HandleEvent(string e, params object[] p) {
        if (e.StartsWith("quests")) {
            UpdateUI();
        }
    }
    void OnEnable() {
        Events.gInstance.gEvent += HandleEvent;
    }

    void OnDisable() {
        Events.gInstance.gEvent -= HandleEvent;
    }
}
