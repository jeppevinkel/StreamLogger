using StreamLogger.Api.EventArgs;

namespace OpenVRNotificationPipeIntegration.EventHandlers
{
    public static class GameChangeEventHandler
    {
        public static void OnGameChangeEvent(GameChangeEventArgs e)
        {
            Main.Instance.CurrentGame = e.GameChange.GameId == 0 ? null : $"{e.GameChange.GameId} - {e.GameChange.GameName}";

            Main.Instance.StyleManager.NotificationStyles = new NotificationStyles();
            Main.Instance.StyleManager.LoadStyles();
        }
    }
}