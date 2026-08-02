using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SceneAuditor : EditorWindow
{
    [MenuItem("Tools/Audit Scene Setup")]
    public static void Run()
    {
        Debug.Log("[SceneAudit] ===== FULL SCENE AUDIT =====");

        int issues = 0;

        // === SINGLETONS ===
        issues += CheckSingleton<GameManager>("GameManager");
        issues += CheckSingleton<WorldManager>("WorldManager");
        issues += CheckSingleton<InputManager>("InputManager");
        issues += CheckSingleton<Bootstrap>("Bootstrap");
        issues += CheckSingleton<SettingsManager>("SettingsManager");
        issues += CheckSingleton<TimeManager>("TimeManager");
        issues += CheckSingleton<AudioManager>("AudioManager");
        issues += CheckSingleton<AmbientManager>("AmbientManager");
        issues += CheckSingleton<AtmosphereController>("AtmosphereController");
        issues += CheckSingleton<CelestialCycle>("CelestialCycle");
        issues += CheckSingleton<CloudLayer>("CloudLayer");
        issues += CheckSingleton<AchievementManager>("AchievementManager");
        issues += CheckSingleton<LocalizationManager>("LocalizationManager");
        issues += CheckSingleton<ItemSpawner>("ItemSpawner");
        issues += CheckSingleton<BlockBreakingSystem>("BlockBreakingSystem");
        issues += CheckSingleton<BlockParticleSystem>("BlockParticleSystem");
        issues += CheckSingleton<BlockIconGenerator>("BlockIconGenerator");
        issues += CheckSingleton<GroundItemManager>("GroundItemManager");
        issues += CheckSingleton<WorldItemSpawner>("WorldItemSpawner");
        issues += CheckSingleton<WaterMeshBuilder>("WaterMeshBuilder");
        issues += CheckSingleton<FadeController>("FadeController");
        issues += CheckSingleton<UnderwaterVisuals>("UnderwaterVisuals");
        issues += CheckSingleton<UnderwaterAudioFilter>("UnderwaterAudioFilter");
        issues += CheckSingleton<InWorldCraftingManager>("InWorldCraftingManager");
        issues += CheckSingleton<KnappingSession>("KnappingSession");

        // === PLAYER ===
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[SceneAudit] MISSING: No GameObject with tag 'Player'");
            issues++;
        }
        else
        {
            Debug.Log($"[SceneAudit] OK: Player found: {player.name}");
            issues += CheckComponentOn<PlayerController>(player, "PlayerController");
            issues += CheckComponentOn<PlayerMovement>(player, "PlayerMovement");
            issues += CheckComponentOn<PlayerWater>(player, "PlayerWater");
            issues += CheckComponentOn<PlayerCamera>(player, "PlayerCamera");
            issues += CheckComponentOn<PlayerAudio>(player, "PlayerAudio");
            issues += CheckComponentOn<PlayerVoice>(player, "PlayerVoice");
            issues += CheckComponentOn<VoxelInteraction>(player, "VoxelInteraction");
            issues += CheckComponentOn<Inventory>(player, "Inventory");
            issues += CheckComponentOn<HeldItemDisplay>(player, "HeldItemDisplay");
            issues += CheckComponentOn<PlayerInteractionController>(player, "PlayerInteractionController");
            issues += CheckComponentOn<InWorldCraftingInteractionHandler>(player, "InWorldCraftingInteractionHandler");
            issues += CheckComponentOn<KnappingInteractionHandler>(player, "KnappingInteractionHandler");

            // Check VoxelInteraction refs
            VoxelInteraction vi = player.GetComponent<VoxelInteraction>();
            if (vi != null)
            {
                if (vi.playerCamera == null)
                {
                    Debug.LogError("[SceneAudit] MISSING REF: VoxelInteraction.playerCamera is null");
                    issues++;
                }
                else
                    Debug.Log($"[SceneAudit]   VoxelInteraction.playerCamera: {vi.playerCamera.name}");

                if (vi.playerController == null)
                {
                    Debug.LogError("[SceneAudit] MISSING REF: VoxelInteraction.playerController is null");
                    issues++;
                }
                else
                    Debug.Log($"[SceneAudit]   VoxelInteraction.playerController: OK");
            }

            // Check PlayerInteractionController refs
            PlayerInteractionController pic = player.GetComponent<PlayerInteractionController>();
            if (pic != null)
            {
                if (pic.cameraTransform == null)
                {
                    Debug.LogError("[SceneAudit] MISSING REF: PlayerInteractionController.cameraTransform is null");
                    issues++;
                }
                if (pic.voxelInteraction == null)
                {
                    Debug.LogError("[SceneAudit] MISSING REF: PlayerInteractionController.voxelInteraction is null");
                    issues++;
                }
                if (pic.inWorldCraftingHandler == null)
                {
                    Debug.LogError("[SceneAudit] MISSING REF: PlayerInteractionController.inWorldCraftingHandler is null");
                    issues++;
                }
                if (pic.knappingHandler == null)
                {
                    Debug.LogError("[SceneAudit] MISSING REF: PlayerInteractionController.knappingHandler is null");
                    issues++;
                }
            }

            // Check Inventory refs
            Inventory inv = player.GetComponent<Inventory>();
            if (inv != null)
            {
                if (inv.itemDatabase == null)
                {
                    Debug.LogError("[SceneAudit] MISSING REF: Inventory.itemDatabase is null");
                    issues++;
                }
                else
                    Debug.Log($"[SceneAudit]   Inventory.itemDatabase: OK");
            }

            // Check HeldItemDisplay refs
            HeldItemDisplay hid = player.GetComponent<HeldItemDisplay>();
            if (hid != null)
            {
                if (hid.handAnchor == null)
                {
                    Debug.LogError("[SceneAudit] MISSING REF: HeldItemDisplay.handAnchor is null");
                    issues++;
                }
                else
                    Debug.Log($"[SceneAudit]   HeldItemDisplay.handAnchor: {hid.handAnchor.name}");
            }

            // Check PlayerCamera refs
            PlayerCamera pc = player.GetComponent<PlayerCamera>();
            if (pc != null)
            {
                if (pc.cameraTransform == null)
                {
                    Debug.LogError("[SceneAudit] MISSING REF: PlayerCamera.cameraTransform is null");
                    issues++;
                }
                if (pc.playerCamera == null)
                {
                    Debug.LogError("[SceneAudit] MISSING REF: PlayerCamera.playerCamera (Camera component) is null");
                    issues++;
                }
            }

            // Check camera exists
            Camera mainCam = player.GetComponentInChildren<Camera>();
            if (mainCam == null)
            {
                Debug.LogError("[SceneAudit] MISSING: No Camera found under Player");
                issues++;
            }
            else
                Debug.Log($"[SceneAudit]   Player camera: {mainCam.gameObject.name}");
        }

        // === GAMEMANAGER REFS ===
        GameManager gm = Object.FindObjectOfType<GameManager>();
        if (gm != null)
        {
            if (gm.worldManager == null)
            {
                Debug.LogError("[SceneAudit] MISSING REF: GameManager.worldManager is null");
                issues++;
            }
            if (gm.player == null)
            {
                Debug.LogError("[SceneAudit] MISSING REF: GameManager.player is null");
                issues++;
            }
            if (gm.pauseMenuUI == null)
                Debug.LogWarning("[SceneAudit] WARNING: GameManager.pauseMenuUI is null");
            if (gm.startupCamera == null)
                Debug.LogWarning("[SceneAudit] WARNING: GameManager.startupCamera is null");
        }

        // === WORLDMANAGER REFS ===
        WorldManager wm = Object.FindObjectOfType<WorldManager>();
        if (wm != null)
        {
            if (wm.chunkPrefab == null)
            {
                Debug.LogError("[SceneAudit] MISSING REF: WorldManager.chunkPrefab is null");
                issues++;
            }
            if (wm.blockDatabase == null)
            {
                Debug.LogError("[SceneAudit] MISSING REF: WorldManager.blockDatabase is null");
                issues++;
            }
            else
                Debug.Log($"[SceneAudit]   WorldManager.blockDatabase: OK ({wm.blockDatabase.blocks.Count} blocks)");
        }

        // === BLOCKBREAKINGSYSTEM REFS ===
        BlockBreakingSystem bbs = Object.FindObjectOfType<BlockBreakingSystem>();
        if (bbs != null)
        {
            if (bbs.breakOverlayPrefab == null)
            {
                Debug.LogError("[SceneAudit] MISSING REF: BlockBreakingSystem.breakOverlayPrefab is null");
                issues++;
            }
            if (bbs.crackTextures == null || bbs.crackTextures.Length == 0)
            {
                Debug.LogError("[SceneAudit] MISSING REF: BlockBreakingSystem.crackTextures is empty");
                issues++;
            }
        }

        // === BLOCKPARTICLESYSTEM REFS ===
        BlockParticleSystem bps = Object.FindObjectOfType<BlockParticleSystem>();
        if (bps != null)
        {
            if (bps.particlePrefab == null)
            {
                Debug.LogError("[SceneAudit] MISSING REF: BlockParticleSystem.particlePrefab is null");
                issues++;
            }
        }

        // === ITEMSPAWNER REFS ===
        ItemSpawner isp = Object.FindObjectOfType<ItemSpawner>();
        if (isp != null)
        {
            if (isp.droppedItemPrefab == null)
            {
                Debug.LogError("[SceneAudit] MISSING REF: ItemSpawner.droppedItemPrefab is null");
                issues++;
            }
        }

        // === GROUNDITEMMANAGER REFS ===
        GroundItemManager gim = Object.FindObjectOfType<GroundItemManager>();
        if (gim != null)
        {
            var field = typeof(GroundItemManager).GetField("itemDatabase",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var db = field.GetValue(gim) as ItemDatabase;
                if (db == null)
                {
                    Debug.LogError("[SceneAudit] MISSING REF: GroundItemManager.itemDatabase is null");
                    issues++;
                }
                else
                    Debug.Log("[SceneAudit]   GroundItemManager.itemDatabase: OK");
            }
        }

        // === INWORLDCRAFTINGMANAGER REFS ===
        InWorldCraftingManager iwcm = Object.FindObjectOfType<InWorldCraftingManager>();
        if (iwcm != null)
        {
            if (iwcm.recipes == null || iwcm.recipes.Count == 0)
            {
                Debug.LogWarning("[SceneAudit] WARNING: InWorldCraftingManager has no recipes");
            }
            else
            {
                Debug.Log($"[SceneAudit]   InWorldCraftingManager recipes: {iwcm.recipes.Count}");
                for (int i = 0; i < iwcm.recipes.Count; i++)
                {
                    var r = iwcm.recipes[i];
                    if (r == null)
                    {
                        Debug.LogError($"[SceneAudit] MISSING REF: InWorldCraftingManager.recipes[{i}] is null");
                        issues++;
                    }
                    else
                        Debug.Log($"[SceneAudit]     Recipe {i}: {r.recipeName} -> {r.resultItemId}");
                }
            }
        }

        // === KNAPPING REFS ===
        KnappingSession ks = Object.FindObjectOfType<KnappingSession>();
        if (ks != null)
        {
            if (ks.knappingCamera == null)
                Debug.LogWarning("[SceneAudit] WARNING: KnappingSession.knappingCamera is null");
            if (ks.stonePivot == null)
                Debug.LogWarning("[SceneAudit] WARNING: KnappingSession.stonePivot is null");
            if (ks.rawStoneTemplate == null)
            {
                Debug.LogError("[SceneAudit] MISSING REF: KnappingSession.rawStoneTemplate is null");
                issues++;
            }
            if (ks.goalBladeTemplate == null)
            {
                Debug.LogError("[SceneAudit] MISSING REF: KnappingSession.goalBladeTemplate is null");
                issues++;
            }
            if (ks.ghostMaterial == null)
                Debug.LogWarning("[SceneAudit] WARNING: KnappingSession.ghostMaterial is null");
        }

        // === ATMOSPHERE REFS ===
        AtmosphereController ac = Object.FindObjectOfType<AtmosphereController>();
        if (ac != null)
        {
            if (ac.directionalLight == null)
            {
                Debug.LogError("[SceneAudit] MISSING REF: AtmosphereController.directionalLight is null");
                issues++;
            }
            if (ac.skyboxMaterial == null)
                Debug.LogWarning("[SceneAudit] WARNING: AtmosphereController.skyboxMaterial is null");
        }

        // === CELESTIAL REFS ===
        CelestialCycle cc = Object.FindObjectOfType<CelestialCycle>();
        if (cc != null)
        {
            if (cc.sun == null)
                Debug.LogWarning("[SceneAudit] WARNING: CelestialCycle.sun is null");
            if (cc.moon == null)
                Debug.LogWarning("[SceneAudit] WARNING: CelestialCycle.moon is null");
            if (cc.directionalLight == null)
                Debug.LogWarning("[SceneAudit] WARNING: CelestialCycle.directionalLight is null");
            if (cc.followTarget == null)
                Debug.LogWarning("[SceneAudit] WARNING: CelestialCycle.followTarget is null");
        }

        // === CLOUDLAYER REFS ===
        CloudLayer cl = Object.FindObjectOfType<CloudLayer>();
        if (cl != null)
        {
            if (cl.cloudChunkPrefab == null)
            {
                Debug.LogError("[SceneAudit] MISSING REF: CloudLayer.cloudChunkPrefab is null");
                issues++;
            }
            if (cl.followTarget == null)
                Debug.LogWarning("[SceneAudit] WARNING: CloudLayer.followTarget is null");
        }

        // === WATERMESHBUILDER REFS ===
        WaterMeshBuilder wmb = Object.FindObjectOfType<WaterMeshBuilder>();
        if (wmb != null)
        {
            if (wmb.waterMaterial == null)
            {
                Debug.LogError("[SceneAudit] MISSING REF: WaterMeshBuilder.waterMaterial is null");
                issues++;
            }
        }

        // === UI CHECKS ===
        issues += CheckSingleton<LoadingScreenUI>("LoadingScreenUI");
        issues += CheckSingleton<CrosshairUI>("CrosshairUI");
        issues += CheckSingleton<HotbarUI>("HotbarUI (on Canvas)");

        HotbarUI hotbar = Object.FindObjectOfType<HotbarUI>();
        if (hotbar != null)
        {
            if (hotbar.slotPrefab == null)
            {
                Debug.LogError("[SceneAudit] MISSING REF: HotbarUI.slotPrefab is null");
                issues++;
            }
            if (hotbar.slotsParent == null)
            {
                Debug.LogError("[SceneAudit] MISSING REF: HotbarUI.slotsParent is null");
                issues++;
            }
        }

        // === BOOTSTRAP REFS ===
        Bootstrap bs = Object.FindObjectOfType<Bootstrap>();
        if (bs != null)
        {
            var field = typeof(Bootstrap).GetField("systemObjects",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var arr = field.GetValue(bs) as MonoBehaviour[];
                if (arr == null || arr.Length == 0)
                {
                    Debug.LogWarning("[SceneAudit] WARNING: Bootstrap.systemObjects is empty");
                }
                else
                {
                    Debug.Log($"[SceneAudit]   Bootstrap systems: {arr.Length}");
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (arr[i] == null)
                        {
                            Debug.LogError($"[SceneAudit] MISSING REF: Bootstrap.systemObjects[{i}] is null");
                            issues++;
                        }
                        else
                            Debug.Log($"[SceneAudit]     [{i}] {arr[i].GetType().Name}");
                    }
                }
            }
        }

        // === LAYERS CHECK ===
        int heldItemLayer = LayerMask.NameToLayer("HeldItem");
        if (heldItemLayer < 0)
        {
            Debug.LogError("[SceneAudit] MISSING LAYER: 'HeldItem' layer does not exist");
            issues++;
        }
        else
            Debug.Log($"[SceneAudit] OK: Layer 'HeldItem' = {heldItemLayer}");

        // === DIRECTIONAL LIGHT ===
        Light[] lights = Object.FindObjectsOfType<Light>();
        bool hasDirLight = false;
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional)
            {
                hasDirLight = true;
                Debug.Log($"[SceneAudit] OK: Directional light found: {l.gameObject.name}");
                break;
            }
        }
        if (!hasDirLight)
        {
            Debug.LogError("[SceneAudit] MISSING: No Directional Light in scene");
            issues++;
        }

        // === SUMMARY ===
        if (issues == 0)
            Debug.Log("[SceneAudit] ===== ALL CLEAR — 0 issues =====");
        else
            Debug.LogError($"[SceneAudit] ===== FOUND {issues} ISSUE(S) =====");
    }

    static int CheckSingleton<T>(string label) where T : Object
    {
        T obj = Object.FindObjectOfType<T>();
        if (obj == null)
        {
            Debug.LogError($"[SceneAudit] MISSING: {label} ({typeof(T).Name}) not found in scene");
            return 1;
        }

        MonoBehaviour mb = obj as MonoBehaviour;
        if (mb != null && !mb.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[SceneAudit] INACTIVE: {label} exists but GameObject is inactive");
            return 0;
        }

        Debug.Log($"[SceneAudit] OK: {label} -> {(obj as MonoBehaviour)?.gameObject.name ?? obj.name}");
        return 0;
    }

    static int CheckComponentOn<T>(GameObject obj, string label) where T : Component
    {
        T comp = obj.GetComponent<T>();
        if (comp == null)
        {
            Debug.LogError($"[SceneAudit] MISSING COMPONENT: {label} not found on {obj.name}");
            return 1;
        }
        Debug.Log($"[SceneAudit] OK: {obj.name} has {label}");
        return 0;
    }
}