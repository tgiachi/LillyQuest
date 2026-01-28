namespace LillyQuest.Engine.Screens.UI;

public sealed record MenuItem(string Text, Action OnSelect, bool IsEnabled = true);
