using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace OutletTweaks
{
    [BepInPlugin("bb.outlettweaks", "Outlet Tweaks", "1.7.2")]
    public class Plugin : BasePlugin
    {
        // Hier speichere ich den Logger damit ich später Log-Nachrichten schreiben kann
        public static new ManualLogSource Log;
        
        // Die Plugin Instanz brauche ich um von anderen Klassen darauf zuzugreifen
        public static Plugin instance;

        // Config-Variablen für die Einstellungen (werden aus der .cfg Datei gelesen)
        public static ConfigEntry<float> kundenMultiplikator;
        public static ConfigEntry<float> preisMultiplikator;
        public static ConfigEntry<float> mietenMultiplikator;

        public override void Load()
        {
            // Log und Instanz speichern
            Log = base.Log;
            instance = this;

            Log.LogInfo("=== Outlet Tweaks wird geladen ===");

            // Config-Datei erstellen oder laden
            // Die Config-Datei liegt in: BepInEx/config/bb.outlettweaks.cfg
            kundenMultiplikator = Config.Bind("Kunden",
                "Multiplikator",
                2f,
                "Wie viele Kunden extra spawnen sollen (2.0 = doppelt so viele, 3.0 = dreimal so viele)");

            preisMultiplikator = Config.Bind("Marktpreise",
                "Prozent",
                0.8f,
                "Prozent vom Originalpreis (0.8 = 80%, 0.5 = 50% billiger)");

            mietenMultiplikator = Config.Bind("Miete",
                "Multiplikator",
                20f,
                "Wie viel teurer die Miete sein soll (20.0 = 20x teurer, 10.0 = 10x teurer)");

            // Config-Werte ins Log schreiben damit man sieht was geladen wurde
            Log.LogInfo($"Config geladen: Kunden x{kundenMultiplikator.Value}");
            Log.LogInfo($"Config geladen: Preise {preisMultiplikator.Value * 100}%");
            Log.LogInfo($"Config geladen: Miete x{mietenMultiplikator.Value}");

            // WICHTIG: Warte 5 Sekunden bevor die Patches angewendet werden
            // Warum? Das Spiel braucht Zeit um alle Klassen zu laden
            // Wenn wir zu früh patchen crasht das Spiel!
            Log.LogInfo("Warte 5 Sekunden damit das Spiel alles laden kann...");
            
            // Coroutine starten die nach 5 Sekunden die Patches anwendet
            AddComponent<DelayedPatcher>();
        }
    }

    // Diese Klasse wartet 5 Sekunden und wendet dann die Patches an
    public class DelayedPatcher : UnityEngine.MonoBehaviour
    {
        private void Start()
        {
            // Coroutine starten
            // WrapToIl2Cpp() ist nötig weil das Spiel IL2CPP nutzt (Unity Kompilierungs-Typ)
            StartCoroutine(PatchNach5Sekunden().WrapToIl2Cpp());
        }

        private IEnumerator PatchNach5Sekunden()
        {
            // 5 Sekunden warten
            yield return new UnityEngine.WaitForSeconds(5f);

            // Jetzt patchen
            Plugin.Log.LogInfo("5 Sekunden sind um, wende jetzt Patches an...");

            try
            {
                // Harmony erstellen - das Tool das uns erlaubt Spiel-Code zu ändern
                var harmony = new Harmony("bb.outlettweaks");

                // Versuche alle Patches anzuwenden
                // Falls einer fehlschlägt soll das Spiel nicht crashen
                CustomerSpawnTweaks.PatcheAnwenden(harmony);
                MarketPriceTweaks.PatcheAnwenden(harmony);
                BillTweaks.PatcheAnwenden(harmony);

                Plugin.Log.LogInfo("=== Outlet Tweaks erfolgreich geladen ===");
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogError($"FEHLER beim Patchen: {ex.Message}");
                Plugin.Log.LogError($"Stack: {ex.StackTrace}");
            }

            // Component zerstören weil wir ihn nicht mehr brauchen
            UnityEngine.Object.Destroy(this);
        }
    }
}