using System.Reflection;
using System.Runtime.InteropServices;
using P5R.SocialStatMultiplier;
using P5R.SocialStatMultiplier.Configuration;
using P5R.SocialStatMultiplier.Template;
using System.IO;
using Reloaded.Mod.Interfaces;

/*
 * Стенд проверяет логику мода без игры: создаётся настоящий объект Mod,
 * ему подсовывается поддельный массив очков (5 значений short), после чего
 * стенд «играет роль игры» — увеличивает значения, как это делала бы P5R,
 * и проверяет, что мод умножает прирост, не разгоняется сам, слушается
 * настроек Suspended/Enabled и не портит значения при загрузке сейва.
 */

internal class LoggerProxy : DispatchProxy
{
    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        if (targetMethod.Name is "WriteLine" or "Write" && args.Length >= 1)
            Console.WriteLine("      мод: " + Mediator.Apply(args[0]));

        return targetMethod.ReturnType == typeof(void) ? null : default;
    }
}

internal class LoaderProxy : DispatchProxy
{
    public ILogger Logger = null;
    public string ConfigDirectory = null;

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        if (targetMethod.Name == "GetLogger")
            return Logger;
        if (targetMethod.Name == "GetModConfigDirectory")
            return ConfigDirectory;

        // GetController<T>() возвращает null — мод залогирует, что сканер не найден,
        // и это нормально: массив очков стенд подставит сам.
        return targetMethod.ReturnType.IsValueType ? Activator.CreateInstance(targetMethod.ReturnType) : null;
    }
}

internal class LoaderV1Proxy : DispatchProxy
{
    public ILogger Logger = null;
    public string ConfigDirectory = null;

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        switch (targetMethod.Name)
        {
            case "GetLogger":
                return Logger;
            case "GetModConfigDirectory":
                return ConfigDirectory;
            default:
                return targetMethod.ReturnType.IsValueType ? Activator.CreateInstance(targetMethod.ReturnType) : null;
        }
    }
}

internal class ModConfigV1Proxy : DispatchProxy
{
    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        if (targetMethod.Name == "get_ModId")
            return "P5R.SocialStatMultiplier.Tests";
        if (targetMethod.Name == "get_ModName")
            return "Тестовый запуск";

        return targetMethod.ReturnType.IsValueType ? Activator.CreateInstance(targetMethod.ReturnType) : null;
    }
}

internal static class Mediator
{
    // Reloaded пишет логи через WriteLine(string) и WriteLine(string, Color).
    public static string Apply(object arg) => arg?.ToString() ?? string.Empty;
}

internal static unsafe class Program
{
    private static int _failed;
    private static int _passed;

    private static void Check(string name, long expected, long actual)
    {
        if (expected == actual)
        {
            _passed++;
            Console.WriteLine($"  [OK]   {name}: {actual}");
        }
        else
        {
            _failed++;
            Console.WriteLine($"  [ПРОВАЛ] {name}: ожидалось {expected}, получено {actual}");
        }
    }

    private static short* NewStats()
    {
        var p = (short*)Marshal.AllocHGlobal(5 * sizeof(short));
        for (var i = 0; i < 5; i++)
            p[i] = 0;
        return p;
    }

    /// <summary>«Игра» начисляет очки: значение растёт как в реальном P5R.</summary>
    private static void Award(short* stats, int index, int amount) => stats[index] = (short)(stats[index] + amount);

    private static void Settle() => Thread.Sleep(150);

