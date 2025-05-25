using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace TheRedPlague.Mono.Systems;

public class AdditiveSceneManager : MonoBehaviour
{
    public Data data;

    public float minTimeForUnload = 5;

    private Bounds _realBounds;
    private bool _busyLoading;

    private Scene _scene;
    private GameObject _sceneRoot;

    private float _realtimeLastActive;

    private static List<Texture> _lightmapTextures = new();
    private List<Texture> _instanceLightmapTextures;

    public const string LightmapsMeshObjectName = "DATA_LightmapsMesh";

    public static Texture GetLightmapTexture(string name)
    {
        foreach (var texture in _lightmapTextures)
        {
            if (texture.name == name)
            {
                return texture;
            }
        }

        Plugin.Logger.LogWarning($"Failed to find lightmap texture by name {name}!");
        return null;
    }

    private void Start()
    {
        PrepareData();
        LazyUpdate();
        InvokeRepeating(nameof(LazyUpdate), 0.1f + Random.value * 0.2f, 1f);
    }

    private void PrepareData()
    {
        var encompassingBoxSize = Mathf.Max(data.sceneBounds.size.x, data.sceneBounds.size.z);
        _realBounds = new Bounds(data.sceneBounds.center + data.scenePosition,
            new Vector3(encompassingBoxSize, data.sceneBounds.size.y, encompassingBoxSize));
    }

    private bool InEnablementRange()
    {
        var position = MainCamera.camera.transform.position;

        if (_realBounds.Contains(position))
        {
            return true;
        }

        if (Vector3.SqrMagnitude(_realBounds.ClosestPoint(position) - position) <
            data.loadDistance * data.loadDistance)
        {
            return true;
        }

        return false;
    }

    private void LazyUpdate()
    {
        if (_busyLoading)
        {
            return;
        }

        var inEnablementRange = InEnablementRange();
        if (inEnablementRange)
        {
            _realtimeLastActive = Time.realtimeSinceStartup;
        }

        if (!_scene.isLoaded)
        {
            if (inEnablementRange)
            {
                StartCoroutine(LoadScene());
            }
        }
        else if (!inEnablementRange && Time.realtimeSinceStartup > _realtimeLastActive + minTimeForUnload)
        {
            StartCoroutine(UnloadScene());
        }
    }

    private IEnumerator LoadScene()
    {
        _busyLoading = true;
        var task = SceneManager.LoadSceneAsync(data.scenePath, LoadSceneMode.Additive);
        yield return task;

        _scene = SceneManager.GetSceneByPath(data.scenePath);
        if (!_scene.IsValid())
        {
            Plugin.Logger.LogError($"Scene {data.scenePath} loaded but is invalid!");
            yield break;
        }

        var rootObjects = _scene.GetRootGameObjects();

        if (rootObjects.Length == 0)
        {
            Plugin.Logger.LogError("Failed to find any root scene objects!");
            yield break;
        }

        if (rootObjects.Length > 1)
        {
            Plugin.Logger.LogError("Scene cannot have multiple root objects! The following objects will be unmanaged:");
            for (var i = 1; i < rootObjects.Length; i++)
            {
                Plugin.Logger.LogError($"- {i}: {rootObjects[i].name}");
            }
        }

        _sceneRoot = rootObjects[0];
        _sceneRoot.transform.position = data.scenePosition;
        _sceneRoot.transform.eulerAngles = data.sceneRotation;

        var lightmapParent = _sceneRoot.transform.Find(LightmapsMeshObjectName);
        if (lightmapParent != null)
        {
            _instanceLightmapTextures = new List<Texture>();
            var renderer = lightmapParent.GetComponent<Renderer>();
            foreach (var material in renderer.materials)
            {
                if (material == null)
                {
                    Plugin.Logger.LogWarning("Null material exists in LightmapsMesh!");
                    continue;
                }

                _instanceLightmapTextures.Add(material.mainTexture);
            }

            _lightmapTextures.AddRange(_instanceLightmapTextures);
        }
        else
        {
            Plugin.Logger.LogWarning(
                $"Child of scene root '{_sceneRoot.transform}' named '{LightmapsMeshObjectName}' not found!");
        }

        data.onSceneLoaded.Invoke(_sceneRoot);

        if (data.onSceneLoadedAsync != null)
        {
            yield return data.onSceneLoadedAsync.Invoke(_sceneRoot);
        }
        
        _busyLoading = false;
        _realtimeLastActive = Time.realtimeSinceStartup;
    }

    private IEnumerator UnloadScene()
    {
        _busyLoading = true;
        yield return SceneManager.UnloadSceneAsync(_scene);
        ReleaseLightmapTextures();
        _scene = default;
        _busyLoading = false;
    }

    private void ReleaseLightmapTextures()
    {
        if (_instanceLightmapTextures == null) return;
        foreach (var texture in _instanceLightmapTextures)
        {
            _lightmapTextures.Remove(texture);
        }

        _instanceLightmapTextures = null;
    }

    private static IEnumerator UnloadSceneOnDestroy(Scene scene)
    {
        yield return SceneManager.UnloadSceneAsync(scene);
    }

    private void OnDestroy()
    {
        if (_scene.isLoaded && !_busyLoading)
        {
            ReleaseLightmapTextures();
            UWE.CoroutineHost.StartCoroutine(UnloadSceneOnDestroy(_scene));
        }
    }

    public class Data : ScriptableObject
    {
        public string scenePath;
        public Bounds sceneBounds;
        public Vector3 scenePosition;
        public Vector3 sceneRotation;
        public float loadDistance;
        public System.Action<GameObject> onSceneLoaded;
        public System.Func<GameObject, IEnumerator> onSceneLoadedAsync;

        public Data(string scenePath, Bounds sceneBounds, Vector3 scenePosition,
            Vector3 sceneRotation, float loadDistance, System.Action<GameObject> onSceneLoaded,
            System.Func<GameObject, IEnumerator> onSceneLoadedAsync = null)
        {
            this.scenePath = scenePath;
            this.sceneBounds = sceneBounds;
            this.scenePosition = scenePosition;
            this.sceneRotation = sceneRotation;
            this.loadDistance = loadDistance;
            this.onSceneLoaded = onSceneLoaded;
            this.onSceneLoadedAsync = onSceneLoadedAsync;
        }
    }
}