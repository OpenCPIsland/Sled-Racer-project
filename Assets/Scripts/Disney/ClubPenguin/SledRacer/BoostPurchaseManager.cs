using Disney.ClubPenguin.Service.MWS;
using Disney.ClubPenguin.Service.MWS.Domain;
using Disney.DMOAnalytics.Framework;
using InAppPurchases;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
    public class BoostPurchaseManager
    {
        private bool isLoading;

        public HashSet<BoostManager.AvailableBoosts> OwnedBoosts { get; private set; }

        public bool IsLoading
        {
            get { return isLoading; }
            private set { }
        }

        public event EventHandler OnLoadComplete;

        public BoostPurchaseManager()
        {
            OwnedBoosts = new HashSet<BoostManager.AvailableBoosts>();
            ForceClaimAllBoosts(); // Forcefully claim all boosts on initialization
            loadPurchasesFromDisk();
        }

        private static BoostManager.AvailableBoosts? getBoostBySKU(string sku)
        {
            switch (sku)
            {
                case "disney.clubpenguinsledracer.invincitube":
                case "disney.clubpenguinsledracer.invincitube.2211063":
                    return BoostManager.AvailableBoosts.INVULNERABLE;
                case "disney.clubpenguinsledracer.jetpackboost":
                case "disney.clubpenguinsledracer.jetpackboost.2211068":
                    return BoostManager.AvailableBoosts.JETPACK;
                case "disney.clubpenguinsledracer.parachute":
                case "disney.clubpenguinsledracer.parachute.2211064":
                    return BoostManager.AvailableBoosts.PARACHUTE;
                case "disney.clubpenguinsledracer.revive":
                case "disney.clubpenguinsledracer.revive.2211066":
                    return BoostManager.AvailableBoosts.REVIVE;
                case "disney.clubpenguinsledracer.tubewax":
                case "disney.clubpenguinsledracer.tubewax.2211067":
                    return BoostManager.AvailableBoosts.SKIWAX;
                case "disney.clubpenguinsledracer.tootblaster":
                case "disney.clubpenguinsledracer.tootblaster.2211065":
                    return BoostManager.AvailableBoosts.WHOOPIECUSHION;
                default:
                    return null;
            }
        }

        private static string getSKUByBoost(BoostManager.AvailableBoosts boost)
        {
            switch (boost)
            {
                case BoostManager.AvailableBoosts.INVULNERABLE:
                    return "disney.clubpenguinsledracer.invincitube.2211063";
                case BoostManager.AvailableBoosts.JETPACK:
                    return "disney.clubpenguinsledracer.jetpackboost.2211068";
                case BoostManager.AvailableBoosts.PARACHUTE:
                    return "disney.clubpenguinsledracer.parachute.2211064";
                case BoostManager.AvailableBoosts.REVIVE:
                    return "disney.clubpenguinsledracer.revive.2211066";
                case BoostManager.AvailableBoosts.SKIWAX:
                    return "disney.clubpenguinsledracer.tubewax.2211067";
                case BoostManager.AvailableBoosts.WHOOPIECUSHION:
                    return "disney.clubpenguinsledracer.tootblaster.2211065";
                default:
                    return null;
            }
        }

        public void OnLogin(bool isMember)
        {
            isLoading = true;
            ForceClaimAllBoosts(); // Ensure all boosts are claimed on login
            if (LocalPlayerAccountService.IsLocalAccount(Service.Get<PlayerDataService>().PlayerData.Account))
            {
                isLoading = false;
                if (this.OnLoadComplete != null)
                {
                    this.OnLoadComplete(this, null);
                }
                return;
            }

            loadPurchasesFromMWS(delegate (HashSet<string> registeredPurchases)
            {
                if (isMember)
                {
                    claimAllPurchases(registeredPurchases);
                }
                isLoading = false;
                if (this.OnLoadComplete != null)
                {
                    this.OnLoadComplete(this, null);
                }
            });
        }

        public void OnLogout()
        {
            clearPurchases();
            loadPurchasesFromDisk();
        }

        public void OnPurchase(IEnumerable<string> items)
        {
            addAllPurchases(items);
        }

        public void OnRestore(IEnumerable<string> items)
        {
            addAllPurchases(items);
        }

        private void ForceClaimAllBoosts()
        {
            Debug.Log("Force-claiming all boosts...");

            // Forcefully add all available boosts to the owned list
            foreach (BoostManager.AvailableBoosts boost in Enum.GetValues(typeof(BoostManager.AvailableBoosts)))
            {
                OwnedBoosts.Add(boost);
                Debug.Log($"Boost {boost} has been forcefully claimed.");
            }
        }

        private void claimAllPurchases(HashSet<string> registeredPurchases)
        {
            IList<string> list = new List<string>();
            foreach (BoostManager.AvailableBoosts value in EnumUtil.GetValues<BoostManager.AvailableBoosts>())
            {
                string sKUByBoost = getSKUByBoost(value);
                if (sKUByBoost != null && !registeredPurchases.Contains(sKUByBoost))
                {
                    list.Add(sKUByBoost);
                }
            }
            if (list.Count > 0)
            {
                Service.Get<IMWSClient>().ClaimProductsForMember(DMOAnalyticsSysInfo._getUniqueIdentifier(), StoreType.GOOGLE_PLAY, list, delegate { });
                addAllPurchases(list);
            }
        }

        private void loadPurchasesFromDisk()
        {
            SavedStorePurchasesCollection savedStorePurchasesCollection = new SavedStorePurchasesCollection();
            savedStorePurchasesCollection.LoadFromDisk();
            foreach (SavedStorePurchaseData purchasedItemsDatum in savedStorePurchasesCollection.PurchasedItemsData)
            {
                addPurchase(purchasedItemsDatum.sku);
            }
        }

        private void loadPurchasesFromMWS(Action<HashSet<string>> callback = null)
        {
            Service.Get<IMWSClient>().GetIAPPurchasesForPlayer(null, delegate (IGetIAPPurchasesResponse response)
            {
                HashSet<string> hashSet = new HashSet<string>();
                foreach (ProductPurchase product in response.Products)
                {
                    addPurchase(product.ProductId);
                    hashSet.Add(product.ProductId);
                }
                if (callback != null)
                {
                    callback(hashSet);
                }
            });
        }

        private void addAllPurchases(IEnumerable<string> skus)
        {
            foreach (string sku in skus)
            {
                addPurchase(sku);
            }
        }

        private void addPurchase(string sku)
        {
            BoostManager.AvailableBoosts? boostBySKU = getBoostBySKU(sku);
            if (!boostBySKU.HasValue)
            {
                UnityEngine.Debug.LogWarning("Attempting to add unknown purchased sku " + sku);
            }
            else
            {
                OwnedBoosts.Add(boostBySKU.Value);
            }
        }

        private void clearPurchases()
        {
            OwnedBoosts.Clear();
        }
    }
}
