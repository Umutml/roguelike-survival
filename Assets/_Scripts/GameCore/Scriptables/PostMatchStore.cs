using System.Collections.Generic;
using Cathei.LinqGen;
using Unity.Mathematics.Geometry;
using UnityEngine;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "PostMatchStore", menuName = "ScriptableObjects/PostMatchStore")]
    public class PostMatchStore : ScriptableObject
    {
        public List<PostMatchItem> postMatchItems;

        public List<(PostMatchItem, PurchaseDetails)> GetRandomPurchaseItems(int count, int podCount)
        {
            return postMatchItems.Gen().OrderBy(_ => Random.value)
                .Select((item, index) => (item, GenerateRandomPurchaseDetails(index, podCount))).Take(count).ToList();
        }

        public PurchaseDetails GenerateRandomPurchaseDetails(int index, int podCount)
        {
            var randomAmount = Random.Range(Mathf.Max(-podCount * 1.5f, 10), Mathf.Max(podCount, 10));
            var purchaseOption = index == 0 ? PurchaseOptions.Gem : PurchaseOptions.XpPod;

            return new PurchaseDetails((int) randomAmount, purchaseOption);
        }
    }

    public record PurchaseDetails(int Price, PurchaseOptions PurchaseOption)
    {
        public int Price { get; } = Price;
        public PurchaseOptions PurchaseOption { get; } = PurchaseOption;
    }
}
