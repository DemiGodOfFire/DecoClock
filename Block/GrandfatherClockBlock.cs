using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.ServerMods;

namespace DecoClock
{
    public class GrandfatherClockBlock : ClockBlock, ITexPositionSource
    {
        public Size2i AtlasSize { get; set; } = null!;
        string[] materials = null!;
        string material;
        string pathBlock => "decoclock:shapes/block/grandfatherclock/frame";

        public TextureAtlasPosition? this[string textureCode]
        {
            get
            {
                var capi = (ICoreClientAPI)api;
                ITexPositionSource TexturePositionSource = capi.Tesselator.GetTextureSource(this, 0, true);

                if(textureCode == "frame")
                {
                    var texturePath = new AssetLocation($"block/wood/debarked/{material}");
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

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            LoadTypes();
        }

        private void LoadTypes()
        {
            var grp = Attributes["material"].AsObject<RegistryObjectVariantGroup>();
            //materials = grp.States;

            if (grp.LoadFromProperties != null)
            {
                var prop = api.Assets.TryGet(grp.LoadFromProperties.WithPathPrefixOnce("worldproperties/").WithPathAppendixOnce(".json"))?.ToObject<StandardWorldProperty>();
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

            this.CreativeInventoryStacks =
            [
                new CreativeTabAndStackList() { Stacks = [.. stacks], Tabs = ["general", "decorative", "decoclock"] }
            ];

        }

        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            string material = itemstack?.Attributes.GetString("material") ?? "oak";

            renderinfo.ModelRef = ObjectCacheUtil.GetOrCreate(capi, material, () =>
            {
                AtlasSize = capi.BlockTextureAtlas.Size;
                this.material = material;
                MeshData mesh = GenMesh(material, $"{Core.ModId}:InventoryClock");

                return capi.Render.UploadMultiTextureMesh(mesh);
            });

        }

        public MeshData GenMesh(string material, string typeForLogging)
        {
            ICoreClientAPI capi = (ICoreClientAPI)api;
            Shape shape = capi.Assets.TryGet($"{pathBlock}.json").ToObject<Shape>();

            capi.Tesselator.TesselateShape(typeForLogging, shape, out MeshData mesh, this);

            return mesh;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (!world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak)) { return false; }
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEGrandfatherClock be)
            {
                return be.OnInteract(byPlayer, blockSel);
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            bool val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);

            if (val)
            {
                if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEGrandfatherClock be)
                {
                    BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
                    double dx = byPlayer.Entity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
                    double dz = (float)byPlayer.Entity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
                    float angleHor = (float)Math.Atan2(dx, dz);
                    float deg22dot5rad = GameMath.PIHALF / 4;
                    float roundRad = (int)Math.Round(angleHor / deg22dot5rad) * deg22dot5rad;
                    be.MeshAngle = roundRad;
                    if (world.Side == EnumAppSide.Client)
                    {
                        be.UpdateMesh();
                    }
                    be.MarkDirty(true);
                }
            }
            return val;
        }

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            if (blockAccessor.GetBlockEntity(pos) is BEGrandfatherClock be)
            {
                Cuboidf[] selectionBoxes = new Cuboidf[1];
                float angleDeg = be.MeshAngle * GameMath.RAD2DEG;
                float roundedAngle = (float)Math.Round(angleDeg / 90f) * 90f;
                if (roundedAngle < 0)
                {
                    roundedAngle += 360;
                }
                switch (roundedAngle)
                {
                    case 0:
                        selectionBoxes[0] = SelectionBoxes[0];
                        break;

                    case 90:
                        selectionBoxes[0] = SelectionBoxes[1];
                        break;

                    case 180:
                        selectionBoxes[0] = SelectionBoxes[2];
                        break;

                    case 270:
                        selectionBoxes[0] = SelectionBoxes[3];
                        break;

                    default:
                        selectionBoxes[0] = SelectionBoxes[0];
                        api.Logger.Error("Achtung! Angle not correct!");
                        break;
                }

                return selectionBoxes;
            }

            return base.GetSelectionBoxes(blockAccessor, pos);
        }

        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return GetSelectionBoxes(blockAccessor, pos);
        }
    }
}
