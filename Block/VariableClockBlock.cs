using csvorbis;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;
using Vintagestory.ServerMods;

namespace DecoClock
{
    public abstract class VariableClockBlock : ClockBlock, ITexPositionSource
    {
        public Size2i AtlasSize { get; set; } = null!;
        string[] materials = null!;
        public string Material { get; private set; } = null!;
        public abstract string Key { get; }


        public TextureAtlasPosition? this[string textureCode]
        {
            get
            {
                var capi = (ICoreClientAPI)api;
                ITexPositionSource TexturePositionSource = capi.Tesselator.GetTextureSource(this, 0, true);

                if (textureCode == "frame")
                {
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
                            capi.World.Logger.Warning("For render in block " + Code +
                                ", no such texture found.", texturePath);
                        }

                        pos ??= capi.BlockTextureAtlas.UnknownTexturePosition;
                    }

                    return pos ??= capi.BlockTextureAtlas.UnknownTexturePosition;
                }

                return TexturePositionSource["textureCode"] ?? capi.BlockTextureAtlas.UnknownTexturePosition;

            }
        }

        public MeshData GenMesh(string typeForLogging)
        {
            ICoreClientAPI capi = (ICoreClientAPI)api;
            string PathShape = $"decoclock:shapes/block/{Key}/frame";
            Shape shape = capi.Assets.TryGet($"{PathShape}.json").ToObject<Shape>();

            capi.Tesselator.TesselateShape(typeForLogging, shape, out MeshData mesh, this);

            return mesh;
        }

        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            string material = itemstack?.Attributes.GetString("material") ?? "oak";

            renderinfo.ModelRef = ObjectCacheUtil.GetOrCreate(capi, material + Key, () =>
            {
                AtlasSize = capi.BlockTextureAtlas.Size;
                this.Material = material;
                MeshData mesh = GenMesh($"{Core.ModId}:Inventory{Key}");

                return capi.Render.UploadMultiTextureMesh(mesh);
            });

        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(world, blockPos, byItemStack);
            //BEClock? be = (BEClock)world.BlockAccessor.GetBlockEntity(blockPos);
            //if (be != null && byItemStack != null)
            //{
            //    be.Material = byItemStack.Attributes.GetString("material", "oak");

            //    if (world.Side == EnumAppSide.Client)
            //    {

            //        be.UpdateMesh();
            //    }

            //    be.MarkDirty(redrawOnClient: true);
            //}
        }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            LoadTypes();
        }

        private void LoadTypes()
        {
            var grp = Attributes["material"].AsObject<RegistryObjectVariantGroup>();

            if (grp.LoadFromProperties != null)
            {
                var prop = api.Assets.TryGet(grp.LoadFromProperties
                    .WithPathPrefixOnce("worldproperties/")
                    .WithPathAppendixOnce(".json"))?
                    .ToObject<StandardWorldProperty>();
                materials = prop.Variants.Select(p => p.Code.Path).ToArray();
            }

            List<JsonItemStack> stacks = [];

            foreach (var material in materials)
            {
                var jstack = new JsonItemStack()
                {
                    Code = this.Code,
                    Type = EnumItemClass.Block,
                    Attributes = new JsonObject(JToken.Parse("{ \"material\": \"" + material + "\" }"))
                };

                jstack.Resolve(api.World, Code + " type");
                stacks.Add(jstack);
            }

            CreateCreativeInventoryStacks(stacks);

        }

        public virtual void CreateCreativeInventoryStacks(List<JsonItemStack> stacks)
        {
            this.CreativeInventoryStacks =
            [
                new CreativeTabAndStackList() { Stacks = stacks.ToArray(), Tabs = ["general", "decorative", "decoclock"] }
            ];
        }

    }
}
