using LillyQuest.Core.Types;

namespace LillyQuest.Core.Data.Configs.Sections;

public record EngineKeyBinding(
    string ActionName,
    string KeyCombination,
    ShortcutTriggerType TriggerType = ShortcutTriggerType.Press
);
