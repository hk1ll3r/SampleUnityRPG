using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

// Normal variables persist but can be wiped in a game reset.
// Persistent variables survive game resets. Badge stats for example are in this category.
public static class PersistentVariable {
    public static readonly Logger logger = new Logger(Debug.unityLogger.logHandler);
    public static readonly string LogTag = typeof(PersistentVariable).Name;

    // these will always be available and survive resets
    public static Dictionary<string, float> persistents = new Dictionary<string, float>();

    public static void RegisterPersistent(string name) {
        persistents[name] = Get(name, 0f);
    }

    public static void Initialize() {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
            var attribute = Attribute.GetCustomAttribute(type, typeof(HasPersistentVariableAttribute)) as HasPersistentVariableAttribute;
            if (attribute != null) {
                logger.LogFormat(LogType.Log, "{0} type {1} has persistent variable attribute", LogTag, type.Name);
                //System.Reflection.Ru.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                type.GetMethod("RegisterGlobals").Invoke(null, null);
            }
        }

        logger.Log(string.Join(Environment.NewLine, persistents.Keys.Select(k => k)));
    }

    public static void Set(string varName, float val) {
        if (Scope.Global.Match(varName)) {
            persistents[varName] = val;
        }
        PlayerPrefs.SetFloat(varName, val);
        PlayerPrefs.Save();
        logger.LogFormat(LogType.Log, "{0} set var '{1}': {2:00.0}", LogTag, varName, val);
    }

    public static float Get(string varName, float? defVal = null) {
        if (!defVal.HasValue && !PlayerPrefs.HasKey(varName)) {
            logger.LogFormat(LogType.Warning, "global var {0} does not exist and no default value is provided.", varName);
        }

        float retVal = PlayerPrefs.GetFloat(varName, defVal.GetValueOrDefault());
        logger.LogFormat(LogType.Log, "{0} get var '{1}': {2:00.0}", LogTag, varName, retVal);
        return retVal;
    }
    /** 
        returns the value of varName.
     */
    public static float SetIfAbsent(string varName, float val) {
        if (!PlayerPrefs.HasKey(varName)) {
            Set(varName, val);
        }
        return Get(varName);
    }

    public static void Clear() {
        PlayerPrefs.DeleteAll();
        foreach (KeyValuePair<string, float> t in persistents) {
            PlayerPrefs.SetFloat(t.Key, t.Value);
        }
        PlayerPrefs.Save();
        logger.LogFormat(LogType.Log, "{0} clear.", LogTag);
    }

    public static void NukeAll() {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        persistents.Clear();
        logger.LogFormat(LogType.Log, "{0} nuked.", LogTag);
    }

}
