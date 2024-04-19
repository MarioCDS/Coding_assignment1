using Domain.Helpers;
using Domain.Interfaces;
using Domain.Models;
using static Domain.Enums.ItemEnum;

namespace Domain.Services
{
    public class PriceCalculatorService : IPriceCalculator
    {
        private readonly IDictionary<ItemName, decimal> _prices;
        private readonly IDictionary<ItemName, Func<IEnumerable<ItemName>, decimal>> _specialOffers;

        public PriceCalculatorService(IDictionary<ItemName, decimal> prices, IDictionary<ItemName, Func<IEnumerable<ItemName>, decimal>> specialOffers)
        {
            _prices = prices ?? throw new ArgumentNullException(nameof(prices));
            _specialOffers = specialOffers ?? throw new ArgumentNullException(nameof(specialOffers));
        }

        //Calculates and returns the subtotal, total and offersApplied
        public PriceCalculationModel CalculateTotalPrice(IEnumerable<ItemName> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            PriceCalculationModel priceCalculationResult = new PriceCalculationModel();

            //Calculate all fields to be returned through the Model
            priceCalculationResult.Subtotal = items.Sum(item => _prices.ContainsKey(item) ? _prices[item] : 0);
            priceCalculationResult.Total = priceCalculationResult.Subtotal + OfferHelper.CalculateOffersDiscount(_specialOffers, items);
            priceCalculationResult.OffersApplied = OfferHelper.GenerateOfferDescription(_specialOffers, items);

            return priceCalculationResult;
        }
    }
}
