# Getting Started
## URP Renderer Setup
Firstly, the URP [Universal Renderer](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.0/manual/urp-universal-renderer.html) will need some settings updated for the Raymarching to be supported. 

Change the URP renderer to use the [Deferred Rendering Path](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.0/manual/rendering/deferred-rendering-path.html) - Raymarching is not supported on the Forward renderer. 

The [[RaymarchRenderFeature]] needs to be added to the renderer (this is at the bottom of the Universal Renderer Asset, click 'Add Renderer Feature'). For more information on adding Render Features click [here](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.0/manual/urp-renderer-feature-how-to-add.html) to go to the docs. The  [[RaymarchRenderFeature]] includes any settings that are used for setting up the custom render pass.

## Scene Setup
Each scene that will render any Raymarched objects needs to include a [[RaymarchScene]]. Right click in the hierarchy -> Raymarching -> Create Scene. There can only be one Raymarch Scene per Unity scene.

Add [[RaymarchObjects]] to the scene. Right click in the hierarchy -> Raymarching -> Create Object. This will create an empty Raymarch object in the scene, for the object to render assign an [[SDFShaderFeature]] (there are some common objects already included in the project).

To update the shader that renders all the objects in the scene click the "Regenerate Shader" button on the [[RaymarchScene]].

You should now have a basic scene with Raymarched Objects.