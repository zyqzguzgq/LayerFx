LayerFx is a tool for rendering objects into separate textures for working with complex graphics<br>
It has built-in functionality for blending layers, similar to Photoshop, and rendering them on the screen or into a texture.<br>


![_Tech](https://github.com/NullTale/LayerFx/assets/1497430/92d4af5a-23f4-4137-9bc5-ad670f00fe95)<br>
<sub>layers blending</sub>


> example from documentation can be found in the package samples

### Toolkit

LayerFx it's a part of the overall Fx toolkit divided into three parts
- [VolFx](https://github.com/NullTale/VolFx) - customizable post-processing with LayerMask filtering and texture processing
- [ScreenFx](https://github.com/NullTale/ScreenFx) - custom timeline tracks for screen space Fx
- LayerFx - render feature to draw objects into buffers, can be used as photoshop layers

### Package Manager Url

Install via [Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html)
```
https://github.com/NullTale/LayerFx.git
```


### Installation and Usage

To start working with layers, you need to add the `LayerFx` RenderFeature to the Urp Renderer<br>
After that, you can create layers within it and configure their parameters<br>

![_LayerFx](https://github.com/NullTale/LayerFx/assets/1497430/cc823ba9-895a-41ef-bc37-ff91aa54cb30)

Each layer works as a RenderPass, collecting objects for rendering based on `LayerMask` or using the `InLayer` component.<br>
You can access the content of the layers using the specified global texture name inside the shader<br>
or output the blending result directly into the camera via `CombinePass`.

![image](https://github.com/NullTale/LayerFx/assets/1497430/4094d8dd-8cf0-4433-a008-9cf2d55d720c)<br>
<sub>access to the layer texture throug shader graph</sub>

![_Cover](https://github.com/NullTale/LayerFx/assets/1497430/9fbc659d-eb1a-4eea-9f7f-bb75bbb9b7e8)<br>
<sub>example with animated content, the content of the layers can be used for any purposes through a shader</sub>
