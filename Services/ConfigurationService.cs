using System;
using System.IO;
using System.Text.Json;
using GlassPane.Models;

namespace GlassPane.Services
{
    public class ConfigurationService
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GlassPane",
            "keybinds.json");

        private static ConfigurationService instance;
        private static readonly object lockObject = new object();

        public static ConfigurationService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new ConfigurationService();
                        }
                    }
                }
                return instance;
            }
        }

        private ConfigurationService()
        {
            // Ensure the directory exists
            var configDir = Path.GetDirectoryName(ConfigPath);
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
        }

        public KeybindConfiguration LoadConfiguration()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var config = JsonSerializer.Deserialize<KeybindConfiguration>(json);
                    
                    // Ensure all desktop numbers have keybinds
                    for (int i = 1; i <= 9; i++)
                    {
                        if (!config.AssignmentKeybinds.ContainsKey(i))
                        {
                            config.AssignmentKeybinds[i] = new KeybindInfo
                            {
                                Modifiers = ModifierKeys.Control,
                                Key = (VirtualKey)('0' + i),
                                Description = $"Assign to Desktop {i}"
                            };
                        }

                        if (!config.SwitchKeybinds.ContainsKey(i))
                        {
                            config.SwitchKeybinds[i] = new KeybindInfo
                            {
                                Modifiers = ModifierKeys.Alt,
                                Key = (VirtualKey)('0' + i),
                                Description = $"Switch to Desktop {i}"
                            };
                        }
                    }

                    return config;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load configuration: {ex.Message}");
            }

            // Return default configuration if loading fails
            return new KeybindConfiguration();
        }

        public void SaveConfiguration(KeybindConfiguration config)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                var json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save configuration: {ex.Message}");
                throw;
            }
        }

        public void ResetToDefaults()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    File.Delete(ConfigPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to reset configuration: {ex.Message}");
            }
        }
    }
} 