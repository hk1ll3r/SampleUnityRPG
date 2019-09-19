using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

[RequireComponent(typeof(Collider))]
public class HighlightTarget : MonoBehaviour
{
    public int priority = 0;
    public string title;

    private bool _inInteraction;
    public bool InInteraction {
        get { return _inInteraction; }
        set { _inInteraction = value; }
    }

    void OnDisable() {
        foreach (var c in GetComponents<Collider>()) {
            c.enabled = false;
        }
    }
    void OnEisable() {
        foreach (var c in GetComponents<Collider>()) {
            c.enabled = true;
        }
    }
}
