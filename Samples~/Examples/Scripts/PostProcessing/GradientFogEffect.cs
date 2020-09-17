﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.PostProcessing;

// Define the Volume Component for the custom post processing effect 
[System.Serializable, VolumeComponentMenu("CustomPostProcess/Gradient Fog")]
public class GradientFogEffect : VolumeComponent
{
    [Tooltip("Controls the blending between the original and the fog color.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0, 0, 1);
    
    [Tooltip("Controls the distance at which the fog strength is 63.2%.")]
    public MinFloatParameter fogDistance = new MinFloatParameter(20, 0);

    [Tooltip("Define the near fog color.")]
    public ColorParameter nearFogColor = new ColorParameter(Color.red, true, false, true);

    [Tooltip("Define the far fog color.")]
    public ColorParameter farFogColor = new ColorParameter(Color.blue, true, false, true);
    
    [Tooltip("Define the distance at which the fog color gradient starts.")]
    public MinFloatParameter nearColorDistance = new MinFloatParameter(5, 0);
    
    [Tooltip("Define the distance at which the fog color gradient ends.")]
    public MinFloatParameter farColorDistance = new MinFloatParameter(20, 0);
}

// Define the renderer for the custom post processing effect
[CustomPostProcess("Gradient Fog", CustomPostProcessInjectPoint.AfterOpaqueAndSky)]
public class GradientFogEffectRenderer : CustomPostProcessRenderer
{
    // A variable to hold a reference to the corresponding volume component (you can define as many as you like)
    private GradientFogEffect m_VolumeComponent;

    // The postprocessing material (you can define as many as you like)
    private Material m_Material;
    
    // The ids of the shader variables
    static class ShaderIDs {
        internal readonly static int Input = Shader.PropertyToID("_MainTex");
        internal readonly static int Intensity = Shader.PropertyToID("_intensity");
        internal readonly static int Exponent = Shader.PropertyToID("_exponent");
        internal readonly static int ColorRange = Shader.PropertyToID("_colorRange");
        internal readonly static int NearFogColor = Shader.PropertyToID("_nearFogColor");
        internal readonly static int FarFogColor = Shader.PropertyToID("_farFogColor");
    }

    // By default, the effect is visible in the scene view, but we can change that here.
    public override bool visibleInSceneView => true;

    // Setup is called once so we use it to create our material
    public override void Setup()
    {
        m_Material = CoreUtils.CreateEngineMaterial("Hidden/GradientFog");
    }

    // Called once before rendering. Return true if the effect should be rendered for this camera.
    public override bool SetupCamera(ref RenderingData renderingData)
    {
        // Get the current volume stack
        var stack = VolumeManager.instance.stack;
        // Get the corresponding volume component
        m_VolumeComponent = stack.GetComponent<GradientFogEffect>();
        // if intensity value > 0, then we need to render this effect. 
        return m_VolumeComponent.intensity.value > 0;
    }

    // The actual rendering execution is done here
    public override void Render(CommandBuffer cmd, ref RenderingData renderingData, RenderTargetIdentifier source, RenderTargetIdentifier destination)
    {
        // set material properties
        if(m_Material != null){
            m_Material.SetFloat(ShaderIDs.Intensity, m_VolumeComponent.intensity.value);
            m_Material.SetFloat(ShaderIDs.Exponent, 1/m_VolumeComponent.fogDistance.value);
            m_Material.SetVector(ShaderIDs.ColorRange, new Vector2(m_VolumeComponent.nearColorDistance.value, m_VolumeComponent.farColorDistance.value));
            m_Material.SetColor(ShaderIDs.NearFogColor, m_VolumeComponent.nearFogColor.value);
            m_Material.SetColor(ShaderIDs.FarFogColor, m_VolumeComponent.farFogColor.value);
        }
        // set source texture
        cmd.SetGlobalTexture(ShaderIDs.Input, source);
        // draw a fullscreen triangle to the destination
        CoreUtils.DrawFullScreen(cmd, m_Material, destination);
    }
}