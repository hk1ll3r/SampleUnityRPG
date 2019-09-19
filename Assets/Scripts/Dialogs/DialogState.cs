using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DialogState : DialogBase {
    
    public static readonly new string LogTag = typeof(DialogState).Name;    

    public delegate void ChoiceDelegate(DialogChoice c);

    public override void defaultOnSkipped(SkipSource source) {
        logger.LogFormat(LogType.Log, "{0} ({1}) onSkipped src {2}", LogTag, path, source);
        base.defaultOnSkipped(source);
        _active = false;
        if (source != SkipSource.Parent) _parent.OnChildSkipped(source);
    }

    public override void defaultOnStarted(string path = "") {
        logger.LogFormat(LogType.Log, "{0} ({1}) onStarted {2}", LogTag, this.path, path);
        if (!string.IsNullOrEmpty(path)) {
            logger.LogFormat(LogType.Warning, "{0} ({1}) on Started has non-empty path {2}", LogTag, this.path, path);
        }
        base.defaultOnStarted();
        _active = true;
        DialogManager.gInstance.SetDialogState(this);
    }

    public virtual void defaultOnChoice(DialogChoice c) {
        logger.LogFormat(LogType.Log, "{0} ({1}) got choice {2} target {3}", LogTag, path, c.name, c.target);
        if (!c.HasTarget) {
            this.OnSkipped(SkipSource.User);
        } else {
            parent.TransitionToState(c.target);
        }
    }

    public string text;             // text to show
    public List<DialogChoice> choices;
    public bool HasChoices {
        get { return choices != null && choices.Count > 0; }
    }
    
    private bool _active;
    public override bool IsActive {
        get { return _active; }
    }

    public ChoiceDelegate OnChoice = null;

    public DialogState(string name,
                        string text,
                        Dictionary<string, UIProp<object>> uiProps = null,
                        Dictionary<string, float> variables = null,
                        List<DialogChoice> choices = null,
                        DialogSet parent = null,
                        ChoiceDelegate onChoice = null,
                        EventDelegate handleEvent = null,
                        SkipDelegate onSkipped = null,
                        StartDelegate onStarted = null) : base(name, parent, variables, handleEvent, onSkipped, onStarted) {
        this.text = text;
        this.uiProps = uiProps ?? new Dictionary<string, UIProp<object>>();
        this.choices = choices ?? new List<DialogChoice>();

        this.OnChoice = onChoice ?? defaultOnChoice;
        this.HandleEvent = handleEvent ?? defaultHandleEvent;
        this.OnStarted = onStarted ?? defaultOnStarted;
        this.OnSkipped = onSkipped ?? defaultOnSkipped;

        _active = false;
    }

}
