using UnityEngine;
using System.Collections.Generic;
using BSXScript;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class BSXAPI()
{
    public void Header(Table header)
    {
        string gameVersion = header.Get("gameVersion").String;
        string bsxVersion = header.Get("bsxVersion").String;

        Debug.Log($"[BSX] Running script for Beat Saber {gameVersion}, BSX {bsxVersion}");
    }
}