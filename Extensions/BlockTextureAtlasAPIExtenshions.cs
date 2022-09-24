using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace DecoClock
{
    public static class BlockTextureAtlasAPIExtensions
    {
        public static TextureAtlasPosition GetPosition(this IBlockTextureAtlasAPI atlas, Block block, string textureName = null, bool returnNullWhenMissing = false)
        {
            return GetPosition((BlockTextureAtlasManager)atlas, block, textureName, returnNullWhenMissing);
        }

        public static TextureAtlasPosition GetPosition(this BlockTextureAtlasManager atlas, Block block, string textureName = null, bool returnNullWhenMissing = false)
        {
            if (block.Shape == null || block.Shape.VoxelizeTexture)
            {
                CompositeTexture texture = block.FirstTextureInventory;
                CompositeShape shape = block.Shape;
                if (shape?.Base != null &&
                    !block.Textures.TryGetValue(block.Shape.Base.Path.ToString(), out texture))
                {
                    texture = block.FirstTextureInventory;
                }
                int textureSubId = texture.Baked.TextureSubId;
                return atlas.TextureAtlasPositionsByTextureSubId[textureSubId];
            }
            return atlas.GetPosition(block, textureName, returnNullWhenMissing);
        }
    }
}
