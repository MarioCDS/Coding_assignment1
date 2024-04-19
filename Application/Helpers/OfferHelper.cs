using System.Text;
using static Domain.Enums.ItemEnum;

namespace Domain.Helpers
{
    public static class OfferHelper
    {
        // Applies the offer: 10% discount on the total price of apples
        public static decimal ApplyAppleDiscount(IEnumerable<ItemName> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            decimal discount = 0;
            int appleCount = items.Count(item => item == ItemName.Apples);

            // Calculate discount based on the number of apples purchased
            discount = appleCount * 0.1m;

            return discount;
        }

        // Applies the offer: Buy 2 tins of soup and get a loaf of bread for half of the price
        public static decimal ApplySoupOffer(IEnumerable<ItemName> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            decimal discount = 0;
            int soupCount = items.Count(item => item == ItemName.Soup);
            int breadCount = items.Count(item => item == ItemName.Bread);

            // Check if ther oder is eligible for the discount
            if (soupCount >= 2 && breadCount > 0)
            {
                int maxDiscountedBreads = soupCount / 2; // Maximum number of discounted breads based on soup count
                int actualDiscountedBreads = Math.Min(maxDiscountedBreads, breadCount);
                discount = 0.4m * actualDiscountedBreads; // Calculate discount based on amount of discounted breads
            }

            return discount;
        }

        //Checks if any offers should be applied and calculates discount
        public static decimal CalculateOffersDiscount(IDictionary<ItemName, Func<IEnumerable<ItemName>, decimal>> specialOffers, IEnumerable<ItemName> items)
        {
            decimal appleDiscount = specialOffers.ContainsKey(ItemName.Apples) ? ApplyOffer(ItemName.Apples, items, specialOffers) : 0;
            decimal soupOffer = specialOffers.ContainsKey(ItemName.Soup) ? ApplyOffer(ItemName.Soup, items, specialOffers) : 0;

            return -(appleDiscount + soupOffer);
        }

        //Applies a specific offer to a specific set of items
        public static decimal ApplyOffer(ItemName offerName, IEnumerable<ItemName> items, IDictionary<ItemName, Func<IEnumerable<ItemName>, decimal>> specialOffers)
        {
            if (specialOffers.TryGetValue(offerName, out Func<IEnumerable<ItemName>, decimal> offerFunction))
                return offerFunction(items);

            return 0;
        }

        //Create the description for the offers that were applied if any
        public static string GenerateOfferDescription(IDictionary<ItemName, Func<IEnumerable<ItemName>, decimal>> specialOffers, IEnumerable<ItemName> items)
        {
            var description = new StringBuilder();

            // Flags to check if any offers were applied
            bool appleOfferApplied = specialOffers.ContainsKey(ItemName.Apples) && ApplyOffer(ItemName.Apples, items, specialOffers) > 0;
            bool soupOfferApplied = specialOffers.ContainsKey(ItemName.Soup) && ApplyOffer(ItemName.Soup, items, specialOffers) > 0;

            // If offers were applied adds message to show user
            if (appleOfferApplied)
                description.AppendLine($"Apples 10% off: {FormatDiscount(ApplyOffer(ItemName.Apples, items, specialOffers))}");

            if (soupOfferApplied)
                description.AppendLine($"Buy 2 tins of soup and get a loaf of bread for half price: {FormatDiscount(ApplyOffer(ItemName.Soup, items, specialOffers))}");

            // If no offers were applied, append the message
            if (!appleOfferApplied && !soupOfferApplied)
                description.AppendLine("(no offers available)");

            return description.ToString();
        }

        //Formats the discount amount either in pounds or pence
        public static string FormatDiscount(decimal discount)
        {
            if (discount >= 1)
                return $"-{discount:0.00}£";
            else
                return $"-{discount * 100:F0}p";
        }
    }
}
