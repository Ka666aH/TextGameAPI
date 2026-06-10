using TextGame.Infrastructure.Token;

namespace TextGame.Domain.GameText
{
    public static class ExceptionsLabels
    {
        public const string UserNotFoundCode = "USER_NOT_FOUND";
        public const string UserNotFoundMessage = "Пользователь не найден.";

        public const string SessionNotFoundCode = "SESSION_NOT_FOUND";
        public const string SessionNotFoundMessage = "Игровая сессия не найдена.";

        public const string SaveNotFoundCode = "SAVE_NOT_FOUND";
        public const string SaveNotFoundMessage = "Сохранение не найдено.";

        public const string ImpossibleDeleteSaveCode = "IMPOSSIBLE_DELETE_SAVE";
        public const string ImpossibleDeleteSaveMessage = "Невозможно удалить сохранение.";

        public const string NotSessionOwnerCode = "SESSION_BELONGS_TO_ANOTHER_USER";
        public const string NotSessionOwnerMessage = "Игровая сессия принадлежит другому пользователю.";

        public const string NotSaveOwnerCode = "SAVE_BELONGS_TO_ANOTHER_USER";
        public const string NotSaveOwnerMessage = "Сохранение принадлежит другому пользователю.";

        public const string IncorrectPasswordCode = "INCORRECT_PASSWORD";
        public const string IncorrectPasswordMessage = "Неверный пароль.";

        public const string AccessTokenMissingCode = "ACCESS_TOKEN_NOT_FOUND";
        public const string AccessTokenMissingMessage = "Токен доступа не найден.";

        public const string MissingUserIdClaimCode = "MISSING_USER_ID_CLAIM";
        public const string MissingUserIdClaimMessage = $"Токен доступа не содержит клейм {AccessClaims.UserId}.";

        public const string MissingSessionIdClaimCode = "MISSING_SESSION_ID_CLAIM";
        public const string MissingSessionIdClaimMessage = $"Токен доступа не содержит клейм {AccessClaims.SessionId}.";

        public const string RefreshTokenMissingCode = "REFRESH_TOKEN_NOT_FOUND";
        public const string RefreshTokenMissingMessage = "Токен обновления не найден.";

        public const string RefreshTokenExpiredCode = "REFRESH_TOKEN_EXPIRED";
        public const string RefreshTokenExpiredMessage = "Токен обновления просрочен.";

        public const string RefreshTokenCompromisedCode = "REFRESH_TOKEN_COMPROMISED";
        public const string RefreshTokenCompromisedMessage = "Токен обновления скомпрометирован.";

        public const string BattleWinCode = "YOU_WIN_IN_BATTLE";
        public const string DefeatCode = "DEFEAT";

        public const string ClosedCode = "CLOSED";
        public const string ClosedText = "Сундук закрыт!";

        public const string NothingFoundCode = "NOTHING_FOUND";
        public const string NothingFoundMessage = "Тут ничего нет!";

        public const string ImpossibleStealCode = "CAN_NOT_STEAL";
        public const string ImpossibleStealText = "Невозможно украсть. За Вами следят.";

        public const string InBattleCode = "IN_BATTLE";
        public const string InBattleText = "В бою!";

        public const string LockedCode = "LOCKED";
        public const string LockedText = "Сундук заперт!";

        public const string NoKeyCode = "NO_KEY_ERROR";
        public const string NoKeyText = "Нет ключа!";

        public const string NoMapCode = "NO_MAP_ERROR";
        public const string NoMapText = "Нет карты!";

        public const string NoMoneyCode = "NO_MONEY";
        public const string NoMoneyText = "Недостаточно средств!";

        public const string NotShopCode = "NOT_IN_SHOP";
        public const string NotShopText = "Невозможно вне магазина!";

        public const string EnemyNotFoundCode = "ENEMY_NOT_FOUND";
        public const string EnemyNotFoundMessage = "Противник не найден.";

        public const string ItemNotFoundCode = "ITEM_NOT_FOUND";
        public const string ItemNotFoundMessage = "Предмет не найден.";

        public const string RoomNotFoundCode = "ROOM_NOT_FOUND";
        public const string RoomNotFoundMessage = "Комната не найдена.";

        public const string UncarryableCode = "UNCARRYABLE_ERROR";
        public const string UncarryableText = "Невозможно поднять этот предмет!";

        public const string UndiscoveredRoomCode = "UNDISCOVERED_ROOM_ERROR";
        public const string UndiscoveredRoomText = "Комната ещё не открыта.";

        public const string UnsearchedRoomCode = "ROOM_NOT_SEARCHED";
        public const string UnsearchedRoomText = "Комната ещё не обыскана!";

        public const string UnsellableItemCode = "UNSELLABLE_ERROR";
        public const string UnsellableItemText = "Невозможно продать этот предмет!";

        public const string UnstartedGameCode = "NOT_STARTED";
        public const string UnstartedGameText = "Игра ещё не начата!";

        public const string WinCode = "WIN";
        public const string WinText = "Вы нашли выход и выбрались наружу.";

        public const string NotChestCode = "NOT_CHEST";
        public const string NotChestText = "Это не сундук.";
        public const string NotHealCode = "NOT_HEAL";
        public const string NotHealText = "Это не предмет лечения.";
        public const string NotEquipmentCode = "NOT_EQUIPMENT";
        public const string NotEquipmentText = "Это не снаряжение.";

        public const string EnemyDefeated = "{0} повержен.";
        public const string PlayerSuicideText = "Вы погибли от своей же атаки. Как отчаянно.";
        public const string EnemySuicideText = "{0} погиб от своей же атаки. Как глупо.";
        public const string PlayerDefeated = "Вы были повержены {0}ОМ.";
        public const string PlayerPoisoned = "{0} приводит Вас к гибели!";
        public const string PlayerEaten = "НА ВАС НАПАЛ МИМИК! ВЫ БЫЛИ ПРОГЛОЧЕНЫ И ПЕРЕВАРЕНЫ!";

        public const string ValidationErrorCode = "VALIDATION_ERROR";
        public const string ValidationErrorMessage = "Одна или несколько ошибок валидации.";

        public const string InternalServerErrorCode = "INTERNAL_SERVER_ERROR";
        public const string InternalServerErrorMessage = "Непредвиденная ошибка сервера.";
    }
}