namespace Themes
{
    public interface IThemeReceiver
    {
        public void ReceiveThemeOnGameStart(Theme theme);
        
        public void ReceiveTheme(Theme theme);
    }
}