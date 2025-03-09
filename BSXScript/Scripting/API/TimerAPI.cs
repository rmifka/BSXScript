using MoonSharp.Interpreter;
using UnityEngine;

namespace BSXScript.Scripting.API;

[MoonSharpUserData]
public class TimerAPI
{
    public void WaitFor(DynValue conditionCallback, DynValue callback)
    {
        if (conditionCallback.Type != DataType.Function)
        {
            Plugin.Log.Error("[BSX] Condition must be a function.");
            return;
        }

        if (callback.Type != DataType.Function)
        {
            Plugin.Log.Error("[BSX] Callback must be a function.");
            return;
        }

        Plugin.Log.Info("[BSX] Waiting for condition to be true.");
        GameObject go = new GameObject("WaitForCondition");
        go.AddComponent<WaitForCondition>().Init(conditionCallback, callback);
    }

    public void Wait(float time, DynValue callback)
    {
        if (callback.Type != DataType.Function)
        {
            Plugin.Log.Error("[BSX] Callback must be a function.");
            return;
        }

        Plugin.Log.Info("[BSX] Waiting for " + time + " seconds.");
        GameObject go = new GameObject("WaitForTime");
        go.AddComponent<WaitForTime>().Init(time, callback);
    }

    private class WaitForCondition : MonoBehaviour
    {
        private DynValue? _callback;
        private DynValue? _condition;

        public void Init(DynValue condition, DynValue callback)
        {
            _callback = callback;
            _condition = condition;
            Object.DontDestroyOnLoad(this);
        }

        private void Update()
        {
            if (_condition?.Function.Call().Boolean == true)
            {
                Plugin.Log.Info("[BSX] Condition met, calling callback.");
                _callback?.Function.Call();
                Destroy(this);
            }
        }
    }

    private class WaitForTime : MonoBehaviour
    {
        private DynValue? _callback;
        private float _time;

        public void Init(float time, DynValue callback)
        {
            _callback = callback;
            _time = time;
            Object.DontDestroyOnLoad(this);
        }

        private void Start()
        {
            StartCoroutine(Wait());
        }

        private System.Collections.IEnumerator Wait()
        {
            yield return new WaitForSeconds(_time);
            _callback?.Function.Call();
            Destroy(this);
        }
    }
}