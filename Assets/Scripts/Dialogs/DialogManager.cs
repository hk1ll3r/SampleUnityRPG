using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogManager : MonoBehaviour {
    public static readonly new string LogTag = typeof(DialogManager).Name;
    public static Logger logger = new Logger(Debug.unityLogger.logHandler);

    public static DialogManager gInstance;
    public GameObject choicePrefab;

    Canvas dialogCanvas;
    GameObject dialogPanel;
    Text nameText;
    Text dialogText;
    Image avatarImage;

    private DialogState curState = null;

    private float _stateStartTime;

    //Awake is always called before any Start functions
    void Awake() {
        // Check if instance already exists
        if (gInstance == null) gInstance = this;
        // If instance already exists and it's not this:
        if (gInstance != this) throw new ApplicationException("only one DialogManager :(");

        //tv = GameObject.FindWithTag("MainCanvas").transform.Find("TV").GetComponent<TVController>();
        //frontImage = tv.transform.Find("FrontImage").gameObject.GetComponent<Image>();
        //backImage = tv.transform.Find("BackImage").gameObject.GetComponent<Image>();

        dialogCanvas = GameObject.Find("DialogCanvas").GetComponent<Canvas>();
        dialogPanel = dialogCanvas.transform.Find("DialogPanel").gameObject;
        avatarImage = dialogCanvas.transform.Find("AvatarImage").GetComponent<Image>();
        nameText = dialogCanvas.transform.Find("NameText").GetComponent<Text>();

        // Add click handler to dialogPanel
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { OnPanelClick((PointerEventData)data); });
        dialogPanel.GetComponent<EventTrigger>().triggers.Add(entry);

        dialogText = dialogPanel.transform.Find("DialogText").GetComponent<Text>();
    }

    void OnPanelClick(PointerEventData data) {
        logger.LogFormat(LogType.Log, "user skip event raised");
        if (!curState.HasChoices) {
            curState.OnSkipped(DialogBase.SkipSource.User);
        }
    }

    void OnChoiceClick(DialogChoice c) {
        curState.OnChoice(c);
    }

    public void UnloadCurrentDialogState() {
        foreach (Transform t in dialogPanel.transform) {
            if (t.name != "DialogText") {
                GameObject.Destroy(t.gameObject);
            }
        }
        curState = null;
    }

    public void SetDialogState(DialogState state) {
        if (curState != null) {
            UnloadCurrentDialogState();
        }

        curState = state;

        if (state == null) {
            dialogCanvas.enabled = false;
        } else {
            dialogCanvas.enabled = true;

            _stateStartTime = Time.time;
            
            avatarImage.sprite = (Sprite)Resources.Load("Avatars/" + state.GetUIProp<string>("avatar"), typeof(Sprite));
            dialogText.text = state.text;
            nameText.text = state.GetUIProp<string>("name");

            if (curState.HasChoices) {
                foreach (DialogChoice c in curState.choices) {
                    GameObject choiceButton = Instantiate(choicePrefab, dialogPanel.transform);
                    choiceButton.transform.Find("Text").GetComponent<Text>().text = string.Format("{0}) {1}", c.name, c.text);
                    DialogChoice cc = c;
                    choiceButton.GetComponent<Button>().onClick.AddListener(() => {
                        OnChoiceClick(cc);
                    });
                }
            }
        }
    }
}
