// ============================================================================
// Outlet Tweaks - Market Price Reduction
// ============================================================================
// Patches the market price calculation to apply percentage-based discount.
// Uses Harmony postfix patch to modify returned prices.
// ============================================================================

using System;
using HarmonyLib;

namespace OutletTweaks
{
    internal class MarketPriceTweaks
    {
        private static int preiseGeaendert = 0;

        public static void PatcheAnwenden(Harmony harmony)
        {
            Plugin.Log.LogInfo("[Preise] Suche nach MarketPriceManagment...");

            // MarketPriceManagment Klasse finden (ja, mit 'g' am Ende, nicht 'ment')
            var priceManagerType = AccessTools.TypeByName("MarketPriceManagment");
            if (priceManagerType == null)
            {
                Plugin.Log.LogError("[Preise] MarketPriceManagment nicht gefunden!");
                return;
            }

            // GetMarketPrice Methode patchen
            PatcheGetMarketPrice(harmony, priceManagerType);

            // CheckPriceIsKnown Methoden patchen (gibts in zwei Varianten)
            PatcheCheckPriceIsKnown(harmony, priceManagerType);

            Plugin.Log.LogInfo($"[Preise] Alle Preise werden auf {Plugin.preisMultiplikator.Value * 100}% gesetzt!");
        }

        private static void PatcheGetMarketPrice(Harmony harmony, Type priceManagerType)
        {
            // GetMarketPrice(InteractionObjectPickup) finden
            var pickupType = AccessTools.TypeByName("InteractionObjectPickup");
            var getMarketPriceMethode = AccessTools.Method(priceManagerType, "GetMarketPrice", new[] { pickupType });

            if (getMarketPriceMethode != null)
            {
                var postfix = typeof(MarketPriceTweaks).GetMethod(nameof(GetMarketPrice_Postfix));
                harmony.Patch(getMarketPriceMethode, null, new HarmonyMethod(postfix));
                Plugin.Log.LogInfo("[Preise] GetMarketPrice gepatcht");
            }
        }

        private static void PatcheCheckPriceIsKnown(Harmony harmony, Type priceManagerType)
        {
            // CheckPriceIsKnown gibt es mit InteractionObjectPickup Parameter
            var pickupType = AccessTools.TypeByName("InteractionObjectPickup");
            var checkPricePickup = AccessTools.Method(priceManagerType, "CheckPriceIsKnown", new[] { pickupType });

            if (checkPricePickup != null)
            {
                var postfix = typeof(MarketPriceTweaks).GetMethod(nameof(CheckPriceIsKnown_Postfix));
                harmony.Patch(checkPricePickup, null, new HarmonyMethod(postfix));
                Plugin.Log.LogInfo("[Preise] CheckPriceIsKnown (Pickup) gepatcht");
            }

            // CheckPriceIsKnown gibt es auch mit PickupData Parameter
            var pickupDataType = AccessTools.TypeByName("PickupData");
            var checkPriceData = AccessTools.Method(priceManagerType, "CheckPriceIsKnown", new[] { pickupDataType });

            if (checkPriceData != null)
            {
                var postfix = typeof(MarketPriceTweaks).GetMethod(nameof(CheckPriceIsKnown_Postfix));
                harmony.Patch(checkPriceData, null, new HarmonyMethod(postfix));
                Plugin.Log.LogInfo("[Preise] CheckPriceIsKnown (Data) gepatcht");
            }
        }

        // Wird nach GetMarketPrice aufgerufen und ändert den Rückgabewert
        // __result ist eine spezielle Harmony Variable die den Return-Wert enthält
        // Mit 'ref' kann ich den Wert ändern bevor er ans Spiel zurückgegeben wird
        public static void GetMarketPrice_Postfix(ref float __result)
        {
            if (__result > 0) // Nur positive Preise ändern (0 oder negativ = ungültig)
            {
                __result = __result * Plugin.preisMultiplikator.Value;
            }
        }

        // Wird nach CheckPriceIsKnown aufgerufen und ändert den Preis
        public static void CheckPriceIsKnown_Postfix(ref float __result)
        {
            if (__result > 0)
            {
                float alterPreis = __result;
                __result = __result * Plugin.preisMultiplikator.Value;

                // Nur die ersten paar Änderungen loggen
                preiseGeaendert++;
                if (preiseGeaendert <= 5)
                {
                    Plugin.Log.LogInfo($"[Preise] ${alterPreis:F2} -> ${__result:F2}");
                }
                else if (preiseGeaendert == 6)
                {
                    Plugin.Log.LogInfo("[Preise] Preisänderung läuft (weitere Logs deaktiviert)");
                }
            }
        }
    }
}