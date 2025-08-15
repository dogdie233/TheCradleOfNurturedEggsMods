using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

using Il2CppInterop.Runtime.Injection;

using UnityEngine;

using UObject = UnityEngine.Object;

namespace dogdie233.Uncensored;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal new static ManualLogSource Log;

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        
        ClassInjector.RegisterTypeInIl2Cpp<AlwaysUncensored>();
        var keepUncensoredObject = new GameObject(MyPluginInfo.PLUGIN_NAME);
        keepUncensoredObject.hideFlags |= HideFlags.HideAndDontSave;
        UObject.DontDestroyOnLoad(keepUncensoredObject);
        keepUncensoredObject.AddComponent<AlwaysUncensored>();
        
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}

public class AlwaysUncensored : MonoBehaviour
{
    private void OnEnable()
    {
        Plugin.Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} is being enabled!");
        MosaicControl.mosaic = false;
    }

    private void Update()
    {
        MosaicControl.mosaic = false;
    }

    private void OnDisable()
    {
        Plugin.Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} is being disabled unexcepted!");
    }

    private void OnDestroy()
    {
        Plugin.Log.LogError($"{MyPluginInfo.PLUGIN_NAME} is being destroyed unexcepted!");
    }
}
