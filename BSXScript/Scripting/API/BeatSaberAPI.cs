using System;
using System.Reflection;
using BSXScript;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class BeatSaberAPI
{
    public object? GetClassInstance(string className)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            Component? component = obj.GetComponent(className);
            if (component != null)
            {
                if (!component.gameObject.activeInHierarchy)
                {
                    Plugin.Log.Warn($"[BSX] GameObject with Class {className} is not active in hierarchy.");
                }

                // Register the type
                UserData.RegisterType(component.GetType());
                return component;
            }
        }

        Plugin.Log.Warn($"[BSX] Class '{className}' not found in scene.");
        return null;
    }

    public object? CallMethod(object? instance, string methodName, params object[] parameters)
    {
        if (instance == null)
            return null;

        MethodInfo? method = instance.GetType().GetMethod(methodName);
        if (method != null)
        {
            return method.Invoke(instance, parameters);
        }

        Plugin.Log.Warn($"[BSX] Method '{methodName}' not found on '{instance.GetType().Name}'.");
        return null;
    }
    
    public object? GetProperty(object? instance, string propertyName)
    {
        if (instance == null)
            return null;

        PropertyInfo? property = instance.GetType().GetProperty(propertyName);
        if (property != null)
        {
            return property.GetValue(instance);
        }

        Plugin.Log.Warn($"[BSX] Property '{propertyName}' not found on '{instance.GetType().Name}'.");
        return null;
    }
}