using P5R.SocialStatMultiplier.Configuration;
using P5R.SocialStatMultiplier.Template;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;

namespace P5R.SocialStatMultiplier
{
    /// <summary>
    /// P5R Social Stat Multiplier — умножает все получаемые очки социальных характеристик.
    ///
    /// Как это работает:
    ///   Очки соц. характеристик лежат в памяти игры одним массивом из 5 значений типа short:
    ///   [0] Knowledge (Знания), [1] Guts (Смелость), [2] Proficiency (Ловкость),
    ///   [3] Kindness (Доброта), [4] Charm (Обаяние).
    ///   Адрес массива находится по сигнатуре "4C 8D 35 ?? ?? ?? ?? 0F B7 FD" (lea r14,[rip+...] + movzx edi,ch).
    ///   Мод раз в 20 мс читает массив; как только значение выросло (игра начислила очки),
    ///   разница домножается на множитель из настроек и записывается обратно.
    ///   Никакие файлы игры не правятся: выключил галочку — начисление снова 1 к 1.
    /// </summary>
    public unsafe class Mod : ModBase
    {
        private const string Tag = Strings.LogTag;

        /// <summary>Период опроса массива очков, мс.</summary>
        private const int PollIntervalMs = 20;

        /// <summary>Максимальное правдоподобное разовое начисление. Больше — считаем это загрузкой сейва и не трогаем.</summary>
        private const int MaxPlausibleGain = 100;

        /// <summary>Верхняя граница «разумных» значений при проверке найденного адреса.</summary>
        private const short SanityMax = 32000;

        private static readonly string[] StatNames = Strings.StatNames;

        private readonly IModLoader _modLoader;
        private readonly ILogger _logger;
        private readonly IMod _owner;
        private readonly IModConfig _modConfig;

        private Config _configuration;

        private nuint _baseAddress;
        internal short* _socialStatPoints;
        internal volatile bool _watcherRunning;

        private CancellationTokenSource? _cancellationTokenSource;

        // Настройки, читаемые рабочим потоком (меняются на лету через конфиг).
        private volatile bool _enabled = true;
        private volatile bool _paused = false;
        private volatile bool _verboseLog = false;
        private volatile int _multiplierPercent = 200; // множитель * 100 (чтобы не тянуть float в поток)

        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _logger = context.Logger;
            _owner = context.Owner;
            _modConfig = context.ModConfig;
            _configuration = context.Configuration;

            using (var process = Process.GetCurrentProcess())
                _baseAddress = (nuint)process.MainModule!.BaseAddress;

            ApplyConfiguration(_configuration, announce: false);

            var scannerController = _modLoader.GetController<IStartupScanner>();
            if (scannerController == null || !scannerController.TryGetTarget(out var startupScanner) || startupScanner == null)
            {
                Error(Strings.NoScanner);
                return;
            }

            startupScanner.AddMainModuleScan("4C 8D 35 ?? ?? ?? ?? 0F B7 FD", result =>
            {
                if (!result.Found)
                {
                    Error(Strings.SignatureNotFound);
                    return;
                }

                var match = _baseAddress + (nuint)result.Offset;
                var points = (short*)GetGlobalAddress(match + 3);

                // Страховка: если сигнатура нашлась в неожиданном месте, писать туда нельзя.
                var probe = new short[5];
                var sane = true;
                for (var i = 0; i < 5; i++)
                {
                    probe[i] = points[i];
                    if (probe[i] < 0 || probe[i] > SanityMax)
                        sane = false;
                }

                if (!sane)
                {
                    Error(string.Format(Strings.BadAddress, string.Join(", ", probe)));
                    return;
                }

                _socialStatPoints = points;
                Info(string.Format(Strings.ArrayFound, (ulong)points, string.Join(", ", probe)));
                StartWatcher();
            });
        }

        /// <summary>Пересчитывает внутренние поля из конфига.</summary>
        private void ApplyConfiguration(Config configuration, bool announce)
        {
            _enabled = configuration.Enabled;

            var multiplier = configuration.Multiplier;
            if (multiplier < 1f) multiplier = 1f;
            if (multiplier > 10f) multiplier = 10f;
            _multiplierPercent = (int)Math.Round(multiplier * 100f);

            _verboseLog = configuration.VerboseLog;

            if (announce)
                Info(string.Format(Strings.SettingsUpdated, _enabled ? Strings.EnabledWord : Strings.DisabledWord, multiplier));
        }

