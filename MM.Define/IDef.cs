namespace MM.Define;

public interface IDef
{
    /// <summary>
    /// The unique ID of this def.
    /// </summary>
    string ID { get; set; }

    /// <summary>
    /// Called once after all defs have been loaded.
    /// Called <b>before </b> <see cref="LatePostLoad"/> and <see cref="ConfigErrors"/>.
    /// </summary>
    void PostLoad();

    /// <summary>
    /// Called once after all defs have been loaded and had <see cref="PostLoad"/> called on them.
    /// </summary>
    void LatePostLoad();

    /// <summary>
    /// Called once after <see cref="PostLoad"/> and <see cref="LatePostLoad"/>.
    /// Should be used to check for errors in the def.
    /// </summary>
    void ConfigErrors(ConfigErrorReporter config);
}