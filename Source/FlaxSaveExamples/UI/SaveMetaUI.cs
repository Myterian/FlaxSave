// Copyright © 2025 Thomas Jungclaus. All rights reserved. Released under the MIT License.

using System;
using System.Text;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxSave;

/// <summary>
/// SaveMetaUI Script.
/// </summary>
public class SaveMetaUI : Script
{
    public ControlReference<Label> SavetTitelLabel;
    public ControlReference<Label> VersionLabel;
    public ControlReference<Label> DateLabel;


    public void Bind(SaveMeta metaData)
    {
        SavetTitelLabel.Control.Text = metaData.IsAutoSave ? new LocalizedString() { Id = "AutoSave" } : metaData.DisplayName;
        VersionLabel.Control.Text = metaData.SaveVersion.ToString();

        DateTime localTime = metaData.SaveDate.ToLocalTime();
        StringBuilder dateBuilder = new();

        dateBuilder.AppendJoin(", ", localTime.ToString("d", Localization.CurrentCulture), localTime.ToString("t", Localization.CurrentCulture));
        DateLabel.Control.Text = dateBuilder.ToString();

    }
}