    private static int Main()
    {
        Console.WriteLine("=== Проверка мода «Множитель социальных очков» ===");

        var logger = DispatchProxy.Create<ILogger, LoggerProxy>();
        var loader = DispatchProxy.Create<IModLoader, LoaderProxy>();
        ((LoaderProxy)(object)loader).Logger = logger;

        var config = new Config { Enabled = true, Multiplier = 2f, VerboseLog = true };
        var context = new ModContext
        {
            Logger = logger,
            ModLoader = loader,
            Configuration = config,
            ModConfig = null,
            Owner = null,
        };

        var mod = new Mod(context);
        var stats = NewStats();
        mod._socialStatPoints = stats;
        mod.StartWatcher();
        Settle();

        Console.WriteLine("\n1. Базовое умножение x2 (Обаяние +2 → +4)");
        Award(stats, 4, 2);
        Settle();
        Check("Обаяние после +2 при x2", 4, stats[4]);

        Console.WriteLine("\n2. Повторное начисление (ещё +3 → +6 к текущему)");
        Award(stats, 4, 3);
        Settle();
        Check("Обаяние 4 + 3*2", 10, stats[4]);

        Console.WriteLine("\n3. Мод не «разгоняется» сам по себе");
        Settle();
        Settle();
        Check("Обаяние не изменилось", 10, stats[4]);

        Console.WriteLine("\n4. Загрузка сейва (скачок больше 100 очков) не умножается");
        stats[0] = 5000;
        Settle();
        Check("Знания после скачка", 5000, stats[0]);
        Award(stats, 0, 5);
        Settle();
        Check("Знания 5000 + 5*2", 5010, stats[0]);

        Console.WriteLine("\n5. Уменьшение значения (сброс/загрузка) не «догоняется»");
        stats[2] = 20;
        Settle();
        Check("Ловкость 20 очков x2", 40, stats[2]);
        stats[2] = 5;
        Settle();
        Check("Ловкость после сброса", 5, stats[2]);

        Console.WriteLine("\n6. Смена множителя на x3 через настройки (без перезапуска игры)");
        mod.ConfigurationUpdated(new Config { Enabled = true, Multiplier = 3f, VerboseLog = false });
        Award(stats, 3, 2);
        Settle();
        Check("Доброта 0 + 2*3", 6, stats[3]);

        Console.WriteLine("\n7. Настройка «Включить мод» = выключено → очки 1 к 1");
        mod.ConfigurationUpdated(new Config { Enabled = false, Multiplier = 3f });
        Award(stats, 3, 2);
        Settle();
        Check("Доброта 6 + 2 (без умножения)", 8, stats[3]);

        Console.WriteLine("\n8. Кнопка Suspend / Resume в лоадере");
        mod.ConfigurationUpdated(new Config { Enabled = true, Multiplier = 3f });
        mod.Suspend();
        Award(stats, 3, 2);
        Settle();
        Check("Доброта 8 + 2 (пауза)", 10, stats[3]);
        mod.Resume();
        Award(stats, 3, 2);
        Settle();
        Check("Доброта 10 + 2*3 (после Resume)", 16, stats[3]);

        Console.WriteLine("\n9. Ограничение переполнения (short 32767)");
        mod.ConfigurationUpdated(new Config { Enabled = true, Multiplier = 10f });
        stats[4] = 32700;
        Settle();
        Award(stats, 4, 10);
        Settle();
        Check("Обаяние 32700 + 10*10, но не больше 32767", 32767, stats[4]);

        Console.WriteLine("\n10. Множитель x1 = обычная игра");
        mod.ConfigurationUpdated(new Config { Enabled = true, Multiplier = 1f });
        Award(stats, 1, 3);
        Settle();
        Check("Смелость 0 + 3", 3, stats[1]);

        Console.WriteLine("\n11. Выгрузка мода останавливает слежение");
        mod.Disposing();
        Thread.Sleep(100);
        stats[0] = 6000;
        Settle();
        Check("Знания после выгрузки", 6000, stats[0]);

        Console.WriteLine("\n12. Совместимость с загрузчиком: CanSuspend()/CanUnload() ДО StartEx() (та самая ошибка загрузки)");
        try
        {
            var startup = new Startup();
            var earlyCanSuspend = startup.CanSuspend();
            var earlyCanUnload = startup.CanUnload();
            startup.Suspend();
            startup.Resume();
            startup.Disposing();
            Check("CanSuspend() до StartEx() без исключения", 0, earlyCanSuspend ? 1 : 0);
            Check("CanUnload() до StartEx() без исключения", 0, earlyCanUnload ? 1 : 0);
        }
        catch (Exception e)
        {
            Check("CanSuspend()/CanUnload() до StartEx()", 0, 1);
            Console.WriteLine("      исключение: " + e);
        }

        Console.WriteLine("\n13. Полный старт мода через Startup.StartEx() (как это делает лоадер)");
        try
        {
            var configDir = Path.Combine(Path.GetTempPath(), "p5r_sst_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(configDir);

            var loaderV1 = DispatchProxy.Create<IModLoader, LoaderProxy>();
            ((LoaderProxy)(object)loaderV1).Logger = logger;
            ((LoaderProxy)(object)loaderV1).ConfigDirectory = configDir;

            var modConfigV1 = DispatchProxy.Create<IModConfig, ModConfigV1Proxy>();

            var startup = new Startup();
            startup.StartEx(loaderV1, modConfigV1);

            Check("после StartEx() CanSuspend() == true", 1, startup.CanSuspend() ? 1 : 0);
            Check("Config.json создан лоадером", 1, File.Exists(Path.Combine(configDir, "Config.json")) ? 1 : 0);
            Check("в Config.json есть множитель", 1,
                File.ReadAllText(Path.Combine(configDir, "Config.json")).Contains("Multiplier") ? 1 : 0);
            startup.Disposing();
        }
        catch (Exception e)
        {
            Check("StartEx() проходит без исключения", 0, 1);
            Console.WriteLine("      исключение: " + e);
        }

        Console.WriteLine($"\n=== Итог: пройдено {_passed}, провалено {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
