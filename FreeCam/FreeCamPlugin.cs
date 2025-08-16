using System;

using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

using Il2CppInterop.Runtime.Injection;

using Unity.Cinemachine;

using UnityEngine;
using UnityEngine.InputSystem;

using Object = System.Object;
using UObject = UnityEngine.Object;

namespace dogdie233.FreeCam;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class FreeCamPlugin : BasePlugin
{
    internal new static ManualLogSource Log;

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        
        ClassInjector.RegisterTypeInIl2Cpp<CameraInputController>();
        var cameraObject = new GameObject(MyPluginInfo.PLUGIN_NAME);
        cameraObject.hideFlags |= HideFlags.HideAndDontSave;
        UObject.DontDestroyOnLoad(cameraObject);
        cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<CameraInputController>();
        
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}

public class CameraInputController : MonoBehaviour
{
    private Camera cam;
    private CinemachineCamera ccam;
    private Vector2 lastMousePosition;
    private float pressingKeyboardDuration = 0;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();

        cam.enabled = false;
    }

    private void Update()
    {
        var dir = Vector3.zero;
        if (Keyboard.current is { } keyboard)
        {
            dir.z += keyboard.wKey.isPressed ? 1 : 0;
            dir.z -= keyboard.sKey.isPressed ? 1 : 0;
            dir.x -= keyboard.aKey.isPressed ? 1 : 0;
            dir.x += keyboard.dKey.isPressed ? 1 : 0;

            if (keyboard.lKey.wasPressedThisFrame)
            {
                if (cam.enabled)
                    DeactiveCamera();
                else
                    ActiveCamera();
            }
        }
        
        if (!cam.enabled)
            return;

        if (dir.magnitude > 0)
            pressingKeyboardDuration += Time.deltaTime;
        else
            pressingKeyboardDuration = 0f;
        
        transform.Translate(dir * pressingKeyboardDuration, Space.Self);

        if (Mouse.current is { } mouse)
        {
            if (mouse.rightButton.wasPressedThisFrame)
                lastMousePosition = mouse.position.ReadValue();
            if (mouse.rightButton.isPressed)
            {
                var delta = mouse.position.ReadValue() - lastMousePosition;
                lastMousePosition = mouse.position.ReadValue();
                
                if (delta.magnitude > 0)
                {
                    transform.Rotate(Vector3.up, delta.x * 0.1f, Space.Self);
                    transform.Rotate(Vector3.left, delta.y * 0.1f, Space.Self);
                }
            }
        }
    }

    private void ActiveCamera()
    {
        var camController = UObject.FindFirstObjectByType<CameraController>();
        ccam = camController?.cinemachineVirtualCamera;;
        if (!ccam)
            ccam = FindFirstObjectByType<CinemachineCamera>();

        if (!ccam)
        {
            FreeCamPlugin.Log.LogError("No CinemachineCamera found in the scene!");
            return;
        }
        
        var mainCam = Camera.main;
        if (mainCam)
        {
            cam.clearFlags = mainCam.clearFlags;
        }
        cam.transform.position = ccam.transform.position;
        cam.transform.rotation = ccam.transform.rotation;
        cam.nearClipPlane = ccam.Lens.NearClipPlane;
        cam.farClipPlane = ccam.Lens.FarClipPlane;
        cam.fieldOfView = ccam.Lens.FieldOfView;
        
        FreeCamPlugin.Log.LogInfo($"Apply camera settings from CinemachineCamera: {ccam.name}");

        ccam.enabled = false;
        cam.enabled = true;

        lastMousePosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;
        
        FreeCamPlugin.Log.LogInfo($"Activated free camera");
    }
    
    private void DeactiveCamera()
    {
        cam.enabled = false;
        
        if (ccam)
            ccam.enabled = true;
        
        FreeCamPlugin.Log.LogInfo($"Deactivated free camera");
    }
}