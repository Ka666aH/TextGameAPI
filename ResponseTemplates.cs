namespace TextGame
{
    public record SuccessfulResponse(object? data);
    public record ErrorResponse(string? code, string? message);
    public record GameOverResponse(string? message, GameOverStatsDTO? stats);
    public static class ErrorCodes
    {
        public static readonly string NotFound = "NOT_FOUND";

        public static readonly string UnstartedGameError = "NOT_STARTED";
        public static readonly string CurrentRoomError = "CURRENT_ROOM_NOT_RECEIVED";
        public static readonly string InventoryError = "INVENTORY_NOT_RECEIVED";
        public static readonly string CoinsError = "COINS_NOT_RECEIVED";
        public static readonly string NextRoomError = "NEXT_ROOM_ERROR";
        public static readonly string SearchError = "SEARCH_ERROR";
        public static readonly string TakeItemError = "TAKE_ITEM_ERROR";
        public static readonly string UncarryableError = "UNCARRYABLE_ERROR";
        public static readonly string EmptyError = "EMPTY_ERROR";
        public static readonly string TakeItemsError = "TAKE_ITEMS_ERROR";
        public static readonly string NotChestError = "NOT_CHEST_ERROR";
        public static readonly string NoKeyError = "NO_KEY_ERROR";
    }
}
