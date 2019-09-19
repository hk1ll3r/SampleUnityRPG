using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class SpeechSource : MonoBehaviour {
    public static readonly string LogTag = typeof(SpeechSource).Name;

    public static Logger logger = new Logger(Debug.unityLogger.logHandler);

    public virtual DialogSet dialogs {
        get;
    }
}
