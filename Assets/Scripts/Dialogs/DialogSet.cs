using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class DialogSet : DialogBase {
    public static readonly new string LogTag = typeof(DialogSet).Name;

    public static Dictionary<string, string> GetDefaultTransitions(int size) {
        Dictionary<string, string> ret = new Dictionary<string, string>(size);
        for (int i = 2; i <= size; i++) {
            ret[(i - 1).ToString()] = i.ToString();
        }
        return ret;
    }

    public delegate void ChildSkipDelegate(SkipSource source);
    public ChildSkipDelegate OnChildSkipped = null;       // child skipped, transition locally or call parent

    private Dictionary<string, DialogBase> _states;
    public Dictionary<string, DialogBase> states {
        get { return _states; }
    }

    private Dictionary<string, string> _transitions;
    public Dictionary<string, string> transitions {
        get { return _transitions; }
    }

    public string startStateName = "";

    private string _curStateName = "";
    public string CurrentStateName {
        get { return _curStateName; }
    }
    public DialogBase CurrentDialogBase {
        get { return states[CurrentStateName]; }
    }
    public DialogState CurrentDialogState {
        get {
            if (!IsActive) throw new ApplicationException(string.Format("{0} ({1}) currentstate called on inactive state", LogTag, path));
            DialogBase b = CurrentDialogBase;
            return (b as DialogState) ?? ((DialogSet)b).CurrentDialogState;
        }
    }

    public override void defaultHandleEvent(string eventName, params object[] eventParams) {
        logger.LogFormat(LogType.Log, "{0} ({1}) got event {2}", LogTag, path, eventName);
        base.defaultHandleEvent(eventName, eventParams);
    }

    public override void defaultOnSkipped(SkipSource source) {
        logger.LogFormat(LogType.Log, "{0} ({1}) onSkipped src {2}", LogTag, path, source);
        // skip children
        if (IsActive) {
            states[CurrentStateName].OnSkipped(SkipSource.Parent);
            _curStateName = "";
        }

        // self cleanup
        base.defaultOnSkipped(source);

        // call parent's skip if they didn't initiate it to avoid infinite loop
        if (HasParent && source != SkipSource.Parent) {
            _parent.OnChildSkipped(source);
        }
    }

    public override void defaultOnStarted(string path = "") {
        logger.LogFormat(LogType.Log, "{0} ({1}) onStarted '{2}'", LogTag, this.path, path);
        if (!string.IsNullOrEmpty(path) && !CheckPathExists(path)) {
            throw new ApplicationException(string.Format("{0} ({1}) path '{2}' nay", LogTag, this.path, path));
        }
        base.defaultOnStarted(path);

        string[] names = string.IsNullOrEmpty(path) ? new string[] { startStateName } : path.Split('/');

        DialogBase db = states[names[0]];
        _curStateName = db.name;
        if (names.Length == 1) {
            db.OnStarted();
        } else if (db is DialogSet) {
            ((DialogSet)db).OnStarted(path.Substring(names[0].Length + 1));
        }
    }

    public void defaultOnChildSkipped(SkipSource source) {
        logger.LogFormat(LogType.Log, "{0} ({1}) onChildSkipped child {2} src {3}", LogTag, path, CurrentStateName, source);
        if (transitions.ContainsKey(CurrentStateName)) {
            string nextStateName = transitions[CurrentStateName];
            _curStateName = nextStateName;
            states[nextStateName].OnStarted();
        } else {
            _curStateName = "";
            OnSkipped(source);
        }
    }

    public void TransitionToState(string path) {
        logger.LogFormat(LogType.Log, "{0} ({1}) transition to '{2}'", LogTag, this.path, path);
        if (string.IsNullOrEmpty(path)) {
            return;
        }
        if (!CheckPathExists(path)) {
            throw new ApplicationException(string.Format("{0} ({1}) path '{2}' nay", LogTag, this.path, path));
        }
        if (!IsActive) {
            throw new ApplicationException(string.Format("{0} ({1}) transition called on non-active state", LogTag, path));
        }

        string[] names = path.Split('/');

        if (names[0] == "..") {
            parent.TransitionToState(string.Join("/", names.Skip(1).ToArray()));
        } else if (names[0] == CurrentStateName) {
            // same child contains both old and new active states, let them handle it
            DialogBase db = states[CurrentStateName];
            if (db is DialogSet) {
                ((DialogSet)db).TransitionToState(string.Join("/", names.Skip(1).ToArray()));
            } else {
                logger.LogFormat(LogType.Warning, "{0} ({1}) transition to active state {2}", LogTag, path, names[0]);
            }
        } else {
            // this is the state handling the switch... exciting!
            states[CurrentStateName].OnSkipped(SkipSource.Parent);
            _curStateName = names[0];
            string newPath = names.Length > 1 ? path.Substring(names[0].Length + 1) : "";
            states[CurrentStateName].OnStarted(newPath);
        }
    }

    public override bool IsActive {
        get { return CurrentStateName != ""; }
    }

    public DialogSet(string name,
                    DialogSet parent = null,
                    Dictionary<string, float> variables = null,
                    Dictionary<string, DialogBase> states = null,
                    Dictionary<string, string> transitions = null,
                    string startStateName = "1",
                    EventDelegate handleEvent = null,
                    SkipDelegate onSkipped = null,
                    StartDelegate onStarted = null,
                    ChildSkipDelegate onChildSkipped = null
                    ) : base(name, parent, variables, handleEvent, onSkipped, onStarted) {
        this._states = states ?? new Dictionary<string, DialogBase>();
        foreach (DialogBase b in _states.Values) {
            b.Parent = this;
        }
        this._transitions = transitions ?? GetDefaultTransitions(_states.Count);
        this.startStateName = startStateName;

        this.HandleEvent = handleEvent ?? defaultHandleEvent;
        this.OnStarted = onStarted ?? defaultOnStarted;
        this.OnSkipped = onSkipped ?? defaultOnSkipped;

        this.OnChildSkipped = onChildSkipped ?? defaultOnChildSkipped;
    }

    public bool CheckPathExists(string path) {
        return GetDialogBaseByPath(path) != null;
    }

    public DialogBase GetDialogBaseByPath(string path) {
        string[] names = path.Split('/');
        if (names[0] == "..") {
            if (!HasParent) {
                logger.LogFormat(LogType.Warning, "{0} ({1}) no parent for path {2}", LogTag, this.path, path);
                return null;
            } else {
                return parent.GetDialogBaseByPath(path.Substring(names[0].Length + 1));
            }
        } else if (!states.ContainsKey(names[0])) {
            logger.LogFormat(LogType.Warning, "{0} ({1}) getting non-existent path {2}", LogTag, this.path, path);
            return null;
        } else {
            DialogBase db = states[names[0]];
            if (names.Length == 1) return db;
            else if (!(db is DialogSet)) return null;
            else return ((DialogSet)db).GetDialogBaseByPath(path.Substring(names[0].Length + 1));
        }
    }

    public void AddState(DialogBase db, Tuple<string, string>[] transitions = null) {
        if (_states.ContainsKey(db.name)) {
            throw new ApplicationException(string.Format("{0} ({1}) already contains state {2}", LogTag, this.path, db.name));
        }
        _states[db.name] = db;
        db.Parent = this;

        AddTransitions(transitions);
        logger.LogFormat(LogType.Log, "{0} ({1}) added state '{2}'", LogTag, path, db.name);
    }

    public void RemoveState(string name) {
        if (name == CurrentStateName) {
            throw new ApplicationException(string.Format("{0} ({1}) cannot remove the active state {2}", LogTag, path, name));
        }
        if (startStateName == name) {
            throw new ApplicationException(string.Format("{0} ({1}) cannot remove start state {2}", LogTag, path, name));
        }

        _transitions.Remove(name);
        foreach (var item in _transitions.Where(kvp => kvp.Value == name).ToList()) {
            _transitions.Remove(item.Key);
        }

        _states.Remove(name);
        logger.LogFormat(LogType.Log, "{0} ({1}) removed state '{2}'", LogTag, path, name);
    }

    public void AddTransitions(Tuple<string, string>[] transitions) {
        if (transitions == null) return;

        foreach (var t in transitions) {
            if (!(_states.ContainsKey(t.Item1) && _states.ContainsKey(t.Item2))) {
                throw new ApplicationException(string.Format("{0} ({1}) Transition {2} -> {3} states {4}", LogTag, path, t.Item1, t.Item2, _states.Keys));
            }
        }

        foreach (var t in transitions) {
            _transitions.Add(t.Item1, t.Item2);
        }
    }
}
