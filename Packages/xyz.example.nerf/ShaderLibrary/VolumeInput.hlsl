#ifndef VOLUME_INPUT_INCLUDED
#define VOLUME_INPUT_INCLUDED

#define _Scale 0.5 // TODO: Hacky, remove
int _MaxSteps;
float _StepSize;

// TODO: Move somewhere else
float ComputeDepth(float3 positionOS)
{
    float4 positionHCS = TransformObjectToHClip(positionOS);
    return positionHCS.z / positionHCS.w;
}

#endif // VOLUME_INPUT_INCLUDED
