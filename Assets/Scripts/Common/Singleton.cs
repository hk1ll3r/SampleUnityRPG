using System.Collections;
using System;
using UnityEngine;

public class Singleton : MonoBehaviour {
    public string tagName;

    //Awake is always called before any Start functions
    void Awake() {
        if (string.IsNullOrEmpty(tagName)) throw new ApplicationException(string.Format("Singleton tagName empty for object {0}", gameObject.name));

        GameObject curTagged = GameObject.FindWithTag(tagName);
        if (curTagged != null && curTagged != gameObject) {
            gameObject.SetActive(false);
            Destroy(gameObject);
        } else {
            gameObject.tag = tagName;
            //Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);
        }
    }
}
