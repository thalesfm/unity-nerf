#ifndef SPHERICAL_HARMONICS_INCLUDED
#define SPHERICAL_HARMONICS_INCLUDED

void EvalSH16(float3 direction, out float result[16])
{
    float x = direction.x;
    float y = direction.y;
    float z = direction.z;
    float xx = x * x;
    float yy = y * y;
    float zz = z * z;
    float xy = x * y;
    float yz = y * z;
    float xz = x * z;

    result[0] = 0.28209479177387814;
    result[1] = -0.4886025119029199 * y;
    result[2] = 0.4886025119029199 * z;
    result[3] = -0.4886025119029199 * x;
    result[4] = 1.0925484305920792 * xy;
    result[5] = -1.0925484305920792 * yz;
    result[6] = 0.31539156525252005 * (2.f * zz - xx - yy);
    result[7] = -1.0925484305920792 * xz;
    result[8] = 0.5462742152960396 * (xx - yy);
    result[9] = -0.5900435899266435 * y * (3.f * xx - yy);
    result[10] = 2.890611442640554 * xy * z;
    result[11] = -0.4570457994644658 * y * (4.f * zz - xx - yy);
    result[12] = 0.3731763325901154 * z * (2.f * zz - 3.f * xx - 3.f * yy);
    result[13] = -0.4570457994644658 * x * (4.f * zz - xx - yy);
    result[14] = 1.445305721320277 * z * (xx - yy);
    result[15] = -0.5900435899266435 * x * (xx - 3.f * yy);
}

#endif