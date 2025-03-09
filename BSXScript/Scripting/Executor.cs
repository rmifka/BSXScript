using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BSXScript.Scripting.API;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Interop.RegistrationPolicies;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BSXScript.Scripting;

public class Executor
{
    public static Executor Instance { get; private set; } = null!;
    private readonly string _path;
    private readonly Dictionary<string, List<string>> _injectClassList = new();

    public Executor(string path)
    {
        Instance = this;
        _path = path;
    }

    private readonly Dictionary<string, List<string>> _sceneScripts = new();

    public void RegisterScripts()
    {
        // Get all files in the directory
        if (!Directory.Exists(_path))
        {
            Plugin.Log.Info("Directory does not exist.");
            return;
        }

        var files = Directory.GetFiles(_path, "*.bsx", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var script = File.ReadAllText(file);
            // First line should have format: -- BSX: {SceneName}
            var lines = script.Split('\n');
            var sceneName = lines[0].Split(':')[1].Trim();
            HandleInjection(lines[1]);

            if (!_sceneScripts.ContainsKey(sceneName))
            {
                _sceneScripts[sceneName] = new List<string>();
            }

            _sceneScripts[sceneName].Add(script);
        }

        Plugin.Log.Info("Scripts registered.");
    }

    // Injection Regex
    // -- Inject: {ClassName1}(Method1,Method2), {ClassName2}(Method1,Method2), {ClassName3}(Method1,Method2)
    private static readonly Regex InjectionRegex =
        new(@"-- Inject: ((?:[a-zA-Z0-9_]+(?:\([a-zA-Z0-9_,]+\)))(?:,\s*)?)+", RegexOptions.Compiled);

    private void HandleInjection(string line)
    {
        var match = InjectionRegex.Match(line);

        if (match.Success)
        {
            string injectedClassesMethods = match.Groups[1].Value;

            var classMethodPairs = injectedClassesMethods.Split([','], StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in classMethodPairs)
            {
                var classAndMethods = pair.Trim().Split('(');
                var className = classAndMethods[0].Trim();
                var methods = classAndMethods[1].Trim(')').Split([','], StringSplitOptions.RemoveEmptyEntries);


                if (!_injectClassList.ContainsKey(className))
                {
                    _injectClassList[className] = new List<string>();
                }

                foreach (var method in methods)
                {
                    _injectClassList[className].Add(method.Trim());
                }

                Plugin.Log.Info($"Class: {className}, Methods: {string.Join(", ", methods)}");
            }
        }
        else
        {
            Plugin.Log.Warn("No valid injection pattern found in line.");
        }
    }

    public void HotReloadScripts()
    {
        _sceneScripts.Clear();
        _injectClassList.Clear();
        RegisterScripts();
        Plugin.Log.Info("Scripts hot reloaded.");
    }

    public void ExecuteScriptsInCurrentScene()
    {
        ExecuteScriptsInScene(SceneManager.GetActiveScene().name);
    }

    private void ExecuteScriptsInScene(string sceneName)
    {
        if (!_sceneScripts.TryGetValue(sceneName, out var sceneScript))
        {
            Plugin.Log.Info($"No scripts found for scene {sceneName}.");
            return;
        }

        sceneScript.ForEach(script =>
        {
            var luaScript = new Script();
            UserData.RegistrationPolicy = InteropRegistrationPolicy.Automatic;
            RegisterAPIs(luaScript);
            RegisterComplexAPIs(luaScript);

            luaScript.DoString(script);
        });
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ExecuteScriptsInScene(scene.name);
    }

    private void RegisterAPIs(Script script)
    {
        UserData.RegisterType<BSXAPI>();
        UserData.RegisterType<BeatSaberAPI>();
        UserData.RegisterType<GameObjectAPI>();
        UserData.RegisterType<TransformAPI>();
        UserData.RegisterType<TimerAPI>();

        UserData.RegisterType<UnityEngine.GameObject>();
        UserData.RegisterType<UnityEngine.Transform>();

        script.Globals["BSX"] = new BSXAPI();
        script.Globals["BeatSaber"] = new BeatSaberAPI();
        script.Globals["GameObject"] = new GameObjectAPI();
        script.Globals["Transform"] = new TransformAPI();
        script.Globals["Timer"] = new TimerAPI();
    }

    private void RegisterComplexAPIs(Script script)
    {
        Plugin.Log.Info("Registering complex APIs for injection with length " + _injectClassList.Count);
        foreach (var injectClass in _injectClassList)
        {
            var className = injectClass.Key;
            var methodNames = injectClass.Value;

            var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (var gameObject in gameObjects)
            {
                var component = gameObject.GetComponent(className);

                if (component != null)
                {
                    Plugin.Log.Info($"Attempting to inject {className}.");

                    Plugin.Log.Info($"Component name: {component.name}");
                    Plugin.Log.Info($"Component type: {component.GetType()}");

                    if (!gameObject.activeSelf)
                    {
                        Plugin.Log.Warn($"GameObject with Class {className} is not active in hierarchy.");
                    }

                    script.Globals[className] = component;
                    Plugin.Log.Info($"Injected {className}.");

                    if (methodNames.Count > 0)
                    {
                        Plugin.Log.Info($"Methods to be injected: {string.Join(", ", methodNames)}");
                    }

                    foreach (var methodName in methodNames)
                    {
                        var method = component.GetType().GetMethod(methodName,
                            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                        if (method != null)
                        {
                            script.Globals[methodName] = DynValue.NewCallback((context, args) =>
                            {
                                var methodArgs = args.GetArray().Select(arg => arg.ToObject()).ToArray();

                                Plugin.Log.Info(
                                    $"Invoking method {methodName} with {methodArgs.Length} arguments that are {string.Join(", ", args)}.");
                                for (var i = 0; i < methodArgs.Length; i++)
                                {
                                    if (method.GetParameters()[i].ParameterType == typeof(int))
                                    {
                                        methodArgs[i] = Convert.ToInt32(methodArgs[i]);
                                    }
                                }

                                for (var i = 0; i < methodArgs.Length; i++)
                                {
                                    if (method.GetParameters()[i].ParameterType == typeof(float))
                                    {
                                        methodArgs[i] = Convert.ToSingle(methodArgs[i]);
                                    }
                                }

                                try
                                {
                                    var result = method.Invoke(component, methodArgs);

                                    return result != null ? DynValue.FromObject(script, result) : DynValue.Nil;
                                }
                                catch (Exception ex)
                                {
                                    Plugin.Log.Warn($"Error invoking method {methodName}: {ex.Message}");
                                    return DynValue.Nil;
                                }
                            });
                            Plugin.Log.Info($"Injected method {methodName}.");
                        }
                        else
                        {
                            Plugin.Log.Warn($"Method '{methodName}' not found on '{component.GetType().Name}'.");
                        }
                    }
                }
            }
        }
    }
}