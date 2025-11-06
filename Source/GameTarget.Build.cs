using Flax.Build;

public class GameTarget : GameProjectTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for game
        Modules.Add(nameof(FlaxSaveExamples));
        Modules.Add(nameof(FlaxSave));
                Modules.Add("Game");
    }
}
