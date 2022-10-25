using MultiMiami.Defs;

namespace MultiMiami.Maps
{
    public struct TileContainer
    {
        public TileDef Floor;
        public TileDef Wall;

        public readonly TileDef GetTile(TileLayer layer) => layer switch
        {
            TileLayer.Floor => Floor,
            TileLayer.Wall => Wall,
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, null)
        };

        public void SetTile(TileLayer layer, TileDef tile)
        {
            switch (layer)
            {
                case TileLayer.Floor:
                    Floor = tile;
                    return;
                case TileLayer.Wall:
                    Wall = tile;
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layer), layer, null);
            }
        }
    }
}
