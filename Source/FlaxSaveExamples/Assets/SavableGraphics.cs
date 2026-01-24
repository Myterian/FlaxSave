// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using FlaxEngine;

namespace FlaxSave;

/// <summary>Class to use in a Json Asset for saving and loading Graphics and Video settings at runtime</summary>
public class SavableGraphics : ISavableAsset
{
    public GameWindowMode GameWindowMode;
    public Float2 ScreenSize;
    public PostProcessSettings PostProcessSettings;
    
    public Quality AAQuality;
    public Quality GIQuality;
    public Quality GlobalSDFQuality;
    public Quality ShadowMapsQuality;
    public Quality SSAOQuality;
    public Quality SSRQuality;
    public Quality VolumetricFogQuality;

    public bool AllowCSMBlending;
    public bool GICascadesBlending;
    public bool UseVSync;

    public void SaveAction()
    {
        AAQuality = Graphics.AAQuality;
        AllowCSMBlending = Graphics.AllowCSMBlending;
        GICascadesBlending = Graphics.GICascadesBlending;
        GIQuality = Graphics.GIQuality;
        GlobalSDFQuality = Graphics.GlobalSDFQuality;
        PostProcessSettings = Graphics.PostProcessSettings;
        ShadowMapsQuality = Graphics.ShadowMapsQuality;
        SSAOQuality = Graphics.SSAOQuality;
        SSRQuality = Graphics.SSRQuality;
        UseVSync = Graphics.UseVSync;
        VolumetricFogQuality = Graphics.VolumetricFogQuality;

        GameWindowMode = Screen.GameWindowMode;
        ScreenSize = Screen.Size;
    }

    public void LoadAction()
    {
        Graphics.AAQuality = AAQuality;
        Graphics.AllowCSMBlending = AllowCSMBlending;
        Graphics.GICascadesBlending = GICascadesBlending;
        Graphics.GIQuality = GIQuality;
        Graphics.GlobalSDFQuality = GlobalSDFQuality;
        Graphics.PostProcessSettings = PostProcessSettings;
        Graphics.ShadowMapsQuality = ShadowMapsQuality;
        Graphics.SSAOQuality = SSAOQuality;
        Graphics.SSRQuality = SSRQuality;
        Graphics.UseVSync = UseVSync;
        Graphics.VolumetricFogQuality = VolumetricFogQuality;

        Screen.Size = ScreenSize;
        Screen.GameWindowMode = GameWindowMode;

        if (GameWindowMode != GameWindowMode.Fullscreen && GameWindowMode != GameWindowMode.FullscreenBorderless)
            Screen.MainWindow.Position = Platform.GetMonitorBounds(Screen.MainWindow.Position).Center - (ScreenSize / 2f);
        
        // TODO: Put game window on correct monitor
    }
}
