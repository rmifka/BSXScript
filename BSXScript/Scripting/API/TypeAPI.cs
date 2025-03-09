using UnityEngine;

namespace BSXScript.Scripting.API;

public class TypeAPI
{
    // -- Quaternion --
    public Quaternion Quaternion(float x, float y, float z, float w) => new Quaternion(x, y, z, w);

    public Quaternion QuaternionEuler(float x, float y, float z) => UnityEngine.Quaternion.Euler(x, y, z);

    // -- Color --
    public Color Color(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);

    // -- Vector2 --
    public Vector2 Vector2(float x, float y) => new Vector2(x, y);

    // -- Vector4 --
    public Vector4 Vector4(float x, float y, float z, float w) => new Vector4(x, y, z, w);

    // -- Vector3 --
    public Vector3 Vector3(float x, float y, float z) => new Vector3(x, y, z);
    
    public Vector3 Forward() => UnityEngine.Vector3.forward;
    public Vector3 Back() => UnityEngine.Vector3.back;
    public Vector3 Up() => UnityEngine.Vector3.up;
    public Vector3 Down() => UnityEngine.Vector3.down;
    public Vector3 Left() => UnityEngine.Vector3.left;
    public Vector3 Right() => UnityEngine.Vector3.right;
    public Vector3 Zero() => UnityEngine.Vector3.zero;
    public Vector3 One() => UnityEngine.Vector3.one;
}