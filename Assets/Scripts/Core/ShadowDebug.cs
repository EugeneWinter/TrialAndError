using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ShadowDebug : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F9)) return;

        Debug.Log("=== SHADOW DEBUG ===");

        Debug.Log($"QualitySettings.shadows: {QualitySettings.shadows}");
        Debug.Log($"QualitySettings.shadowResolution: {QualitySettings.shadowResolution}");
        Debug.Log($"QualitySettings.shadowDistance: {QualitySettings.shadowDistance}");
        Debug.Log($"QualitySettings.shadowCascades: {QualitySettings.shadowCascades}");
        Debug.Log($"QualitySettings.shadowmaskMode: {QualitySettings.shadowmaskMode}");

        var currentPipeline = GraphicsSettings.currentRenderPipeline;
        Debug.Log($"currentRenderPipeline: {(currentPipeline != null ? currentPipeline.GetType().Name : "NULL")}");

        UniversalRenderPipelineAsset urp = currentPipeline as UniversalRenderPipelineAsset;
        if (urp != null)
        {
            Debug.Log($"URP.supportsMainLightShadows: {urp.supportsMainLightShadows}");
            Debug.Log($"URP.mainLightShadowmapResolution: {urp.mainLightShadowmapResolution}");
            Debug.Log($"URP.shadowDistance: {urp.shadowDistance}");
            Debug.Log($"URP.shadowCascadeCount: {urp.shadowCascadeCount}");
            Debug.Log($"URP.supportsSoftShadows: {urp.supportsSoftShadows}");
            Debug.Log($"URP.shadowDepthBias: {urp.shadowDepthBias}");
            Debug.Log($"URP.shadowNormalBias: {urp.shadowNormalBias}");
            Debug.Log($"URP.supportsAdditionalLightShadows: {urp.supportsAdditionalLightShadows}");
        }
        else
        {
            Debug.LogError("URP Asset not found. Are you using URP?");
        }

        Light[] lights = FindObjectsOfType<Light>();
        foreach (var l in lights)
        {
            Debug.Log($"Light: {l.gameObject.name}");
            Debug.Log($"  type: {l.type}");
            Debug.Log($"  color: {l.color}");
            Debug.Log($"  intensity: {l.intensity}");
            Debug.Log($"  shadows: {l.shadows}");
            Debug.Log($"  shadowStrength: {l.shadowStrength}");
            Debug.Log($"  shadowBias: {l.shadowBias}");
            Debug.Log($"  shadowNormalBias: {l.shadowNormalBias}");
            Debug.Log($"  cullingMask: {l.cullingMask}");
            Debug.Log($"  renderMode: {l.renderMode}");
            Debug.Log($"  lightmapBakeType: {l.lightmapBakeType}");
            Debug.Log($"  direction (forward): {l.transform.forward}");
            Debug.Log($"  euler: {l.transform.eulerAngles}");
        }

        var chunkRenderers = FindObjectsOfType<ChunkRenderer>();
        Debug.Log($"ChunkRenderer count: {chunkRenderers.Length}");

        if (chunkRenderers.Length > 0)
        {
            var cr = chunkRenderers[0];
            MeshRenderer mr = cr.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Debug.Log($"First chunk shadowCastingMode: {mr.shadowCastingMode}");
                Debug.Log($"First chunk receiveShadows: {mr.receiveShadows}");
                Debug.Log($"First chunk material count: {mr.sharedMaterials.Length}");
                foreach (var m in mr.sharedMaterials)
                {
                    if (m != null)
                    {
                        Debug.Log($"  Material: {m.name}, shader: {m.shader.name}");
                        int shadowCasterPass = m.FindPass("ShadowCaster");
                        Debug.Log($"    ShadowCaster pass index: {shadowCasterPass}");
                    }
                }
            }
        }

        Debug.Log("=== END SHADOW DEBUG ===");
    }
}