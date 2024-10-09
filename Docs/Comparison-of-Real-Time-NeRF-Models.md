# Comparison of Real-Time NeRF Models

Through **FastNeRF** Garbin et al.[^1] present a method for accelerating NeRF inference through caching. Caching a vanilla NeRF model would be unfeasible as it would require $n^5$ samples to capture the entire input space of the neural network. Therefore, the authors propose to split this network into two parts: one MLP which captures position-dependent features and another for reconstructing view-dependent effects. However, the physical/mathematical arguments supporting the validity of this factorization as presented in the paper seem very "hand-wavy." Furthermore, the memory requirements for storing this cache (even for a single scene) are too large for most consumer GPUs.

[^1]: Garbin et al. FastNeRF: High-Fidelity Neural Rendering at 200FPS. 2021.

In **PlenOctrees** Yu et al.[^2] present a similar strategy for achieving real-time inference by caching the result of a modified NeRF model. In order to make the input this model view-independent, the authors develop a network (*NeRF-SH*) that encodes view-dependent effects by directly outputting spherical harmonic coefficients. With this alteration it becomes possible to cache the results of the network for a particular scene using only $n^3$ samples instead of $n^5$ (however each sample requires more data). These samples are then stored sparsely as a voxel octree for efficient sampling.

[^2]: Yu et al. PlenOctrees for Real-time Rendering of Neural Radiance Fields. 2021.

With **KiloNeRF**[^3] Reiser et al. take a rather different approach by splitting the monolithic NeRF network into thousands of smaller MLPs, each of which is responsible for a small part of the scene. By doing this, the number of floating point operations required for each sample along the ray is considerably reduced. Though it seems simple in theory, in practice the implementation of this method relies heavily on batching of matrix multiplications to achieve the desired performance. For this purpose the authors rely a third-party library (i.e. MAGMA) to dynamically load matrices during inference time in an efficient manner. Unfortunately, it seems like this library is only available within Nvidia's CUDA ecosystem.

[^3]: Reiser et al. KiloNeRF: Speeding up Neural Radiance Fields with Thousands of Tiny MLPs. 2021.

**SNeRG** by Hedman et al.[^4] is yet another method based on reducing the number of input dimensions to the NeRF network and then caching the results of this modified model. This time, the authors take an approach inspired by deferred shading techniques in which diffuse and specular colors are computed separately. To do this, they train their network to predict both a diffuse color and a view-independent feature vector for every point in the scene. During rendering, a small decoder network is used to predict specular colors based on the accumulated the value of these feature vectors. Crucially, this step is only performed once per ray. Then, for efficiency purposes, the results of this network are sampled and cached in the form of a custom sparse voxel grid structure.

[^4]: Hedman et al. Baking Neural Radiance Fields for Real-Time View Synthesis. 2021.

## Memory/Storage Requirements

It is difficult to directly compare memory requirements between each method since they often present a trade-off between quality and memory usage. While some authors provide concrete numbers for the storage requirements of their models, others offer a very loose estimate. Still, this is enough to make a rough comparison between them.

Model       | Memory (MB)       | File Size (MB)
----------- | ----------------: | ----------------:
FastNeRF    |             16000 |               ---
PlenOctrees | 1900/1400/400/300 |               ---
KiloNeRF    |              <100 |               ---
SNeRG       |              6900 |     6900/86/70/30

The authors of FastNeRF provide a rough estimate that their method requires an average of 54 GB of memory per scene, yet they claim that this figure can be brought down to 16 GB by assuming that "for moderately sparse volumes, [...] 30% of space is occupied" (though they do not provide a concrete solution for the sparse storage of this data). The authors of PlenOctrees present four different versions of their method, each progressively smaller in terms of size but also less accurate in terms of reproduction quality. With SNeRG, Hedman et al. present an interesting compression scheme where they store their data as a series of images then apply either PNG, JPEG, or H.264 compression to obtain considerably smaller files.

## Benchmarks

Model       | GPU         | Baseline (FPS) | Speed (FPS) | Speedup
----------- | ----------- | -------------: | ----------: | ------:
FastNeRF    | RTX 3090    |          0.060 |      172.00 |    2800
PlenOctrees | Tesla V100  |          0.023 |      167.00 |    7260
KiloNeRF    | GTX 1080Ti  |          0.018 |       45.45 |   2548*
SNeRG       | MacBook Pro |          0.030 |       84.00 |    3600

\* Originally, when this comparison was first performed, the authors of the KiloNeRF paper claimed a 692x speedup factor over the vanilla NeRF model which made it not quite suitable for real-time applications. However, since then they have published a revised version of their paper on arXiv where they claim to achieve 2548x speedup over NeRF. Unfortunately it seems like the only explanation that the newer version of the paper provides for this massive difference is that the authors now use "a self-developed routine that fuses the entire network evaluation into a single CUDA kernel" during inference.
