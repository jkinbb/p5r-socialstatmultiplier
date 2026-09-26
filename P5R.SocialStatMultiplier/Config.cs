using P5R.SocialStatMultiplier.Template.Configuration;
using System.ComponentModel;

namespace P5R.SocialStatMultiplier.Configuration
{
    /// <summary>
    /// Настройки мода. Меняются в интерфейсе Reloaded-II (вкладка с иконкой шестерёнки
    /// рядом с модом) или прямо в файле Config.json — мод подхватывает изменения на ходу,
    /// перезапускать игру не нужно.
    /// </summary>
    public class Config : Configurable<Config>
    {
        [DisplayName(Strings.ConfigEnabledName)]
        [Description(Strings.ConfigEnabledDescription)]
        public bool Enabled { get; set; } = true;

        [DisplayName(Strings.ConfigMultiplierName)]
        [Description(Strings.ConfigMultiplierDescription)]
        public float Multiplier { get; set; } = 2.0f;

        [DisplayName(Strings.ConfigVerboseName)]
        [Description(Strings.ConfigVerboseDescription)]
        public bool VerboseLog { get; set; } = false;
    }
}
