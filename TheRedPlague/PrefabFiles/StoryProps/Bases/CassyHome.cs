using System.Collections;
using Nautilus.Assets;
using Nautilus.Utility;
using TheRedPlague.Mono.VFX;
using UnityEngine;
using UWE;

namespace TheRedPlague.PrefabFiles.StoryProps.Bases;

public static class CassyHome
{
    public static PrefabInfo Info { get; } = PrefabInfo.WithTechType("CassyHome");

    public static void Register()
    {
        var prefab = new CustomPrefab(Info);
        prefab.SetGameObject(GeneratePrefab);
        prefab.Register();
    }

    private static IEnumerator GeneratePrefab(IOut<GameObject> prefab)
    {
        var obj = new GameObject(Info.ClassID);
        obj.SetActive(false);

        var request1 = PrefabDatabase.GetPrefabForFilenameAsync("Assets/Prefabs/Base/GeneratorPieces/BaseRoom.prefab");
        yield return request1;
        if (!request1.TryGetPrefab(out var baseRoom))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseRoom!");
            yield break;
        }

        var request2 =
            PrefabDatabase.GetPrefabForFilenameAsync(
                "Assets/Prefabs/Base/GeneratorPieces/BaseRoomAdjustableSupport.prefab");
        yield return request2;
        if (!request2.TryGetPrefab(out var baseRoomAdjustableSupport))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseRoomAdjustableSupport!");
            yield break;
        }

        var request3 =
            PrefabDatabase.GetPrefabForFilenameAsync(
                "Assets/Prefabs/Base/GeneratorPieces/BaseRoomExteriorBottom.prefab");
        yield return request3;
        if (!request3.TryGetPrefab(out var baseRoomExteriorBottom))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseRoomExteriorBottom!");
            yield break;
        }

        var request4 =
            PrefabDatabase.GetPrefabForFilenameAsync("Assets/Prefabs/Base/GeneratorPieces/BaseRoomCoverSide.prefab");
        yield return request4;
        if (!request4.TryGetPrefab(out var baseRoomCoverSide))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseRoomCoverSide!");
            yield break;
        }

        var request5 =
            PrefabDatabase.GetPrefabForFilenameAsync(
                "Assets/Prefabs/Base/GeneratorPieces/BaseRoomReinforcementSide.prefab");
        yield return request5;
        if (!request5.TryGetPrefab(out var baseRoomReinforcementSide))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseRoomReinforcementSide!");
            yield break;
        }

        var request6 =
            PrefabDatabase.GetPrefabForFilenameAsync("Assets/Prefabs/Base/GeneratorPieces/BaseRoomCoverBottom.prefab");
        yield return request6;
        if (!request6.TryGetPrefab(out var baseRoomCoverBottom))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseRoomCoverBottom!");
            yield break;
        }

        var request7 =
            PrefabDatabase.GetPrefabForFilenameAsync(
                "Assets/Prefabs/Base/GeneratorPieces/BaseRoomInteriorBottom.prefab");
        yield return request7;
        if (!request7.TryGetPrefab(out var baseRoomInteriorBottom))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseRoomInteriorBottom!");
            yield break;
        }

        var request8 =
            PrefabDatabase.GetPrefabForFilenameAsync("Assets/Prefabs/Base/GeneratorPieces/BaseRoomCoverTop.prefab");
        yield return request8;
        if (!request8.TryGetPrefab(out var baseRoomCoverTop))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseRoomCoverTop!");
            yield break;
        }

        var request9 =
            PrefabDatabase.GetPrefabForFilenameAsync("Assets/Prefabs/Base/GeneratorPieces/BaseRoomInteriorTop.prefab");
        yield return request9;
        if (!request9.TryGetPrefab(out var baseRoomInteriorTop))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseRoomInteriorTop!");
            yield break;
        }

        var request10 =
            PrefabDatabase.GetPrefabForFilenameAsync(
                "Assets/Prefabs/Base/GeneratorPieces/BaseRoomCorridorConnector.prefab");
        yield return request10;
        if (!request10.TryGetPrefab(out var baseRoomCorridorConnector))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseRoomCorridorConnector!");
            yield break;
        }

        var request11 =
            PrefabDatabase.GetPrefabForFilenameAsync("Assets/Prefabs/Base/GeneratorPieces/BaseRoomExteriorTop.prefab");
        yield return request11;
        if (!request11.TryGetPrefab(out var baseRoomExteriorTop))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseRoomExteriorTop!");
            yield break;
        }

        var request12 =
            PrefabDatabase.GetPrefabForFilenameAsync("Assets/Prefabs/Base/GeneratorPieces/BaseCorridorIShape.prefab");
        yield return request12;
        if (!request12.TryGetPrefab(out var baseCorridorIShape))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseCorridorIShape!");
            yield break;
        }

        var request13 =
            PrefabDatabase.GetPrefabForFilenameAsync(
                "Assets/Prefabs/Base/GeneratorPieces/BaseCorridorIShapeAdjustableSupport.prefab");
        yield return request13;
        if (!request13.TryGetPrefab(out var baseCorridorIShapeAdjustableSupport))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseCorridorIShapeAdjustableSupport!");
            yield break;
        }

        var request14 =
            PrefabDatabase.GetPrefabForFilenameAsync(
                "Assets/Prefabs/Base/GeneratorPieces/BaseCorridorIShapeCoverSide.prefab");
        yield return request14;
        if (!request14.TryGetPrefab(out var baseCorridorIShapeCoverSide))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseCorridorIShapeCoverSide!");
            yield break;
        }

        var request15 =
            PrefabDatabase.GetPrefabForFilenameAsync("Assets/Prefabs/Base/GeneratorPieces/BaseCorridorHatch.prefab");
        yield return request15;
        if (!request15.TryGetPrefab(out var baseCorridorHatch))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseCorridorHatch!");
            yield break;
        }

        var request16 =
            PrefabDatabase.GetPrefabForFilenameAsync(
                "Assets/Prefabs/Base/GeneratorPieces/BaseCorridorCoverIShapeTopIntClosed.prefab");
        yield return request16;
        if (!request16.TryGetPrefab(out var baseCorridorCoverIShapeTopIntClosed))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseCorridorCoverIShapeTopIntClosed!");
            yield break;
        }

        var request17 =
            PrefabDatabase.GetPrefabForFilenameAsync(
                "Assets/Prefabs/Base/GeneratorPieces/BaseCorridorCoverIShapeTopExtOpened.prefab");
        yield return request17;
        if (!request17.TryGetPrefab(out var baseCorridorCoverIShapeTopExtOpened))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseCorridorCoverIShapeTopExtOpened!");
            yield break;
        }

        var request18 =
            PrefabDatabase.GetPrefabForFilenameAsync(
                "Assets/Prefabs/Base/GeneratorPieces/BaseCorridorCoverIShapeBottomIntClosed.prefab");
        yield return request18;
        if (!request18.TryGetPrefab(out var baseCorridorCoverIShapeBottomIntClosed))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseCorridorCoverIShapeBottomIntClosed!");
            yield break;
        }

        var request19 =
            PrefabDatabase.GetPrefabForFilenameAsync(
                "Assets/Prefabs/Base/GeneratorPieces/BaseCorridorCoverIShapeBottomExtClosed.prefab");
        yield return request19;
        if (!request19.TryGetPrefab(out var baseCorridorCoverIShapeBottomExtClosed))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseCorridorCoverIShapeBottomExtClosed!");
            yield break;
        }

        var request20 =
            PrefabDatabase.GetPrefabForFilenameAsync("Assets/Prefabs/Base/GeneratorPieces/BaseConnectorTube.prefab");
        yield return request20;
        if (!request20.TryGetPrefab(out var baseConnectorTube))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseConnectorTube!");
            yield break;
        }

        var request21 =
            PrefabDatabase.GetPrefabForFilenameAsync(
                "Assets/Prefabs/Base/GeneratorPieces/BaseCorridorIShapeGlass.prefab");
        yield return request21;
        if (!request21.TryGetPrefab(out var baseCorridorIShapeGlass))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseCorridorIShapeGlass!");
            yield break;
        }

        var request22 =
            PrefabDatabase.GetPrefabForFilenameAsync(
                "Assets/Prefabs/Base/GeneratorPieces/BaseCorridorCoverIShapeBottomExtOpened.prefab");
        yield return request22;
        if (!request22.TryGetPrefab(out var baseCorridorCoverIShapeBottomExtOpened))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseCorridorCoverIShapeBottomExtOpened!");
            yield break;
        }

        var request23 =
            PrefabDatabase.GetPrefabForFilenameAsync("Assets/Prefabs/Base/GeneratorPieces/BaseMapRoom.prefab");
        yield return request23;
        if (!request23.TryGetPrefab(out var baseMapRoom))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseMapRoom!");
            yield break;
        }

        var request24 =
            PrefabDatabase.GetPrefabForFilenameAsync(
                "Assets/Prefabs/Base/GeneratorPieces/BaseMapRoomWindowSide.prefab");
        yield return request24;
        if (!request24.TryGetPrefab(out var baseMapRoomWindowSide))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseMapRoomWindowSide!");
            yield break;
        }

        var request25 =
            PrefabDatabase.GetPrefabForFilenameAsync(
                "Assets/Prefabs/Base/GeneratorPieces/BaseMapRoomCorridorConnector.prefab");
        yield return request25;
        if (!request25.TryGetPrefab(out var baseMapRoomCorridorConnector))
        {
            Plugin.Logger.LogError("Failed to load prefab with name BaseMapRoomCorridorConnector!");
            yield break;
        }

        var child1 = Object.Instantiate(baseRoom, obj.transform);
        child1.transform.localPosition = new Vector3(5f, 0f, 5f);
        child1.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child1.SetActive(true);
        StripComponents(child1);
        var child2 = Object.Instantiate(baseRoom, obj.transform);
        child2.transform.localPosition = new Vector3(5f, 3.5f, 5f);
        child2.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child2.SetActive(true);
        StripComponents(child2);
        var child3 = Object.Instantiate(baseRoomAdjustableSupport, obj.transform);
        child3.transform.localPosition = new Vector3(5f, 0f, 5f);
        child3.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child3.SetActive(true);
        StripComponents(child3);
        var child4 = Object.Instantiate(baseRoomExteriorBottom, obj.transform);
        child4.transform.localPosition = new Vector3(5f, 0f, 5f);
        child4.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child4.SetActive(true);
        StripComponents(child4);
        var child5 = Object.Instantiate(baseRoomCoverSide, obj.transform);
        child5.transform.localPosition = new Vector3(10f, 0f, 5f);
        child5.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child5.SetActive(true);
        StripComponents(child5);
        var child6 = Object.Instantiate(baseRoomReinforcementSide, obj.transform);
        child6.transform.localPosition = new Vector3(8.535534f, 0f, 1.464466f);
        child6.transform.localRotation = new Quaternion(0f, 0.3826835f, 0f, 0.9238795f);
        child6.SetActive(true);
        StripComponents(child6);
        var child7 = Object.Instantiate(baseRoomReinforcementSide, obj.transform);
        child7.transform.localPosition = new Vector3(5f, 0f, 0f);
        child7.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child7.SetActive(true);
        StripComponents(child7);
        var child8 = Object.Instantiate(baseRoomReinforcementSide, obj.transform);
        child8.transform.localPosition = new Vector3(1.464466f, 0f, 1.464466f);
        child8.transform.localRotation = new Quaternion(0f, 0.9238796f, 0f, 0.3826835f);
        child8.SetActive(true);
        StripComponents(child8);
        var child9 = Object.Instantiate(baseRoomReinforcementSide, obj.transform);
        child9.transform.localPosition = new Vector3(0f, 0f, 5f);
        child9.transform.localRotation = new Quaternion(0f, 1f, 0f, -4.371139E-08f);
        child9.SetActive(true);
        StripComponents(child9);
        var child10 = Object.Instantiate(baseRoomReinforcementSide, obj.transform);
        child10.transform.localPosition = new Vector3(1.464466f, 0f, 8.535534f);
        child10.transform.localRotation = new Quaternion(0f, 0.9238796f, 0f, -0.3826834f);
        child10.SetActive(true);
        StripComponents(child10);
        var child11 = Object.Instantiate(baseRoomReinforcementSide, obj.transform);
        child11.transform.localPosition = new Vector3(5f, 0f, 10f);
        child11.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, -0.7071068f);
        child11.SetActive(true);
        StripComponents(child11);
        var child12 = Object.Instantiate(baseRoomReinforcementSide, obj.transform);
        child12.transform.localPosition = new Vector3(8.535534f, 0f, 8.535534f);
        child12.transform.localRotation = new Quaternion(0f, 0.3826835f, 0f, -0.9238795f);
        child12.SetActive(true);
        StripComponents(child12);
        var child13 = Object.Instantiate(baseRoomReinforcementSide, obj.transform);
        child13.transform.localPosition = new Vector3(8.535534f, 3.5f, 1.464466f);
        child13.transform.localRotation = new Quaternion(0f, 0.3826835f, 0f, 0.9238795f);
        child13.SetActive(true);
        StripComponents(child13);
        var child14 = Object.Instantiate(baseRoomReinforcementSide, obj.transform);
        child14.transform.localPosition = new Vector3(5f, 3.5f, 0f);
        child14.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child14.SetActive(true);
        StripComponents(child14);
        var child15 = Object.Instantiate(baseRoomReinforcementSide, obj.transform);
        child15.transform.localPosition = new Vector3(1.464466f, 3.5f, 1.464466f);
        child15.transform.localRotation = new Quaternion(0f, 0.9238796f, 0f, 0.3826835f);
        child15.SetActive(true);
        StripComponents(child15);
        var child16 = Object.Instantiate(baseRoomReinforcementSide, obj.transform);
        child16.transform.localPosition = new Vector3(0f, 3.5f, 5f);
        child16.transform.localRotation = new Quaternion(0f, 1f, 0f, -4.371139E-08f);
        child16.SetActive(true);
        StripComponents(child16);
        var child17 = Object.Instantiate(baseRoomReinforcementSide, obj.transform);
        child17.transform.localPosition = new Vector3(1.464466f, 3.5f, 8.535534f);
        child17.transform.localRotation = new Quaternion(0f, 0.9238796f, 0f, -0.3826834f);
        child17.SetActive(true);
        StripComponents(child17);
        /*
        var child18 = Object.Instantiate(baseRoomReinforcementSide, obj.transform);
        child18.transform.localPosition = new Vector3(5f, 3.5f, 10f);
        child18.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, -0.7071068f);
        child18.SetActive(true);
        StripComponents(child18);
        */
        var child19 = Object.Instantiate(baseRoomReinforcementSide, obj.transform);
        child19.transform.localPosition = new Vector3(8.535534f, 3.5f, 8.535534f);
        child19.transform.localRotation = new Quaternion(0f, 0.3826835f, 0f, -0.9238795f);
        child19.SetActive(true);
        StripComponents(child19);
        var child20 = Object.Instantiate(baseRoomCoverBottom, obj.transform);
        child20.transform.localPosition = new Vector3(5f, 0f, 1.577f);
        child20.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child20.SetActive(true);
        StripComponents(child20);
        var child21 = Object.Instantiate(baseRoomCoverBottom, obj.transform);
        child21.transform.localPosition = new Vector3(1.577f, 0f, 5f);
        child21.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child21.SetActive(true);
        StripComponents(child21);
        var child22 = Object.Instantiate(baseRoomCoverBottom, obj.transform);
        child22.transform.localPosition = new Vector3(5f, 0f, 5f);
        child22.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child22.SetActive(true);
        StripComponents(child22);
        var child23 = Object.Instantiate(baseRoomCoverBottom, obj.transform);
        child23.transform.localPosition = new Vector3(8.423f, 0f, 5f);
        child23.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, -0.7071068f);
        child23.SetActive(true);
        StripComponents(child23);
        var child24 = Object.Instantiate(baseRoomCoverBottom, obj.transform);
        child24.transform.localPosition = new Vector3(5f, 0f, 8.423f);
        child24.transform.localRotation = new Quaternion(0f, 1f, 0f, -4.371139E-08f);
        child24.SetActive(true);
        StripComponents(child24);
        var child25 = Object.Instantiate(baseRoomCoverBottom, obj.transform);
        child25.transform.localPosition = new Vector3(5f, 3.5f, 1.577f);
        child25.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child25.SetActive(true);
        StripComponents(child25);
        var child26 = Object.Instantiate(baseRoomCoverBottom, obj.transform);
        child26.transform.localPosition = new Vector3(1.577f, 3.5f, 5f);
        child26.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child26.SetActive(true);
        StripComponents(child26);
        var child27 = Object.Instantiate(baseRoomCoverBottom, obj.transform);
        child27.transform.localPosition = new Vector3(5f, 3.5f, 5f);
        child27.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child27.SetActive(true);
        StripComponents(child27);
        var child28 = Object.Instantiate(baseRoomCoverBottom, obj.transform);
        child28.transform.localPosition = new Vector3(8.423f, 3.5f, 5f);
        child28.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, -0.7071068f);
        child28.SetActive(true);
        StripComponents(child28);
        var child29 = Object.Instantiate(baseRoomCoverBottom, obj.transform);
        child29.transform.localPosition = new Vector3(5f, 3.5f, 8.423f);
        child29.transform.localRotation = new Quaternion(0f, 1f, 0f, -4.371139E-08f);
        child29.SetActive(true);
        StripComponents(child29);
        var child30 = Object.Instantiate(baseRoomInteriorBottom, obj.transform);
        child30.transform.localPosition = new Vector3(5f, 0f, 5f);
        child30.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child30.SetActive(true);
        StripComponents(child30);
        var child31 = Object.Instantiate(baseRoomInteriorBottom, obj.transform);
        child31.transform.localPosition = new Vector3(5f, 3.5f, 5f);
        child31.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child31.SetActive(true);
        StripComponents(child31);
        var child32 = Object.Instantiate(baseRoomCoverTop, obj.transform);
        child32.transform.localPosition = new Vector3(5f, 0f, 1.577f);
        child32.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child32.SetActive(true);
        StripComponents(child32);
        var child33 = Object.Instantiate(baseRoomCoverTop, obj.transform);
        child33.transform.localPosition = new Vector3(1.577f, 0f, 5f);
        child33.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child33.SetActive(true);
        StripComponents(child33);
        var child34 = Object.Instantiate(baseRoomCoverTop, obj.transform);
        child34.transform.localPosition = new Vector3(5f, 0f, 5f);
        child34.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child34.SetActive(true);
        StripComponents(child34);
        var child35 = Object.Instantiate(baseRoomCoverTop, obj.transform);
        child35.transform.localPosition = new Vector3(8.423f, 0f, 5f);
        child35.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, -0.7071068f);
        child35.SetActive(true);
        StripComponents(child35);
        var child36 = Object.Instantiate(baseRoomCoverTop, obj.transform);
        child36.transform.localPosition = new Vector3(5f, 0f, 8.423f);
        child36.transform.localRotation = new Quaternion(0f, 1f, 0f, -4.371139E-08f);
        child36.SetActive(true);
        StripComponents(child36);
        var child37 = Object.Instantiate(baseRoomCoverTop, obj.transform);
        child37.transform.localPosition = new Vector3(5f, 3.5f, 1.577f);
        child37.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child37.SetActive(true);
        StripComponents(child37);
        var child38 = Object.Instantiate(baseRoomCoverTop, obj.transform);
        child38.transform.localPosition = new Vector3(1.577f, 3.5f, 5f);
        child38.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child38.SetActive(true);
        StripComponents(child38);
        var child39 = Object.Instantiate(baseRoomCoverTop, obj.transform);
        child39.transform.localPosition = new Vector3(5f, 3.5f, 5f);
        child39.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child39.SetActive(true);
        StripComponents(child39);
        var child40 = Object.Instantiate(baseRoomCoverTop, obj.transform);
        child40.transform.localPosition = new Vector3(8.423f, 3.5f, 5f);
        child40.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, -0.7071068f);
        child40.SetActive(true);
        StripComponents(child40);
        var child41 = Object.Instantiate(baseRoomCoverTop, obj.transform);
        child41.transform.localPosition = new Vector3(5f, 3.5f, 8.423f);
        child41.transform.localRotation = new Quaternion(0f, 1f, 0f, -4.371139E-08f);
        child41.SetActive(true);
        StripComponents(child41);
        var child42 = Object.Instantiate(baseRoomInteriorTop, obj.transform);
        child42.transform.localPosition = new Vector3(5f, 0f, 5f);
        child42.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child42.SetActive(true);
        StripComponents(child42);
        var child43 = Object.Instantiate(baseRoomInteriorTop, obj.transform);
        child43.transform.localPosition = new Vector3(5f, 3.5f, 5f);
        child43.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child43.SetActive(true);
        StripComponents(child43);
        var child44 = Object.Instantiate(baseRoomCorridorConnector, obj.transform);
        child44.transform.localPosition = new Vector3(10f, 3.5f, 5f);
        child44.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child44.SetActive(true);
        StripComponents(child44);
        var child45 = Object.Instantiate(baseRoomExteriorTop, obj.transform);
        child45.transform.localPosition = new Vector3(5f, 3.5f, 5f);
        child45.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child45.SetActive(true);
        StripComponents(child45);
        var child46 = Object.Instantiate(baseCorridorIShape, obj.transform);
        child46.transform.localPosition = new Vector3(15f, 3.5f, 5f);
        child46.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child46.SetActive(true);
        StripComponents(child46);
        var child47 = Object.Instantiate(baseCorridorIShapeAdjustableSupport, obj.transform);
        child47.transform.localPosition = new Vector3(15f, 3.5f, 5f);
        child47.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child47.SetActive(true);
        StripComponents(child47);
        var child48 = Object.Instantiate(baseCorridorIShapeCoverSide, obj.transform);
        child48.transform.localPosition = new Vector3(15f, 3.5f, 5f);
        child48.transform.localRotation = new Quaternion(0f, 0.7071067f, 0f, -0.7071068f);
        child48.SetActive(true);
        StripComponents(child48);
        var child49 = Object.Instantiate(baseCorridorIShapeCoverSide, obj.transform);
        child49.transform.localPosition = new Vector3(15f, 3.5f, 5f);
        child49.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child49.SetActive(true);
        StripComponents(child49);
        var child50 = Object.Instantiate(baseCorridorHatch, obj.transform);
        child50.transform.localPosition = new Vector3(15f, 3.5f, 5f);
        child50.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child50.SetActive(true);
        StripComponents(child50);
        Object.DestroyImmediate(child50.transform.Find("underWater").GetComponentInChildren<UseableDiveHatch>());
        /*
        var child51 = Object.Instantiate(baseCorridorHatch, obj.transform);
        child51.transform.localPosition = new Vector3(15f, 10.5f, 5f);
        child51.transform.localRotation = new Quaternion(0f, 1f, 0f, -4.371139E-08f);
        child51.SetActive(true);
        StripComponents(child51);
        Object.DestroyImmediate(child51.transform.Find("underWater").GetComponentInChildren<UseableDiveHatch>());
        */
        var child52 = Object.Instantiate(baseCorridorCoverIShapeTopIntClosed, obj.transform);
        child52.transform.localPosition = new Vector3(15f, 3.5f, 5f);
        child52.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child52.SetActive(true);
        StripComponents(child52);
        var child53 = Object.Instantiate(baseCorridorCoverIShapeTopExtOpened, obj.transform);
        child53.transform.localPosition = new Vector3(15f, 3.5f, 5f);
        child53.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child53.SetActive(true);
        StripComponents(child53);
        var child54 = Object.Instantiate(baseCorridorCoverIShapeBottomIntClosed, obj.transform);
        child54.transform.localPosition = new Vector3(15f, 3.5f, 5f);
        child54.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child54.SetActive(true);
        StripComponents(child54);
        var child55 = Object.Instantiate(baseCorridorCoverIShapeBottomIntClosed, obj.transform);
        child55.transform.localPosition = new Vector3(15f, 10.5f, 5f);
        child55.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child55.SetActive(true);
        StripComponents(child55);
        var child56 = Object.Instantiate(baseCorridorCoverIShapeBottomExtClosed, obj.transform);
        child56.transform.localPosition = new Vector3(15f, 3.5f, 5f);
        child56.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child56.SetActive(true);
        StripComponents(child56);
        var child57 = Object.Instantiate(baseConnectorTube, obj.transform);
        child57.transform.localPosition = new Vector3(15f, 7f, 5f);
        child57.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child57.SetActive(true);
        StripComponents(child57);
        var child58 = Object.Instantiate(baseCorridorIShapeGlass, obj.transform);
        child58.transform.localPosition = new Vector3(15f, 10.5f, 5f);
        child58.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child58.SetActive(true);
        StripComponents(child58);
        var child59 = Object.Instantiate(baseCorridorCoverIShapeBottomExtOpened, obj.transform);
        child59.transform.localPosition = new Vector3(15f, 10.5f, 5f);
        child59.transform.localRotation = new Quaternion(0f, 0f, 0f, 1f);
        child59.SetActive(true);
        StripComponents(child59);
        var child60 = Object.Instantiate(baseMapRoom, obj.transform);
        child60.transform.localPosition = new Vector3(15f, 10.5f, 15f);
        child60.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child60.SetActive(true);
        StripComponents(child60);
        var child61 = Object.Instantiate(baseMapRoomWindowSide, obj.transform);
        child61.transform.localPosition = new Vector3(15f, 10.5f, 20f);
        child61.transform.localRotation = new Quaternion(0f, -0.7071068f, 0f, 0.7071068f);
        child61.SetActive(true);
        StripComponents(child61);
        var child62 = Object.Instantiate(baseMapRoomCorridorConnector, obj.transform);
        child62.transform.localPosition = new Vector3(15f, 10.5f, 10f);
        child62.transform.localRotation = new Quaternion(0f, 0.7071068f, 0f, 0.7071068f);
        child62.SetActive(true);
        StripComponents(child62);

        PrefabUtils.AddBasicComponents(obj, Info.ClassID, Info.TechType, LargeWorldEntity.CellLevel.VeryFar);

        var infect = obj.AddComponent<InfectAnything>();
        infect.infectionAmount = 1;
        infect.infectionHeightStrength = 0;
        
        prefab.Set(obj);
    }

    private static void StripComponents(GameObject obj)
    {
        AbandonedBaseUtils.StripComponents(obj, new Color(2, 0.1f, 0.1f), false);
    }
}