# Rendering Model

Our rendering model is mostly designed around Unity's rendering pipeline, but we hope the techniques presented are general enough to be adaptable to other real-time, rasterization-based pipelines. Since the engine is primarily designed for working with meshes, we aim to design a rendering model that is fully compatibile with these types of primitives. Like most modern rendering engines, Unity's pipeline is based around the z-buffer algorithm allowing it perform out-of-order rasterization in order to get the most out of modern GPU hardware.

In an attempt to fit seamlessly within this triangle-based, rasterization paradigm, our rendering model represents NeRF primitives using meshes. A convex mesh (most commonly a box) acts as a volume containing the NeRF scene. We render this volume using a custom ray-marching algorithm implemented via surface shaders: First, the octree structure representing the NeRF scene is loaded onto GPU memory. Then, custom vertex and fragment shaders perform ray-marching per pixel by sampling the octree structure along each ray. The resulting color, transmittance, and depth are then used to blend the results with other elements in the enviroment.

## Our Current Method

Our current method uses a mixture of both transmittance and depth in order to render NeRF primitives. During the ray-marching procedure, we estimate scene depth by computing the expected ray termination distance $E[d]$ and use this value (along with the z-buffer) to cull fragments that would be occluded by the environment. We then use the ray's total transmittance value to compose the final color with the background using alpha blending.

While this method produces good enough results most often than not, it can nevertheless be inaccurate in some edge cases. Since color and transmittance are always computed by ray-marching across the entire volume, this can result in physically incorrect colors when the volume is occupied by other primitives (such as opaque meshes). The method also inherits some of the limitations inherent to the rasterization of transparent objects, such as the need to sort primitives based on distance to the camera as well as diffuculties when rendering intersecting volumes.

## The Transparent Method

With this method, NeRF primitives are rendered during the transparency queue of the pipeline (i.e. after all opaque geometry). The z-buffer is used to determine how far a ray may travel before intersecting the environment. This step ensures we compute more accurate color and transmittance values (as compared to the previous method). We then use alpha blending with the background to compute the resulting color.

> To-do: Implement this rendering method.

## The Opaque Method

This method aims to avoid the limitations related to transparent rendering by avoiding the alpha-blending step altogether. It does this by discarding fragments based on a transmittance threshold set by the user: Rays that that have a transmittance value below the threshold are considered fully opaque, while rays that have a transmittance above that threshold are considered fully transparent. This avoids the requirement to render NeRF primitives during the transparency queue, allowing them to be rendered along with opaque geometry. (In practice, this material is rendered during Unity's "transparent cutout" queue.)

> To-do: Implement this rendering method.
