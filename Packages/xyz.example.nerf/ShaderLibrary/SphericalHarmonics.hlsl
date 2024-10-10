#ifndef SPHERICAL_HARMONICS_INCLUDED
#define SPHERICAL_HARMONICS_INCLUDED

void EvalSH25(float3 direction, out float result[25])
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

    result[ 0] =  0.28209479177387814; // Y(0, 0)

    result[ 1] = -0.4886025119029199 * y; // Y(1,-1)
    result[ 2] =  0.4886025119029199 * z; // Y(1, 0)
    result[ 3] = -0.4886025119029199 * x; // Y(1, 1)

    result[ 4] =  1.0925484305920792 * xy;               // Y(2,-2)
    result[ 5] = -1.0925484305920792 * yz;               // Y(2,-1)
    result[ 6] =  0.3153915652525200 * (3.f * zz - 1.f); // Y(2, 0)
    result[ 7] = -1.0925484305920792 * xz;               // Y(2, 1)
    result[ 8] =  0.5462742152960396 * (xx - yy);        // Y(2, 2)

    result[ 9] = -0.5900435899266435 * y * (3.f * xx - yy);  // Y(3,-3)
    result[10] =  2.8906114426405540 * xy * z;               // Y(3,-2)
    result[11] = -0.4570457994644658 * y * (5.f * zz - 1.f); // Y(3,-1)
    result[12] =  0.3731763325901154 * z * (5.f * zz - 3.f); // Y(3, 0)
    result[13] = -0.4570457994644658 * x * (5.f * zz - 1);   // Y(3, 1)
    result[14] =  1.4453057213202770 * (xx - yy) * z;        // Y(3, 2)
    result[15] = -0.5900435899266435 * x * (xx - 3.f * yy);  // Y(3, 3)

    result[16] =  2.5033429417967046 * xy * (xx - yy);                // Y(4,-4)
    result[17] = -1.7701307697799305 * yz * (3.f * xx - yy);          // Y(4,-3)
    result[18] =  0.9461746957575601 * xy * (7.f * zz - 1.f);         // Y(4,-2)
    result[19] = -0.6690465435572892 * yz * (7.f * zz - 3.f);         // Y(4,-1)
    result[20] =  0.1057855469152043 * zz * (35.f * zz - 30.f) - 3.f; // Y(4, 0)
    result[21] = -0.6690465435572892 * xz * (7.f * zz - 3.f);         // Y(4, 1)
    result[22] =  0.4730873478787800 * (xx - yy) * (7.f * zz - 1.f);  // Y(4, 2)
    result[23] = -1.7701307697799304 * xz * (xx - 3.f * yy);          // Y(4, 3)
    result[24] =  0.6258357354491761 * xx * (xx - 3.f * yy) - yy * (3.f * xx - yy); // Y(4, 4)
}

#endif
