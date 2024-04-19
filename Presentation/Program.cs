using Domain.Helpers;
using Domain.Interfaces;
using Domain.Models;
using Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using static Domain.Enums.ItemEnum;

namespace BJSS
{
    public class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            // Register a singleton to map item names to their respective prices
            services.AddSingleton<IDictionary<ItemName, decimal>>(new Dictionary<ItemName, decimal>
            {
                { ItemName.Soup, 0.65m },
                { ItemName.Bread, 0.80m },
                { ItemName.Milk, 1.30m },
                { ItemName.Apples, 1.00m }
            });

            // Register a singleton to map item names to their respective offers
            services.AddSingleton<IDictionary<ItemName, Func<IEnumerable<ItemName>, decimal>>>(new Dictionary<ItemName, Func<IEnumerable<ItemName>, decimal>>
            {
                {ItemName.Apples, OfferHelper.ApplyAppleDiscount},
                {ItemName.Soup, OfferHelper.ApplySoupOffer}
            });

            // Register a singleton for the PriceCalculatorService
            services.AddSingleton<IPriceCalculator, PriceCalculatorService>();

            var serviceProvider = services.BuildServiceProvider();
            var calculator = serviceProvider.GetRequiredService<IPriceCalculator>();

            bool exitRequested = false;

            do
            {
                Console.WriteLine("Please write 'PriceBasket' followed by the items in the basket separated by spaces (use 'list' to see all available items, and 'offers' to see current offers):");
                string input = Console.ReadLine();

                // If the user requests to see the list of items
                if (input.ToLower() == "list")
                {
                    DisplayAvailableItems(serviceProvider.GetRequiredService<IServiceProvider>());
                    continue;
                }

                // If the user requests to see the current offers
                if (input.ToLower() == "offers")
                {
                    DisplayActiveOffers(serviceProvider.GetRequiredService<IDictionary<ItemName, Func<IEnumerable<ItemName>, decimal>>>());
                    continue;
                }

                // Check if the input starts with "PriceBasket"
                if (!input.ToLowerInvariant().StartsWith("pricebasket"))
                {
                    Console.WriteLine("Please write 'PriceBasket' to add items.");
                    continue;
                }

                // Extract items from the input after removing "PriceBasket"
                input = input.Substring("PriceBasket".Length).Trim();

                if (input == null)
                {
                    Console.WriteLine("Invalid input. Please enter valid items, 'list', or 'offers'.");
                    continue;
                }

                List<ItemName> items = new List<ItemName>();

                // Parse the items into the list
                foreach (string itemName in input.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    // If the input is a valid item and not numeric, add it to the list
                    if (Enum.TryParse(itemName, true, out ItemName item) && !int.TryParse(itemName, out _))
                    {                   
                        items.Add(item);
                    }
                    else
                    {
                        // If the input is either not a valid item or numeric, display an error message
                        Console.WriteLine($"Invalid item: {itemName}");
                    }
                }

                if (items.Count == 0)
                {
                    Console.WriteLine("No valid items or commands were found in your input, please validate your input.");
                }
                else
                {
                    // Get subtotal, total and offersApplied from the service
                    PriceCalculationModel priceCalculationResult = calculator.CalculateTotalPrice(items);

                    // Display messages to the user in console
                    Console.WriteLine($"Subtotal: £{priceCalculationResult.Subtotal:F2}");
                    Console.Write(priceCalculationResult.OffersApplied);
                    Console.WriteLine($"Total: £{priceCalculationResult.Total:F2}");
                }

                Console.WriteLine("Press ESC to exit or any other key to continue adding items.");

                // Check if the user pressed the Escape key to exit the application
                if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    exitRequested = true;
                }
            } while (!exitRequested);

        }

        // Displays the list of available items
        private static void DisplayAvailableItems(IServiceProvider serviceProvider)
        {
            var pricesDictionary = serviceProvider.GetRequiredService<IDictionary<ItemName, decimal>>();
            Console.WriteLine("Available items:");

            // Lists available items with respective prices and name
            foreach (var kvp in pricesDictionary)
            {
                Console.WriteLine($"{kvp.Key} : £{kvp.Value:F2}");
            }
        }

        // Displays the list of active offers
        private static void DisplayActiveOffers(IDictionary<ItemName, Func<IEnumerable<ItemName>, decimal>> offersDictionary)
        {
            Console.WriteLine("Current special offers are:");

            foreach (var kvp in offersDictionary)
            {
                string offerDescription = GetOfferDescription(kvp.Key);
                Console.WriteLine($"- {offerDescription}");
            }
        }

        // Gets the description of the offer
        private static string GetOfferDescription(ItemName itemName)
        {
            switch (itemName)
            {
                case ItemName.Apples:
                    return "Apples have 10% off their normal price this week";
                case ItemName.Soup:
                    return "Buy 2 tins of soup and get a loaf of bread for half price";
                default:
                    return "";
            }
        }

    }
}
