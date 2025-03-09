using System.IO;
using BSXScript.Scripting;
using IPA;
using IPA.Config.Stores;
using IPA.Loader;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using IpaLogger = IPA.Logging.Logger;
using IpaConfig = IPA.Config.Config;

namespace BSXScript;

[Plugin(RuntimeOptions.DynamicInit), NoEnableDisable]
internal class Plugin
{
    internal static IpaLogger Log { get; private set; } = null!;
    internal static PluginConfig Config { get; private set; } = null!;

    // Methods with [Init] are called when the plugin is first loaded by IPA.
    // All the parameters are provided by IPA and are optional.
    // The constructor is called before any method with [Init]. Only use [Init] with one constructor.
    [Init]
    public Plugin(IpaLogger ipaLogger, IpaConfig ipaConfig, PluginMetadata pluginMetadata)
    {
        Log = ipaLogger;
        Config = ipaConfig.Generated<PluginConfig>();
        string scriptPath = Path.Combine(Application.dataPath, "..", "UserData", "BSXScripts");
        ipaLogger.Info($"Script path: {scriptPath}");
        if (!Directory.Exists(scriptPath))
        {
            Directory.CreateDirectory(scriptPath);
        }

        var executor = new Executor(scriptPath);
        executor.RegisterScripts();
        
        var inputManager = new InputManager();
        Log.Info($"{pluginMetadata.Name} {pluginMetadata.HVersion} initialized.");
    }

    [OnStart]
    public void OnApplicationStart()
    {
        SceneManager.sceneLoaded += Executor.Instance.OnSceneLoaded;
    }

    [OnEnable]
    public void OnEnable()
    {
        Log.Info("OnEnable");
        InputManager.Instance.RegisterKeyDownCallback(KeyCode.F1, Executor.Instance.HotReloadScripts);
        InputManager.Instance.RegisterKeyDownCallback(KeyCode.F2, Executor.Instance.ExecuteScriptsInCurrentScene);
        
    }
}