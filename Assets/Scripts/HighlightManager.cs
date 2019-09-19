using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

[RequireComponent(typeof(CapsuleCollider))]
public class HighlightManager : MonoBehaviour {
    public static readonly string LogTag = typeof(HighlightManager).Name;

    public static Logger logger = new Logger(Debug.unityLogger.logHandler);
    private HighlightTarget _target = null;
    public HighlightTarget target {
        get { return _target; }
    }

    CapsuleCollider capsuleCollider;
    private DialogSet rootDialogSet;
    private bool _inInteraction = false;
    public bool InInteraction {
        get { return _inInteraction; }
        set {
            _inInteraction = value;
            target.InInteraction = value;
            if (_inInteraction) {
                HUDController.gInstance.SetActionHint(false);
                SpeechSource ss = target.GetComponent<SpeechSource>();
                if (ss != null) {
                    DialogSet curSet = ss.dialogs;
                    rootDialogSet.AddState(curSet);
                    rootDialogSet.OnStarted(curSet.name);
                }
            }
        }
    }
    public static void SetHighlightTarget(HighlightTarget ht, bool on) {
        Outline o = ht.transform.GetComponentInChildren<Outline>();
        if (o == null) {
            o = ht.transform.parent.GetComponentInChildren<Outline>();
        }
        
        if (o != null) {
            o.enabled = on;
        }
    }

    private void OnTriggerEnter(Collider other) {
        HighlightTarget newTarget = other.GetComponent<HighlightTarget>();
        if (newTarget != null) {
            logger.LogFormat(LogType.Log, "{0} new target entered: {1}", LogTag, newTarget.name);
            if (_target == null) {
                _target = newTarget;
                SetHighlightTarget(_target, true);
                HUDController.gInstance.SetActionHint(true);
            } else if (target.priority < newTarget.priority) {
                SetHighlightTarget(_target, false);
                _target = newTarget;
                SetHighlightTarget(_target, true);
                HUDController.gInstance.SetActionHint(true);
            }
            capsuleCollider.radius = 5f;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (_target != null && other.gameObject == _target.gameObject) {
            logger.LogFormat(LogType.Log, "{0} target exit: {1}", LogTag, _target.name);
            SetHighlightTarget(_target, false);
            _target = null;
            capsuleCollider.radius = 3f;
            HUDController.gInstance.SetActionHint(false);
        }
    }

    void Start() {
        capsuleCollider = GetComponent<CapsuleCollider>();
        rootDialogSet = new DialogSet("root");
        rootDialogSet.OnSkipped = (s) => {
            DialogManager.gInstance.SetDialogState(null);
            List<string> keyList = new List<string>(rootDialogSet.states.Keys);
            foreach (string k in keyList) {
                rootDialogSet.RemoveState(k);
            }
            InInteraction = false;
        };
    }

    // Update is called once per frame
    void Update() {
        if (!_target.enabled) {
            SetHighlightTarget(_target, false);
            _target = null;
            capsuleCollider.radius = 3f;
            HUDController.gInstance.SetActionHint(false);
        }
    }
}
