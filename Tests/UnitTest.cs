using Domain.Helpers;
using Domain.Interfaces;
using Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using static Domain.Enums.ItemEnum;

namespace BJSS.Tests
{
    [TestFixture]
    public class PriceCalculatorTests
    {
        private readonly IPriceCalculator _priceCalculator;
        private readonly IDictionary<ItemName, decimal> _prices;
        private readonly IDictionary<ItemName, Func<IEnumerable<ItemName>, decimal>> _specialOffers;

        public PriceCalculatorTests()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IDictionary<ItemName, decimal>>(new Dictionary<ItemName, decimal>
            {
                {ItemName.Soup, 0.65m},
                {ItemName.Bread, 0.80m},
                {ItemName.Milk, 1.30m},
                {ItemName.Apples, 1.00m}
            });

            services.AddSingleton<IDictionary<ItemName, Func<IEnumerable<ItemName>, decimal>>>(new Dictionary<ItemName, Func<IEnumerable<ItemName>, decimal>>
            {
                {ItemName.Apples, OfferHelper.ApplyAppleDiscount},
                {ItemName.Soup, OfferHelper.ApplySoupOffer}
            });

            services.AddSingleton<IPriceCalculator, PriceCalculatorService>();

            var serviceProvider = services.BuildServiceProvider();

            _priceCalculator = serviceProvider.GetRequiredService<IPriceCalculator>();
            _prices = serviceProvider.GetRequiredService<IDictionary<ItemName, decimal>>();
            _specialOffers = serviceProvider.GetRequiredService<IDictionary<ItemName, Func<IEnumerable<ItemName>, decimal>>>();
        }

        //Testing null items with apple discount case
        [Test]
        public void ApplyAppleDiscount_NullItems_ThrowsArgumentNullException()
        {
            // Arrange
            List<ItemName> items = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() => OfferHelper.ApplyAppleDiscount(items));
        }

        //Testing null items with Soup discount case
        [Test]
        public void ApplySoupOffer_NullItems_ThrowsArgumentNullException()
        {
            // Arrange
            List<ItemName> items = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() => OfferHelper.ApplySoupOffer(items));
        }

        //Testing apple discount with no apples case
        [Test]
        public void ApplyAppleDiscount_NoApples_ReturnsZeroDiscount()
        {
            // Arrange
            List<ItemName> items = new List<ItemName> { ItemName.Bread, ItemName.Soup, ItemName.Milk };

            // Act
            decimal discount = OfferHelper.ApplyAppleDiscount(items);

            // Assert
            Assert.That(discount, Is.EqualTo(0));
        }


        //Testing soup discount with not enough soups case
        [Test]
        public void ApplySoupOffer_LessThanTwoSoups_ReturnsZeroDiscount()
        {
            // Arrange
            List<ItemName> items = new List<ItemName> { ItemName.Soup, ItemName.Bread };

            // Act
            decimal discount = OfferHelper.ApplySoupOffer(items);

            // Assert
            Assert.That(discount, Is.EqualTo(0));
        }

        //Testing soup discount with enough soups and bread case
        [Test]
        public void ApplySoupOffer_TwoSoupsAndBread_ReturnsCorrectDiscount()
        {
            // Arrange
            List<ItemName> items = new List<ItemName> { ItemName.Soup, ItemName.Soup, ItemName.Bread };

            // Act
            decimal discount = OfferHelper.ApplySoupOffer(items);

            // Assert
            Assert.That(discount, Is.EqualTo(0.4m));
        }

        //Testing apple discount with three apples
        [Test]
        public void ApplyAppleDiscount_ThreeApples_ReturnsCorrectDiscount()
        {
            // Arrange
            List<ItemName> items = new List<ItemName> { ItemName.Apples, ItemName.Apples, ItemName.Apples };

            // Act
            decimal discount = OfferHelper.ApplyAppleDiscount(items);

            // Assert
            Assert.That(discount, Is.EqualTo(0.3m));
        }

        //Testing price with all types of items and discounts
        [Test]
        public void CalculateTotalPrice_AllItemsWithDiscounts_ReturnsCorrectTotal()
        {
            // Arrange
            var items = new List<ItemName>
            {
                ItemName.Soup,
                ItemName.Soup,
                ItemName.Bread,
                ItemName.Milk,
                ItemName.Milk,
                ItemName.Milk,
                ItemName.Apples,
                ItemName.Apples,
                ItemName.Apples,
                ItemName.Apples
            };

            var expectedSubtotal = 2 * _prices[ItemName.Soup] +
                                   _prices[ItemName.Bread] +
                                   3 * _prices[ItemName.Milk] +
                                   4 * _prices[ItemName.Apples];
            var expectedAppleDiscount = 0.4m; // 4 apples with a 10% discount each
            var expectedSoupDiscount = 0.4m;  // 2 soups with a half price bread offer

            // Act
            var result = _priceCalculator.CalculateTotalPrice(items);

            // Assert
            Assert.That(result.Subtotal, Is.EqualTo(expectedSubtotal));
            Assert.That(result.OffersApplied, Contains.Substring($"Apples 10% off: {OfferHelper.FormatDiscount(expectedAppleDiscount)}"));
            Assert.That(result.OffersApplied, Contains.Substring($"Buy 2 tins of soup and get a loaf of bread for half price: {OfferHelper.FormatDiscount(expectedSoupDiscount)}"));
            Assert.That(result.Total, Is.EqualTo(expectedSubtotal - expectedAppleDiscount - expectedSoupDiscount));
        }
    }
}
