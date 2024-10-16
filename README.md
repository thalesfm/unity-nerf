# Unity NeRF

<img src="https://github.com/thalesfm/unity-nerf/assets/9205127/113fe5e7-2193-4ac5-a33f-955dbc639cc0" alt="" width="100%"
     onerror="this.onerror=null; this.src='https://thalesfm.github.io/unity-nerf/Docs/Figures/fallback.png'" />

## About

This project aims to implement real-time rendering of [NeRF](https://www.matthewtancik.com/nerf) scenes inside Unity. It is based on the method developed by [Yu et al.](https://alexyu.net/plenoctrees/) and works by caching the results of the NeRF network using a sparse voxel octree (SVO) structure with GPU acceleration.

**Note:** Due to GitHub's file size limits, some of the files required to run the example scenes included in this project are not tracked as part of this repo. These can be found in the "releases" section as downloads. **The sample scenes will not render correctly without these files!**

## Setup

- Download the Unity project, either by cloning this repo directly or by grabbing the [latest release](https://github.com/thalesfm/unity-nerf/releases/latest)
- Download the "Resources.zip" archive from the [latest release](https://github.com/thalesfm/unity-nerf/releases/latest)
- Unzip the contents of "Resources.zip" into the "Assets/Resources" folder of the Unity project
- You should now be able to open the project within the Unity Editor

## Documentation

- [Rendering Model](Docs/Rendering-Model.md)
- [The N3Tree Structure](Docs/N3Tree-Structure.md)
- [The SparseVoxelOctree Structure](Docs/SparseVoxelOctree-Structure.md)
- [Comparison of Real-Time NeRF Models](Docs/Comparison-of-Real-Time-NeRF-Models.md)
- [Comparison of Serialization Formats](Docs/Comparison-of-Serialization-Formats.md)

## Requirements

- Unity 2022.3.41f1 (LTS)
