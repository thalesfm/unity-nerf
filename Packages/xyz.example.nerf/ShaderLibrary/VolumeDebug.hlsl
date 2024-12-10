#ifndef VOLUME_DEBUG_INCLUDED
#define VOLUME_DEBUG_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/DebuggingCommon.hlsl"

#if defined(DEBUG_DISPLAY)

bool CalculateValidationColorForDebug(out half4 debugColor)
{
    switch(_DebugMaterialValidationMode)
    {
        case DEBUGMATERIALVALIDATIONMODE_NONE:
            return false;
        case DEBUGMATERIALVALIDATIONMODE_ALBEDO:
            discard;
            return true; // Suppress compile error
        case DEBUGMATERIALVALIDATIONMODE_METALLIC:
            discard;
            return true; // Suppress compile error
        default:
            return TryGetDebugColorInvalidMode(debugColor);
    }
}

bool CalculateColorForDebugMaterial(out half4 debugColor)
{
    switch(_DebugMaterialMode)
    {
        case DEBUGMATERIALMODE_NONE:
            return false;
        case DEBUGMATERIALMODE_ALBEDO:
            discard;
            return true; // Suppress compile error
        case DEBUGMATERIALMODE_SPECULAR:
            discard;
            return true; // Suppress compile error
        case DEBUGMATERIALMODE_ALPHA:
            discard;
            return true; // Suppress compile error
        case DEBUGMATERIALMODE_SMOOTHNESS:
            discard;
            return true; // Suppress compile error
        case DEBUGMATERIALMODE_AMBIENT_OCCLUSION:
            discard;
            return true; // Suppress compile error
        case DEBUGMATERIALMODE_EMISSION:
            discard;
            return true; // Suppress compile error
        case DEBUGMATERIALMODE_NORMAL_WORLD_SPACE:
            discard;
            return true; // Suppress compile error
        case DEBUGMATERIALMODE_NORMAL_TANGENT_SPACE:
            discard;
            return true; // Suppress compile error
        case DEBUGMATERIALMODE_METALLIC:
            discard;
            return true; // Suppress compile error
        default:
            return TryGetDebugColorInvalidMode(debugColor);
    }
}

bool CanDebugOverrideOutputColor(out half4 debugColor)
{
    if (_DebugMaterialMode == DEBUGMATERIALMODE_LIGHTING_COMPLEXITY)
        discard;
    if (_DebugLightingMode == DEBUGLIGHTINGMODE_SHADOW_CASCADES)
        discard;
    if (CalculateColorForDebugSceneOverride(debugColor))
        return true;
    if (CalculateColorForDebugMaterial(debugColor))
        return true;
    if (CalculateValidationColorForDebug(debugColor))
        return true;
    
    return false;
}

#endif

#endif // VOLUME_DEBUG_INCLUDED
