using System.Linq;
using BSXScript;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class GameObjectAPI
{
    public GameObject? Find(string name, bool activeOnly = true)
    {
        GameObject? obj = GameObject.Find(name);
        return !activeOnly ? Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(o => o.name == name) : obj;
    }

    public DynValue IsObjectActive(GameObject? obj)
    {
        if (obj == null) return DynValue.NewBoolean(false);
        return DynValue.NewBoolean(obj.activeSelf);
    }
}

[MoonSharpUserData]
public class TransformAPI
{
    public void SetPosition(GameObject obj, float x, float y, float z)
    {
        if (obj == null) return;
        obj.transform.position = new Vector3(x, y, z);
    }
}