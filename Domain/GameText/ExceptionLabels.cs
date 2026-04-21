namespace TextGame.Domain.GameText
{
    public static class ExceptionLabels
    {
        public const string BattleWinCode = "YOU_WIN_IN_BATTLE";
        public const string DefeatCode = "DEFEAT";

        public const string ClosedCode = "CLOSED";
        public const string ClosedText = "Сундук закрыт!";

        public const string EmptyCode = "EMPTY_ERROR";
        public const string EmptyText = "Тут ничего нет!";

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

        public const string NullEnemyIdCode = "ENEMY_NOT_FOUND";
        public const string NullEnemyIdText = "Противник не найден.";

        public const string NullItemIdCode = "ITEM_NOT_FOUND";
        public const string NullItemIdText = "Предмет не найден.";

        public const string NullRoomIdCode = "ROOM_NOT_FOUND";
        public const string NullRoomIdText = "Комната не найдена.";

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
        public const string NotEqiipmentCode = "NOT_EQUIPMENT";
        public const string NotEqiipmentText = "Это не снаряжение.";

        public const string EnemyDefeated = "{enemyName} повержен.";
        public const string SuicideText = "Вы погибли от своей же атаки. Как отчаянно.";
        public const string PlayerDefeated = "Вы были повержены {enemyName}ОМ.";
        public const string PlayerPoisoned = "{healName} приводит Вас к гибели!";
        public const string PlayerEaten = "НА ВАС НАПАЛ МИМИК! ВЫ БЫЛИ ПРОГЛОЧЕНЫ И ПЕРЕВАРЕНЫ!";

        public const string InternalServerErrorCode = "INTERNAL_SERVER_ERROR";
        public const string InternalServerErrorMessage = "Непредвиденная ошибка сервера.";
    }
}
