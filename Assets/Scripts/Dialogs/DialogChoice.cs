using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DialogChoice {
    string _name;
    public string name {
        get { return _name; }
    }
    string _text;
    public string text {
        get { return _text; }
    }
    string _target;
    public string target {
        get { return _target; }
    }
    public bool HasTarget {
        get { return _target != null; }
    }

    DialogCondition _condition;
    public DialogCondition condition {
        get { return _condition; }
    }

    public static DialogCondition defaultCondition = () => { return true; };

    public DialogChoice(string name, string text, string target = null, DialogCondition condition = null) {
        this._name = name;
        this._text = text;
        this._target = target;
        this._condition = condition ?? defaultCondition;
    }
}