// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.TestAdapter.Settings;

using System.Xml;
using System.Xml.Serialization;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

[SettingsName(GdUnit4Settings.RUN_SETTINGS_XML_NODE)]

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class GdUnit4SettingsProvider : ISettingsProvider
{
    private GdUnit4Settings Settings { get; set; } = new();

    private XmlSerializer Serializer { get; } = new(typeof(GdUnit4Settings));

    public void Load(XmlReader reader)
    {
        Console.WriteLine("[GdUnit4] GdUnit4SettingsProvider.Load() called");
        try
        {
            if (reader.Read() && reader.Name == GdUnit4Settings.RUN_SETTINGS_XML_NODE)
            {
                Console.WriteLine($"[GdUnit4] Found {GdUnit4Settings.RUN_SETTINGS_XML_NODE} node in XML");
                var settings = Serializer.Deserialize(reader) as GdUnit4Settings;
                Settings = settings ?? new GdUnit4Settings();
                Console.WriteLine($"[GdUnit4] Deserialized settings - GodotProjectDir: '{Settings.GodotProjectDir ?? "(null)"}'");
            }
            else
            {
                Console.WriteLine($"[GdUnit4] No {GdUnit4Settings.RUN_SETTINGS_XML_NODE} node found, reader.Name = '{reader.Name}'");
            }
        }
#pragma warning disable CA1031
        catch (Exception e)
#pragma warning restore CA1031
        {
            Console.WriteLine($"[GdUnit4] Loading GdUnit4 Adapter settings failed! {e}");
        }
    }

    internal static GdUnit4Settings LoadSettings(IDiscoveryContext discoveryContext)
    {
        var gdUnitSettingsProvider = discoveryContext.RunSettings?.GetSettings(GdUnit4Settings.RUN_SETTINGS_XML_NODE) as GdUnit4SettingsProvider;
        var settings = gdUnitSettingsProvider?.Settings ?? new GdUnit4Settings();
        Console.WriteLine($"[GdUnit4] LoadSettings - provider is {(gdUnitSettingsProvider == null ? "null" : "not null")}, GodotProjectDir: '{settings.GodotProjectDir ?? "(null)"}'");
        return settings;
    }
}
