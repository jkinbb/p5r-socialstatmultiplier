namespace P5R.SocialStatMultiplier
{
    /// <summary>
    /// Все тексты, которые видит пользователь: лог мода, подписи настроек, названия статов.
    ///
    /// Язык выбирается на этапе сборки:
    ///   dotnet build -c Release                    → русская версия (по умолчанию)
    ///   dotnet build -c Release -p:ModLang=EN      → английская версия (LANG_EN)
    /// Исходный код, логика и поведение у обеих версий одинаковые.
    /// </summary>
    internal static class Strings
    {
#if LANG_EN
        public const string LogTag = "[Social Stat Multiplier]";
        public const string ErrorPrefix = "ERROR: ";

        public const string NoScanner =
            "Could not get the signature scanner controller. Make sure the mod 'Reloaded.Memory.SigScan.ReloadedII' " +
            "is installed and enabled (it ships with Reloaded-II). Without it this mod cannot work.";

        public const string SignatureNotFound =
            "Social stat points signature was not found. This is probably a different p5r.exe build. " +
            "Nothing is changed - please report this to the mod author.";

        public const string BadAddress =
            "The address found for the social stat points array failed validation (values: {0}). " +
            "The mod stays inactive so nothing gets corrupted.";

        public const string ArrayFound = "Social stat points array found: 0x{0:X}. Current values: {1}";

        public const string SettingsUpdated = "Settings updated: mod {0}, multiplier x{1:0.##}.";

        public const string EnabledWord = "enabled";
        public const string DisabledWord = "disabled";

        public const string MultiplierActive =
            "Multiplier is active: x{0:0.##}. You can change it at any time, no restart needed.";

        public const string Suspended = "Mod suspended: points are awarded 1:1 again.";
        public const string Resumed = "Mod resumed.";
        public const string LoopError = "Error inside the stat watch loop (the mod keeps running).";

        public const string GainLogged = "{0}: +{1} -> +{2} (total {3}).";
        public const string JumpSkipped = "{0}: gain of +{1} (looks like a save load) - not multiplied.";

        public const string ConfigEnabledName = "Enable mod";
        public const string ConfigEnabledDescription =
            "Master switch. Unchecked - points are awarded just like in the vanilla game (1:1).";

        public const string ConfigMultiplierName = "Point multiplier";
        public const string ConfigMultiplierDescription =
            "How many times to multiply the social stat points you gain. 2-3 recommended. Valid range: 1 to 10.";

        public const string ConfigVerboseName = "Verbose log";
        public const string ConfigVerboseDescription =
            "Write every point gain to the Reloaded-II log (debugging only).";

        public static readonly string[] StatNames = { "Knowledge", "Guts", "Proficiency", "Kindness", "Charm" };
#else
        public const string LogTag = "[Множитель соц. очков]";
        public const string ErrorPrefix = "ОШИБКА: ";

        public const string NoScanner =
            "Не удалось получить контроллер сканера сигнатур. Проверь, что установлен и включён мод " +
            "'Reloaded.Memory.SigScan.ReloadedII' (он идёт в комплекте Reloaded-II). Без него мод не работает.";

        public const string SignatureNotFound =
            "Сигнатура массива очков соц. характеристик не найдена. Вероятно, это другая версия p5r.exe. " +
            "Мод ничего не меняет — сообщи об этом разработчику мода.";

        public const string BadAddress =
            "Найденный адрес массива очков не прошёл проверку (значения: {0}). " +
            "Мод остаётся неактивным, чтобы ничего не испортить.";

        public const string ArrayFound = "Массив очков соц. характеристик найден: 0x{0:X}. Текущие значения: {1}";

        public const string SettingsUpdated = "Настройки обновлены: мод {0}, множитель x{1:0.##}.";

        public const string EnabledWord = "включён";
        public const string DisabledWord = "выключен";

        public const string MultiplierActive =
            "Множитель активен: x{0:0.##}. Настройку можно менять в любой момент, перезапуск не нужен.";

        public const string Suspended = "Мод приостановлен: очки начисляются 1 к 1.";
        public const string Resumed = "Мод возобновлён.";
        public const string LoopError = "Ошибка в цикле слежения за очками (мод продолжает работать).";

        public const string GainLogged = "{0}: +{1} → +{2} (всего {3}).";
        public const string JumpSkipped = "{0}: скачок +{1} (похоже на загрузку сейва) — не умножаю.";

        public const string ConfigEnabledName = "Включить мод";
        public const string ConfigEnabledDescription =
            "Общий выключатель. Галочка снята — очки начисляются как в обычной игре (1 к 1).";

        public const string ConfigMultiplierName = "Множитель получаемых очков";
        public const string ConfigMultiplierDescription =
            "Во сколько раз умножать очки социальных характеристик. Рекомендуется 2–3. Допустимо от 1 до 10.";

        public const string ConfigVerboseName = "Подробный журнал";
        public const string ConfigVerboseDescription =
            "Писать в лог Reloaded-II каждое начисление очков (нужно только для отладки).";

        public static readonly string[] StatNames =
        {
            "Знания (Knowledge)",
            "Смелость (Guts)",
            "Ловкость (Proficiency)",
            "Доброта (Kindness)",
            "Обаяние (Charm)",
        };
#endif
    }
}
