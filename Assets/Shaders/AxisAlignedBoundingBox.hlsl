#ifndef AXIS_ALIGNED_BOUNDING_BOX_INCLUDED
#define AXIS_ALIGNED_BOUNDING_BOX_INCLUDED

float IntersectPointBox(float3 position)
{
    return (-1.0 < position.x && position.x < 1.0 &&
            -1.0 < position.y && position.y < 1.0 &&
            -1.0 < position.z && position.z < 1.0);
}

float IntersectRayBox(float3 rayOrigin, float3 rayDirection)
{
    float3 aux1 = (float3(-1.0, -1.0, -1.0) - rayOrigin) / rayDirection;
    float3 aux2 = (float3( 1.0,  1.0,  1.0) - rayOrigin) / rayDirection;
    float3 tmin = min(aux1, aux2);
    float3 tmax = max(aux1, aux2);
    float t1 = max(tmin.x, max(tmin.y, tmin.z));
    float t2 = min(tmax.x, max(tmax.y, tmax.z));

    if (0.0 < t1 && t1 < t2) {
        return t1;
    } else {
        return -1.0;
    }
}

#endif