namespace Quivi.Pos.Api.Dtos.Requests.MenuItems
{
    public class UpdateMenuItemStockRequest
    {
        public required IReadOnlyDictionary<string, bool> StockMap { get; init; }
    }
}