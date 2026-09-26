/*
 * This file and other files in the `Template` folder are intended to be left unedited (if possible),
 * to make it easier to upgrade to newer versions of the template.
*/

using P5R.SocialStatMultiplier.Configuration;
using P5R.SocialStatMultiplier.Template.Configuration;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System.IO;

namespace P5R.SocialStatMultiplier.Template
{
    public class Startup : IMod
    {
        /// <summary>
        /// Used for writing text to the Reloaded log.
        /// </summary>
        private ILogger _logger = null!;

        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private IModLoader _modLoader = null!;

        /// <summary>
        /// Stores the contents of your mod's configuration. Automatically updated by template.
        /// </summary>
        private Config _configuration = null!;

        /// <summary>
        /// Configuration of the current mod.
        /// </summary>
        private IModConfig _modConfig = null!;

        /// <summary>
        /// Encapsulates your mod logic.
        /// </summary>
        /// <remarks>
        /// ВАЖНО: лоадер вызывает CanSuspend()/CanUnload() из конструктора ModInstance,
        /// то есть ДО StartEx(). Поэтому поле изначально null и все обращения к нему
        /// идут через ?. — иначе загрузка мода падает с NullReferenceException
        /// (ошибка «Failed to Load Reloaded-II» в логе).
        /// </remarks>
        private ModBase? _mod;

        /// <summary>
        /// Entry point for your mod.
        /// </summary>
        public void StartEx(IModLoaderV1 loaderApi, IModConfigV1 modConfig)
        {
            _modLoader = (IModLoader)loaderApi;
            _modConfig = (IModConfig)modConfig;
            _logger = (ILogger)_modLoader.GetLogger();

            // Your config file is in Config.json.
            // Need a different name, format or more configurations? Modify the `Configurator`.
            // If you do not want a config, remove Configuration folder and Config class.
            var configDirectory = _modLoader.GetModConfigDirectory(_modConfig.ModId);

            // Страховка: FileSystemWatcher внутри Configurable падает, если папки нет.
            Directory.CreateDirectory(configDirectory);

            var configurator = new Configurator(configDirectory);
            _configuration = configurator.GetConfiguration<Config>(0);
            _configuration.ConfigurationUpdated += OnConfigurationUpdated;

            // Создаём Config.json сразу при первом запуске, чтобы настройки можно было
            // править блокнотом, не открывая интерфейс лоадера (значения = загруженные).
            _configuration.Save?.Invoke();

            // Please put your mod code in the class below,
            // use this class for only interfacing with mod loader.
            _mod = new Mod(new ModContext()
            {
                Logger = _logger,
                ModLoader = _modLoader,
                ModConfig = _modConfig,
                Owner = this,
                Configuration = _configuration,
            });
        }

        private void OnConfigurationUpdated(IConfigurable obj)
        {
            /*
                This is executed when the configuration file gets 
                updated by the user at runtime.
            */

            // Replace configuration with new.
            _configuration = (Config)obj;
            _mod?.ConfigurationUpdated(_configuration);
        }

        /* Mod loader actions. */
        public void Suspend() => _mod?.Suspend();
        public void Resume() => _mod?.Resume();
        public void Unload() => _mod?.Unload();

        /*  If CanSuspend == false, suspend and resume button are disabled in Launcher and Suspend()/Resume() will never be called.
            If CanUnload == false, unload button is disabled in Launcher and Unload() will never be called.
        */
        public bool CanUnload() => _mod?.CanUnload() ?? false;
        public bool CanSuspend() => _mod?.CanSuspend() ?? false;

        /* Automatically called by the mod loader when the mod is about to be unloaded. */
        public Action Disposing => () => _mod?.Disposing();
    }
}