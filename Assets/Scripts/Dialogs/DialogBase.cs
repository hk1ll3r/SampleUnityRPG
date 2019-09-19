using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DialogBase {
    public const string TextCodeClear = "_";
    public static readonly string LogTag = typeof(DialogBase).Name;

    public static Logger logger = new Logger(Debug.unityLogger.logHandler);

    public enum SkipSource {
        User = 1,
        Timer = 2,
        Parent = 3,
        Program = 4,
    }

    protected Dictionary<string, UIProp<object>> uiProps = new Dictionary<string, UIProp<object>>();

    public Dictionary<string, UIProp<object>> defaultUIProps = new Dictionary<string, UIProp<object>>() {
    };

    public T GetUIProp<T>(string name) {
        UIProp<object> rawProp = uiProps.ContainsKey(name) ? uiProps[name] : new UIProp<object>();
        if (rawProp.HasValue) {
            return (T) rawProp.Value;
        } else if (HasParent) {
            return parent.GetUIProp<T>(name);
        } else {
            return default(T);
        }
    }

    public delegate void EventDelegate(string eventName, params object[] eventParams);
    public delegate void SkipDelegate(SkipSource source);
    public delegate void StartDelegate(string path = "");

    public virtual void defaultHandleEvent(string eventName, params object[] eventParams) {
        logger.LogFormat(LogType.Log, "{0} ({1}) got event {2}", LogTag, path, eventName);
        if (HasParent) {
            Parent.HandleEvent(eventName, eventParams);
        }
    }

    public virtual void defaultOnSkipped(SkipSource source) {
        logger.LogFormat(LogType.Log, "{0} ({1}) onSkipped src {2}", LogTag, path, source);
    }

    public virtual void defaultOnStarted(string path = "") {
        logger.LogFormat(LogType.Log, "{0} ({1}) onStarted {2}", LogTag, this.path, path);
        if (IsActive) {
            throw new ApplicationException(string.Format("{0} ({1}) onStarted state already active."));
        }
        SetVariable("#starttime", Time.time);
    }

    private Dictionary<string, float> _variables;
    public Dictionary<string, float> variables {
        get { return _variables; }
    }

    protected string _name;
    protected DialogSet _parent;                 // dialog set this belongs to
    public DialogSet Parent {
        get { return _parent; }
        set { _parent = value; }
    }

    public EventDelegate HandleEvent = null;
    public SkipDelegate OnSkipped = null;       // exiting this state, cleanup!
    public StartDelegate OnStarted = null;      // entering this state, yey!

    public string name {
        get { return _name; }
    }

    public string path {
        get { return string.Format("{0}/{1}", HasParent ? parent.path : "", name); }
    }

    public DialogSet parent {
        get { return _parent; }
    }
    public bool HasParent {
        get { return _parent != null; }
    }

    public DialogBase(string name,
                      DialogSet parent = null,
                      Dictionary<string, float> variables = null,
                      EventDelegate handleEvent = null,
                      SkipDelegate onSkipped = null,
                      StartDelegate onStarted = null
                      ) {
        this._name = name;
        this._parent = parent;
        this._variables = variables ?? new Dictionary<string, float>();
        this.HandleEvent = handleEvent ?? defaultHandleEvent;
        this.OnSkipped = onSkipped ?? defaultOnSkipped;
        this.OnStarted = onStarted ?? defaultOnStarted;

        DeclareVariable("#starttime", 0f);
    }

    public abstract bool IsActive {    // when this state or one of its descendants is in manager
        get;
    }

    public void DeclareVariable(string varName, float initialVal = 0) {
        _variables[varName] = initialVal;
    }

    protected Dictionary<string, float> FindVariable(string varName) {
        if (variables.ContainsKey(varName)) return variables;
        else if (HasParent) return parent.FindVariable(varName);
        else return null;
    }

    public void SetVariable(string varName, float val) {
        Dictionary<string, float> vars = FindVariable(varName);
        vars[varName] = val;
    }

    public float GetVariable(string varName, float? defVal = null) {
        Dictionary<string, float> vars = FindVariable(varName);
        return vars == null ? defVal.Value : vars[varName];
    }
}