        internal void StartWatcher()
        {
            if (_watcherRunning)
                return;

            _watcherRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            var thread = new Thread(() => WatchLoop(token))
            {
                IsBackground = true,
                Name = "P5R Social Stat Multiplier",
            };
            thread.Start();

            Info(string.Format(Strings.MultiplierActive, _multiplierPercent / 100.0));
        }

        /// <summary>
        /// Основной цикл: следит за массивом очков и умножает прирост.
        /// </summary>
        private void WatchLoop(CancellationToken token)
        {
            var last = new short[5];
            for (var i = 0; i < 5; i++)
                last[i] = _socialStatPoints[i];

            while (!token.IsCancellationRequested)
            {
                try
                {
                    for (var i = 0; i < 5; i++)
                    {
                        var current = _socialStatPoints[i];
                        var gain = current - last[i];

                        if (_enabled && !_paused)
                        {
                            var boosted = BoostValue(last[i], current, _multiplierPercent);
                            if (boosted != current)
                            {
                                _socialStatPoints[i] = boosted;
                                current = boosted;

                                if (_verboseLog)
                                    Info(string.Format(Strings.GainLogged, StatNames[i], gain, boosted - last[i], boosted));
                            }
                            else if (_verboseLog && gain > MaxPlausibleGain)
                            {
                                Info(string.Format(Strings.JumpSkipped, StatNames[i], gain));
                            }
                        }

                        last[i] = current;
                    }
                }
                catch (Exception exception)
                {
                    Error(Strings.LoopError, exception);
                }

                Thread.Sleep(PollIntervalMs);
            }
        }

        /* ---------- реакции Reloaded-II ---------- */

        public override bool CanSuspend() => true;
        public override bool CanUnload() => false;

        /// <summary>Кнопка «Suspend» в лоадере: мод перестаёт множить очки, не выгружаясь.</summary>
        public override void Suspend()
        {
            _paused = true;
            Info(Strings.Suspended);
        }

        public override void Resume()
        {
            _paused = false;
            Info(Strings.Resumed);
        }

        public override void ConfigurationUpdated(Config configuration)
        {
            _configuration = configuration;
            ApplyConfiguration(configuration, announce: true);
        }

        public override void Disposing()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _watcherRunning = false;
        }

        /* ---------- вспомогательное ---------- */

        /// <summary>
        /// Считает новое значение очков при приросте. Возвращает текущее значение,
        /// если умножать нечего (нет прироста, множитель = 1, скачок похож на загрузку сейва).
        /// </summary>
        internal static short BoostValue(short last, short current, int multiplierPercent)
        {
            var gain = current - last;

            if (gain <= 0 || gain > MaxPlausibleGain || multiplierPercent <= 100)
                return current;

            var boosted = last + (long)Math.Round(gain * (multiplierPercent / 100.0), MidpointRounding.AwayFromZero);
            if (boosted > short.MaxValue)
                boosted = short.MaxValue;
            if (boosted < 0)
                boosted = 0;

            return (short)boosted;
        }

        /// <summary>
        /// Достаёт адрес глобала из инструкции вида "lea reg,[rip+смещение]"
        /// (ptrAddress — адрес самого смещения, т.е. инструкция + размер опкода/префиксов).
        /// </summary>
        private static unsafe nuint GetGlobalAddress(nuint ptrAddress)
        {
            return (nuint)(*(int*)ptrAddress) + ptrAddress + 4;
        }

        private void Info(string message) => _logger.WriteLine($"{Tag} {message}");

        private void Error(string message) => _logger.WriteLine($"{Tag} {Strings.ErrorPrefix}{message}");

        private void Error(string message, Exception exception) =>
            _logger.WriteLine($"{Tag} {Strings.ErrorPrefix}{message} ({exception.GetType().Name}: {exception.Message})");
    }
}
