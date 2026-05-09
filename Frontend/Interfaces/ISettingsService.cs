using ClearText.DataObjects;

namespace ClearText.Interfaces;

public interface ISettingsService
{
    SettingsConfig Config { get; }
    void UpdateSettings(SettingsConfig config);
    SettingsConfig CreateWorkingCopy();
}