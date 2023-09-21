LayerFx is a tool for rendering objects into separate textures for working with complex graphics.<br>
It has built-in functionality for blending layers, similar to Photoshop, and rendering them on the screen or into a texture.<br>
The content of the layers or the result of their blending can also be used for any purposes through a shader.<br>

![_LayersTech](https://github.com/NullTale/LayerFx/assets/1497430/29ccec28-93db-48b5-bf5d-2bf5a7520be4)<br>
<sub>layers blending</sub>


> example from documentation can be found in the package samples

### Toolkit

LayerFx it's a part of the overall Fx toolkit divided into three parts
- [VolFx](https://github.com/NullTale/VolFx) - customizable post-processing with LayerMask filtering and texture processing
- [ScreenFx](https://github.com/NullTale/ScreenFx) - custom timeline tracks for screen space fx
- LayerFx - render feature to draw objects into buffers, can be used as photoshop layers

### Package Manager Url

Install via [Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html)
```
https://github.com/NullTale/LayerFx.git
```


### Installation and Usage

To start working with layers, you need to add the `LayerFx` RenderFeature to the Urp Renderer.<br>
After that, you can create layers within it and configure their parameters.<br>

Each layer works as a RenderPass, collecting objects for rendering based on `LayerMask` or using the `InLayer` component.<br>
You can access the content of the layers using the specified global texture name inside the shader,<br>
or output the blending result directly into the camera via `CombinePass`.

![image](https://github.com/NullTale/LayerFx/assets/1497430/7e2d867e-a6f2-46b8-8e5e-8ea1bc2af4a7)<br>
<sub>access to the layer texture throug shader graph (texture must be unexposed)</sub>

![_LayersAnimated](https://github.com/NullTale/LayerFx/assets/1497430/a16c4523-fac5-4914-8d71-8c61f2729922)<br>
<sub>example with animated content</sub>
