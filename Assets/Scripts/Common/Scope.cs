using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scope
{
    public static Scope Global = new Scope("global");

    private string _scope;
    
    public static string Globalize(string en) {
        return Global.Apply(en);
    }
    public static Scope Create(string scope) {
        return new Scope(scope);
    }

    private Scope(string scope) {
        _scope = scope;
    }

    public bool Match(string fullName) {
        return fullName.StartsWith(string.Format("{0}.", _scope));
    }

    public string Apply(string partialName) {
        return string.Format("{0}.{1}", _scope, partialName);
    }

    public string Unapply(string fullName) {
        if (!Match(fullName)) throw new ApplicationException(string.Format("Cannot unapply {0} to {1}", _scope, fullName));
        return fullName.Substring(_scope.Length + 1); // +1 for .
    }
}
