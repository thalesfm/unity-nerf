# N3Tree Structure

PlenOctrees stores their data using a custom data structure called N3Tree. It is meant to be a generalization of the quadtree and octree data structures, though in practice it's only ever used as an octree. The authors provide a Python interface for this data structure as a PyTorch extension through their [svox](https://github.com/sxyu/svox) library. However, we ran into issues with CUDA drivers when trying to install this package within the Google Colab environment. As a workaround to this issue we use a stripped-down version of this library without GPU acceleration to load in and access PlenOctrees data within our Python scripts.

## Conventions

N3Trees are parameterized with two integers: `N` and `depth_limit`. The value of `N` determines the number of subdivisions (per dimension) at each level of the structure. Since this value should always be set at 2 for octrees, we will assume `N = 2` from here on out. The value of `depth_limit` determines the size of its internal graph structure. This value effectively limits the maximum resolution achievable with the data structure. Voxels are located within the unit cube, with samples being taken using floating-point coordinates $(x, y, z)$ where $x$, $y$, $z$ are all in the range of $[0, 1]$. Though the structure itself doesn't enforce a particular coordinate scheme, all scenes present in the PlenOctrees dataset use a right-handed XYZ coordinate system (with a vertical Z axis).

## Sampling

Sampling is performed in batches on the GPU using a custom CUDA kernel which always searches the octree from the root down. Sampling relies on the nearest-neighbor method such that no interpolation is performed between voxels.

## Memory layout

The structure is primarily designed to sparsely store floating-point data in the form of vectors. To this end, the structure is parameterized with an integer `data_dim` which determines how many floating-point values are stored at each node. Memory-wise, the structure mostly consists of two NumPy tensors: `self.child` and `self.data`. The first tensor -- `self.child` -- has a dimension of $(n, 2, 2, 2)$ where $n$ denotes the current number of nodes in the octree. It is used to represent the octree's internal tree structure: For each node $i \in \mathbb{N}$, $0 \leq i \lt n$, `self.child[i]` contains a $(2, 2, 2)$-tensor with the indices of each of its children. The second tensor -- `self.data` -- has a dimension of $(n, 2, 2, 2,$ `data_dim` $)$ and is used to store the data pertaining to each node. In both cases octants within a node are indexed in lexicographical order (as seen in the Python expression: `self.child[i, x, y, z]`).

Note that the structure stores data *per node*, not per voxel. This means it is capable of storing data not only in the leaves, but also in the intermediate nodes. Though the authors don't seem to take advantage of this feature in their own rendering algorithm, it could be useful for implementing level-of-detail and/or mipmapping.

Each voxel stores a `data_dim`-dimensional vector of floating-point values. Since no particular layout is enforced for these values, care must be taken to establish a consistent order when reading/writing to these vectors. Ou of the box the `svox` library natively supports formats such as RGB, spherical harmonics, and spherical gaussians. Since the PlenOctrees dataset exclusively uses the spherical harmonics, that is what we will focus on. When using this format, coefficients are grouped by color in the order: red, green, then blue. Within each color coefficients are grouped by sphercial harmonic order, then by degree. This results in the following sequence of coefficients: $f_0^0$, $f_1^{-1}$, $f_1^0$, $f_1^1$, $f_2^{-2}$, $f_2^{-1}$, $f_2^0$, $f_2^1$, $f_2^2$, ... (where $f_\ell^m$ is the coefficient corresponding to the spherical harmonic $Y_\ell^m$ of order $\ell$ and degree $m$. As with every other format, the last value in the vector represents the density $\sigma$.

## Serialization

The structure is serialized to disk using the `.npz` format, which is a custom binary serialization format provided by the NumPy package. A single `.npz` file consists of multiple `.npy` files (each representing a single NumPy array) zipped together into a single archive. Since NumPy only provides official bindings for Python, serialization and deserialization in other environments (such as .NET) can be quite cumbersome.
