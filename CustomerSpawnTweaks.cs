using System;
using HarmonyLib;

namespace OutletTweaks
{
    internal class CustomerSpawnTweaks
    {
        // Zähler für gespawnte Kunden (nur für Logging)
        private static int kundenGespawnt = 0;
        
        // Flag das verhindert dass wir uns selbst triggern (Rekursion)
        private static bool amSpawnen = false;

        public static void PatcheAnwenden(Harmony harmony)
        {
            Plugin.Log.LogInfo("[Kunden] Suche nach SendClient Methode...");

            // ClientSender Klasse finden
            var clientSenderType = AccessTools.TypeByName("ClientSender");
            if (clientSenderType == null)
            {
                Plugin.Log.LogError("[Kunden] ClientSender Klasse nicht gefunden!");
                return;
            }

            // SendClient Methode finden (die spawnt die Kunden)
            var sendClientMethode = AccessTools.Method(clientSenderType, "SendClient");
            if (sendClientMethode == null)
            {
                Plugin.Log.LogError("[Kunden] SendClient Methode nicht gefunden!");
                return;
            }

            // Postfix Patch dranhängen - wird nach jedem SendClient Aufruf ausgeführt
            var postfixMethode = typeof(CustomerSpawnTweaks).GetMethod(nameof(SendClient_Postfix));
            harmony.Patch(sendClientMethode, null, new HarmonyMethod(postfixMethode));

            Plugin.Log.LogInfo("[Kunden] Patch erfolgreich aktiviert!");
            Plugin.Log.LogInfo($"[Kunden] Pro Spawn kommen jetzt {Plugin.kundenMultiplikator.Value}x Kunden");
        }

        // Diese Methode wird nach jedem SendClient Aufruf ausgeführt
        // __instance ist eine spezielle Variable die Harmony automatisch übergibt
        // Sie enthält die ClientSender Instanz auf der SendClient aufgerufen wurde
        // Brauche ich um SendClient auf der gleichen Instanz nochmal aufzurufen
        public static void SendClient_Postfix(object __instance)
        {
            // WICHTIG: Wenn wir schon am Spawnen sind, sofort abbrechen
            // Sonst triggert unser eigener Aufruf den Postfix wieder = Endlosschleife = Crash
            if (amSpawnen)
                return;

            // Wie viele extra Kunden spawnen? (Config Wert - 1, weil einer schon gespawnt wurde)
            int extraKunden = (int)Plugin.kundenMultiplikator.Value - 1;
            
            // Keine extra Kunden? Dann nichts tun
            if (extraKunden <= 0)
                return;

            // Flag setzen: Wir spawnen jetzt extra Kunden
            amSpawnen = true;

            try
            {
                // SendClient Methode holen
                var clientSenderType = AccessTools.TypeByName("ClientSender");
                var sendMethod = AccessTools.Method(clientSenderType, "SendClient");

                // Für jeden extra Kunden die SendClient Methode nochmal aufrufen
                for (int i = 0; i < extraKunden; i++)
                {
                    try
                    {
                        
                        // Jeden Spawn einzeln versuchen - falls einer fehlschlägt sollen die anderen trotzdem spawnen
                        sendMethod.Invoke(__instance, new object[] { });
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogError($"[Kunden] Fehler beim Spawnen von Extra-Kunde #{i + 1}: {ex.Message}");
                    }
                }

                // Nur die ersten paar Spawns loggen damit das Log nicht vollgemüllt wird
                kundenGespawnt++;
                if (kundenGespawnt <= 3)
                {
                    Plugin.Log.LogInfo($"[Kunden] Kunde #{kundenGespawnt} gespawnt + {extraKunden} extra");
                }
                else if (kundenGespawnt == 4)
                {
                    Plugin.Log.LogInfo("[Kunden] Spawning läuft gut (weitere Logs deaktiviert)");
                }
            }
            finally
            {
                // WICHTIG: Flag zurücksetzen, auch wenn ein Fehler passiert ist
                amSpawnen = false;
            }
        }
    }
}