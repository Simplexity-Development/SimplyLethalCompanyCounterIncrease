using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace SimplyLethalCompanyCounterIncrease
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [HarmonyPatch]
    public class Main : BaseUnityPlugin
    {
        private const string modGUID = "SimplexityDev.SimplyLethalCompanyCounterIncrease";
        private const string modName = "Simply Lethal Company Counter Increase";
        private const string modVersion = "1.0.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        private static ManualLogSource logger;
        
        // TODO: Make configurable
        private static int maxItemsOnDesk = 30;

        void Awake()
        {
            logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(DepositItemsDesk), nameof(DepositItemsDesk.PlaceItemOnCounter))]
        static void Postfix(DepositItemsDesk __instance, PlayerControllerB playerWhoTriggered)
        {
            if (__instance.inGrabbingObjectsAnimation) return;
            if (__instance.deskObjectsContainer.GetComponentsInChildren<GrabbableObject>().Length < 12) return;
            if (__instance.deskObjectsContainer.GetComponentsInChildren<GrabbableObject>().Length >= maxItemsOnDesk) return;
            if (GameNetworkManager.Instance != null && playerWhoTriggered == GameNetworkManager.Instance.localPlayerController)
            {
                Vector3 vector = RoundManager.RandomPointInBounds(__instance.triggerCollider.bounds);
                vector.y = __instance.triggerCollider.bounds.min.y;
                RaycastHit raycastHit;
                if (Physics.Raycast(new Ray(vector + Vector3.up * 3f, Vector3.down), out raycastHit, 8f, 1048640, QueryTriggerInteraction.Collide))
                {
                    vector = raycastHit.point;
                }
                vector.y += playerWhoTriggered.currentlyHeldObjectServer.itemProperties.verticalOffset;
                vector = __instance.deskObjectsContainer.transform.InverseTransformPoint(vector);
                __instance.AddObjectToDeskServerRpc(playerWhoTriggered.currentlyHeldObjectServer.gameObject.GetComponent<NetworkObject>());
                playerWhoTriggered.DiscardHeldObject(true, __instance.deskObjectsContainer, vector, false);
                logger.LogDebug("discard held object called from deposit items desk");
            }
        }

    }
}