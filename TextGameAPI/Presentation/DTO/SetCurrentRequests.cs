namespace TextGame.Presentation.DTO
{
    /// <summary>
    /// Тело для «сделать текущим»: PUT /sessions/current и PUT /saves/current.
    /// </summary>
    public record SetCurrentRequest(Guid Id);

    /// <summary>
    /// Тело для перехода в открытую комнату: PUT /state/rooms/current.
    /// </summary>
    public record SetCurrentRoomRequest(int Id);
}
