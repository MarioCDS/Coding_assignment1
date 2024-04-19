using Domain.Models;
using static Domain.Enums.ItemEnum;

namespace Domain.Interfaces
{
    /// <summary>
    /// Represents a service for calculating the total price of a list of items, including any applied offers.
    /// </summary>
    public interface IPriceCalculator
    {
        /// <summary>
        /// Calculates the total price of the given list of items.
        /// </summary>
        /// <param name="items">The list of items.</param>
        /// <returns>A <see cref="PriceCalculationModel"/> containing subtotal, total, and offers applied.</returns>
        PriceCalculationModel CalculateTotalPrice(IEnumerable<ItemName> items);
    }
}
