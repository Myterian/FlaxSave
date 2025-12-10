using Flax.Build;

public class FlaxSaveTarget : GameProjectTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for game
        Modules.Add("FlaxSave");
        Modules.Add("FlaxSaveExamples");
    }
}
