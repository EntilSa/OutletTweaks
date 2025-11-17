// ============================================================================
// Outlet Tweaks - Rent Multiplier
// ============================================================================
// Patches the rent/bill creation system to increase shop rent costs.
// Uses Harmony prefix patch to multiply rent amounts.
// ============================================================================

using System;
using HarmonyLib;

namespace OutletTweaks
{
    internal class BillTweaks
    {
        private static bool rentValueGeaendert = false;

        public static void PatcheAnwenden(Harmony harmony)
        {
            Plugin.Log.LogInfo("[Miete] Suche nach BillManager...");

            // BillManager Klasse finden
            var billManagerType = AccessTools.TypeByName("BillManager");
            if (billManagerType == null)
            {
                Plugin.Log.LogError("[Miete] BillManager nicht gefunden!");
                return;
            }

            // GetRent Methode patchen
            PatcheGetRent(harmony, billManagerType);

            // CreateRentBill Methode patchen
            PatcheCreateRentBill(harmony, billManagerType);

            // CreateBill Methode patchen
            PatcheCreateBill(harmony, billManagerType);

            Plugin.Log.LogInfo($"[Miete] Miete wird x{Plugin.mietenMultiplikator.Value} erhöht!");
        }

        private static void PatcheGetRent(Harmony harmony, Type billManagerType)
        {
            var getRentMethode = AccessTools.Method(billManagerType, "GetRent");
            if (getRentMethode != null)
            {
                var postfix = typeof(BillTweaks).GetMethod(nameof(GetRent_Postfix));
                harmony.Patch(getRentMethode, null, new HarmonyMethod(postfix));
                Plugin.Log.LogInfo("[Miete] GetRent gepatcht");
            }
        }

        private static void PatcheCreateRentBill(Harmony harmony, Type billManagerType)
        {
            var createRentBillMethode = AccessTools.Method(billManagerType, "CreateRentBill");
            if (createRentBillMethode != null)
            {
                var prefix = typeof(BillTweaks).GetMethod(nameof(CreateRentBill_Prefix));
                harmony.Patch(createRentBillMethode, new HarmonyMethod(prefix), null);
                Plugin.Log.LogInfo("[Miete] CreateRentBill gepatcht");
            }
        }

        private static void PatcheCreateBill(Harmony harmony, Type billManagerType)
        {
            var createBillMethode = AccessTools.Method(billManagerType, "CreateBill", 
                new[] { typeof(int), typeof(int), typeof(float) });
            
            if (createBillMethode != null)
            {
                var prefix = typeof(BillTweaks).GetMethod(nameof(CreateBill_Prefix));
                harmony.Patch(createBillMethode, new HarmonyMethod(prefix), null);
                Plugin.Log.LogInfo("[Miete] CreateBill gepatcht");
            }
        }

        // Wird nach GetRent aufgerufen und multipliziert die Miete
        public static void GetRent_Postfix(ref float __result)
        {
            float alteMiete = __result;
            __result = __result * Plugin.mietenMultiplikator.Value;
            
            Plugin.Log.LogInfo($"[Miete] Berechnet: ${alteMiete:F2}/Tag -> ${__result:F2}/Tag");
        }

        // Wird vor CreateRentBill aufgerufen
        // Hier ändern wir die statische Variable rentBillValuePerLevel direkt
        // Musste ich machen weil GetRent alleine nicht gereicht hat - die Rechnung
        // holt sich den Wert auch aus dieser Variable
        public static void CreateRentBill_Prefix()
        {
            if (!rentValueGeaendert) // Nur einmal ändern, nicht bei jedem CreateRentBill Aufruf
            {
                try
                {
                    var billManagerType = AccessTools.TypeByName("BillManager");
                    var rentValueProperty = AccessTools.Property(billManagerType, "rentBillValuePerLevel");
                    
                    if (rentValueProperty != null)
                    {
                        float alterWert = (float)rentValueProperty.GetValue(null);
                        float neuerWert = alterWert * Plugin.mietenMultiplikator.Value;
                        rentValueProperty.SetValue(null, neuerWert);
                        
                        Plugin.Log.LogInfo($"[Miete] rentBillValuePerLevel: {alterWert} -> {neuerWert}");
                        rentValueGeaendert = true;
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogWarning($"[Miete] Konnte rentBillValuePerLevel nicht ändern: {ex.Message}");
                }
            }
        }

        // Wird vor CreateBill aufgerufen und multipliziert den Betrag
        // 'ref' ist wichtig weil ich den Parameter ändern will bevor die Methode ihn nutzt
        public static void CreateBill_Prefix(int startDay, int endDay, ref float value)
        {
            float alterWert = value;
            value = value * Plugin.mietenMultiplikator.Value;
            
            Plugin.Log.LogInfo($"[Miete] Bill erstellt Tag {startDay}-{endDay}: ${alterWert:F2} -> ${value:F2}");
        }
    }
}