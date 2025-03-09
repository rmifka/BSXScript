using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BSXScript.Scripting;

public class InputManager
{
    public static InputManager Instance { get; private set; } = null!;
    private readonly Dictionary<KeyCode, List<Action>> _keyDownCallbacks = new();

    public InputManager()
    {
        Instance = this;
        var inputManagerBehaviour = new GameObject("InputManagerBehaviour").AddComponent<InputManagerBehaviour>();
        Object.DontDestroyOnLoad(inputManagerBehaviour);
    }

    public void RegisterKeyDownCallback(KeyCode keyCode, Action callback)
    {
        if (!_keyDownCallbacks.ContainsKey(keyCode))
        {
            _keyDownCallbacks[keyCode] = new List<Action>();
        }

        _keyDownCallbacks[keyCode].Add(callback);
    }

    public void UnregisterKeyDownCallback(KeyCode keyCode, Action callback)
    {
        if (_keyDownCallbacks.TryGetValue(keyCode, out var downCallback))
        {
            downCallback.Remove(callback);
        }
    }

    private class InputManagerBehaviour : MonoBehaviour
    {
        private void Update()
        {
            foreach (var (keyCode, callbacks) in Instance._keyDownCallbacks)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    foreach (var callback in callbacks)
                    {
                        callback();
                    }
                }
            }
        }
    }
}