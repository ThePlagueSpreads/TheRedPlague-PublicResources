using System.Collections.Generic;
using UnityEngine;

namespace TheRedPlague.Mono.Util;

public class CutHoleInTerrain : MonoBehaviour
{
    public float holeRadius;
    public bool debugMode = true;
    
    public bool disableRenderers;
    public bool disableColliders;
    public bool disableGrass;

    private readonly HashSet<IVoxelandChunk2> _handledChunks = new();
    private readonly HashSet<GameObject> _disabledLayers = new();

    private Vector3 GetHoleCenterPosition()
    {
        return transform.position;
    }

    private void Start()
    {
        InvokeRepeating(nameof(LazyUpdate), Random.value, 0.6f);
    }

    private void LazyUpdate()
    {
        if (!isActiveAndEnabled) return;
        
        var disableRadiusSqr = holeRadius * holeRadius;
        var levels = LargeWorld.main.streamer.streamerV2.clipmapStreamer.levels;

        foreach (var level in levels)
        {
            foreach (var cell in level.cells)
            {
                if (cell.chunk == null) continue;

                IVoxelandChunk2 voxelandChunk = cell.chunk;
                
                if (!debugMode && _handledChunks.Contains(voxelandChunk)) continue;
            
                if (disableRenderers)
                {
                    foreach (var layer in voxelandChunk.hiRenders)
                    {
                        if (layer == null) continue;
                        HandleLayerObject(layer.gameObject, disableRadiusSqr);
                    }   
                }

                if (disableColliders && voxelandChunk.collision != null)
                {
                    HandleLayerObject(voxelandChunk.collision.gameObject, disableRadiusSqr);
                }
        
                if (disableGrass)
                {
                    foreach (var layer in voxelandChunk.grassRenders)
                    {
                        if (layer == null) continue;
                        HandleLayerObject(layer.gameObject, disableRadiusSqr);
                    }   
                }
                
                if (!debugMode)
                    _handledChunks.Add(voxelandChunk);
            }
        }
    }
    
    private void HandleLayerObject(GameObject layer, float radiusSqr)
    {
        var active = Vector3.SqrMagnitude(layer.transform.position - GetHoleCenterPosition()) > radiusSqr;
        if (active && _disabledLayers.Contains(layer))
        {
            layer.SetActive(true);
            _disabledLayers.Remove(layer);
        }
        else if (!active)
        {
            layer.SetActive(false);
            _disabledLayers.Add(layer);
        }
    }

    private void OnDisable()
    {
        foreach (var layer in _disabledLayers)
        {
            if (layer == null) continue;
            layer.gameObject.SetActive(true);
        }
        _disabledLayers.Clear();
        _handledChunks.Clear();
    }
}