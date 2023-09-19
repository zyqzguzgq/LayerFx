LayerFx is a tool for rendering objects into separate textures for working with complex graphics<br>
It has built-in functionality for blending layers, similar to Photoshop, and rendering them on the screen or into a texture.<br>

[Cover]<br>
<sub>an example of blending three layers through CombinePass the content of the layers can be used for any purposes</sub>

### Toolkit

It's a part of the overall Fx toolkit divided into three parts
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

[image]

Each layer works as a RenderPass, collecting objects for rendering based on LayerMask or using the `InLayer` component.<br>
You can access the content of the layers using the specified global texture name inside the shader<br>
or output the blending result directly into the camera through Combine.

> Examples of using layers through ShaderGraph and blending can be found in the Samples.
 
[Tech]<br>
<sub>layers blending</sub>
