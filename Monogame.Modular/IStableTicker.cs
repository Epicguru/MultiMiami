using Microsoft.Xna.Framework;

namespace MM.Core;

/// <summary>
/// When a <see cref="Game"/> class implements this interface and the <see cref="Screen"/>
/// class is initialized, the <see cref="StableTick"/> method will be invoked exactly <see cref="TargetTickRate"/>
/// times per second, regardless of framerate.
/// </summary>
public interface IStableTicker
{
    int TargetTickRate { get; }

    void StableTick();
}
