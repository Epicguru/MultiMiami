using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MM.Core;

/// <summary>
/// Represents the inputs of the <see cref="SpriteBatch.Begin"/>
/// method.
/// </summary>
public readonly struct SpriteBatchArgs
{
    public SpriteSortMode SortMode { get; init; } = SpriteSortMode.Deferred;
    public BlendState BlendState { get; init; } = null;
    public SamplerState SamplerState { get; init; } = null;
    public DepthStencilState DepthStencilState { get; init; } = null;
    public RasterizerState RasterizerState { get; init; } = null;
    public Effect Effect { get; init; } = null;
    public Matrix? Matrix { get; init; } = null;

    public SpriteBatchArgs() { }
}
