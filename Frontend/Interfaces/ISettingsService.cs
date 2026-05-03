using ClearText.DataObjects;
using ClearText.Enums;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ClearText.Interfaces;

public interface ISettingsService
{
    SettingsConfig Config { get; }
    void LoadSettings();
    void UpdateSettings(SettingsConfig config);
    SettingsConfig CreateWorkingCopy();
}