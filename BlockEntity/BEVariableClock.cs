using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace DecoClock
{
    public abstract class BEVariableClock:BEClock
    {
        public string Material { get; set; } = null!;

        public override TextureAtlasPosition? this[string textureCode]
        {
            get
            {
                ItemStack? stack = Inventory.TryGetPart(textureCode);

                if (stack is not null)
                {
                    var capi = (ICoreClientAPI)Api;
                    if (stack.Class == EnumItemClass.Item)
                    {
                        var tex = stack.Item.FirstTexture;
                        AssetLocation texturePath = tex.Baked.BakedName;
                        return GetOrCreateTexPos(texturePath, capi);
                    }
                    else
                    {
                        var tex = stack.Block.FirstTextureInventory;
                        AssetLocation texturePath = tex.Baked.BakedName;
                        return GetOrCreateTexPos(texturePath, capi);
                    }
                }

                if (textureCode == "frame")
                {
                    var capi = (ICoreClientAPI)Api;
                    var texturePath = new AssetLocation($"block/wood/debarked/{Material}");
                    TextureAtlasPosition? pos = capi.BlockTextureAtlas[texturePath];

                    if (pos == null)
                    {
                        IAsset texAsset = capi.Assets.TryGet(texturePath.Clone().
                                                WithPathPrefixOnce("textures/").
                                                WithPathAppendixOnce(".png"));
                        if (texAsset != null)
                        {
                            capi.BlockTextureAtlas.GetOrInsertTexture(texturePath, out _, out pos,
                                () => texAsset.ToBitmap(capi));
                        }
                        else
                        {
                            capi.World.Logger.Warning("For render in block " + this.Block.Code +
                                ", no such texture found.", texturePath);
                        }

                        pos ??= capi.BlockTextureAtlas.UnknownTexturePosition;
                    }

                    return pos ??= capi.BlockTextureAtlas.UnknownTexturePosition;
                }

                return TextureSource[textureCode];
            }
        }

        public override void GetVariablesFromTreeAttributes(ITreeAttribute tree)
        {
            base.GetVariablesFromTreeAttributes(tree);
            Material = tree.GetString("material", Material);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetString("material", Material);
        }

    }
}
