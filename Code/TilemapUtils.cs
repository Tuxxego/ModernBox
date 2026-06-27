using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using TuxModLoader.Reflection;

public static class TilemapUtils
{
    public static void SetRendererMode(string layerClassName, string layerFieldName, TilemapRenderer.Mode mode)
    {
        var mapBoxType = Type.GetType("MapBox");
        var mapBoxInstance = ReflectionHelper.GetStaticFieldValue<object>(mapBoxType, "instance");

        var tilemapObj = ReflectionHelper.GetFieldValue<object>(mapBoxInstance, "tilemap");

        var layersArray = ReflectionHelper.GetFieldValue<object[]>(tilemapObj, "layers");

        Type layerType = Type.GetType(layerClassName);
        int layerIndex = ReflectionHelper.GetStaticFieldValue<int>(layerType, layerFieldName);

        var layer = layersArray[layerIndex];
        GameObject tilemapGO = ReflectionHelper.GetFieldValue<GameObject>(layer, "tilemap");

        var renderer = tilemapGO.GetComponent<TilemapRenderer>();
        if (renderer != null)
            renderer.mode = mode;
    }
}
