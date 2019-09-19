using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class)]
public class HasPersistentVariableAttribute : Attribute {
    public static readonly Logger logger = new Logger(Debug.unityLogger.logHandler);
    public static readonly string LogTag = typeof(HasPersistentVariableAttribute).Name;

    
}
