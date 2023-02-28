using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace DecoClock
{
    internal class BEClock : BlockEntity, ITexPositionSource
    {

        InventoryClock inventory;
        ILoadedSound ambientSound;
        ClockHandRenderer rendererHand;
        ITexPositionSource textureSource;
        MeshData baseMesh;
        public Size2i AtlasSize => textureSource.AtlasSize;
        protected List<ClockItem> Parts { get { if (_parts.Count == 0) { AddParts(); } return _parts; } }
        private List<ClockItem> _parts = new();
        public float MeshAngle;

        protected virtual void AddParts()
        {
            _parts.Add(new("hourhand"));
            _parts.Add(new("clockwork"));
            _parts.Add(new("dialglass"));
            _parts.Add(new("tickmarks"));
            _parts.Add(new("minutehand"));
            _parts.Add(new("clockparts"));
        }


        public virtual TextureAtlasPosition this[string textureCode]
        {
            get
            {
                if (inventory.TryGetPart("clockparts") != null)
                {
                    if (textureCode == "thread") return textureSource["string"];
                }

                ItemStack stack = inventory.TryGetPart(textureCode);
                if (stack is not null)
                {
                    var capi = Api as ICoreClientAPI;
                    if (stack.Class == EnumItemClass.Item)
                    {
                        var tex = stack.Item.FirstTexture;
                        AssetLocation texturePath = tex.Baked.BakedName;
                        // return capi.ItemTextureAtlas[tex.Base];
                        //   return capi.ItemTextureAtlas.GetPosition(stack.Item);
                        return GetOrCreateTexPos(texturePath, capi);
                    }
                    else
                    {
                        // var tex = stack.Block.FirstTexture;
                        return capi.BlockTextureAtlas.GetPosition(stack.Block);
                    }
                }
                return textureSource[textureCode];
            }
        }

        protected TextureAtlasPosition GetOrCreateTexPos(AssetLocation texturePath, ICoreClientAPI capi)
        {
            TextureAtlasPosition texpos = capi.BlockTextureAtlas[texturePath];

            if (texpos == null)
            {
                IAsset texAsset = capi.Assets.TryGet(texturePath.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"));
                if (texAsset != null)
                {
                    capi.BlockTextureAtlas.GetOrInsertTexture(texturePath, out _, out texpos, () => texAsset.ToBitmap(capi));
                }
                else
                {
                    capi.World.Logger.Warning("For render in block " + Block.Code + ", item {0} defined texture {1}, no such texture found.", texturePath);
                }
            }
            return texpos;
        }



        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            MeshAngle = tree.GetFloat("meshAngle", MeshAngle);
            InitInventory();
            inventory.FromTreeAttributes(tree);
            if (Api is ICoreClientAPI)
            {
                UpdateMesh();
                MarkDirty(true);
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetFloat("meshAngle", MeshAngle);
            inventory?.ToTreeAttributes(tree);
        }

        private void InitInventory()
        {
            if (inventory == null)
            {
                inventory = new InventoryClock(Parts.ToArray(), Pos, Api);
            }
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (inventory != null)
            {
                inventory.LateInitialize(inventory.InventoryID, api);
            }
            else InitInventory();
            if (api is ICoreClientAPI capi)
            {
                textureSource = capi.Tesselator.GetTexSource(Block);
                    UpdateMesh();
                //               rendererHand = new ClockHandRenderer(capi, Pos);
            }
        }

        public bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot handslot = byPlayer.InventoryManager.ActiveHotbarSlot;

            if (inventory.TryAddPart(handslot.Itemstack, out ItemStack content))
            {
                var pos = Pos.ToVec3d().Add(0.5, 0.25, 0.5);
                Api.World.PlaySoundAt(Block.Sounds.Place, pos.X, pos.Y, pos.Z, byPlayer);
                (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                //delete one item from player
                if (Api.Side == EnumAppSide.Client)
                {
                    UpdateMesh();
                }
                MarkDirty(true);
                return true;
            }

            return false;
        }


        public MeshData GenBaseMesh(ITesselatorAPI tesselator)
        {
            //ITesselatorAPI tesselator = ((ICoreClientAPI)Api).Tesselator;
            AssetLocation assetLocation = Block.Shape.Base.WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json");
            Shape shape = Api.Assets.TryGet(assetLocation).ToObject<Shape>();
            tesselator.TesselateShape("BeClock", shape, out MeshData mesh, this);

            return mesh;
        }

        public MeshData GenMesh(string type, ITesselatorAPI tesselator = null)
        {
            if (tesselator == null)
            {
                tesselator = ((ICoreClientAPI)Api).Tesselator;
            }
            AssetLocation assetLocation = Block.Shape.Base.WithPathPrefixOnce("shapes/");
            assetLocation.Path = assetLocation.Path.Replace("/complete", $"/{type}.json");
            Shape shape = Api.Assets.TryGet(assetLocation).ToObject<Shape>();
            tesselator.TesselateShape("BeClock", shape, out MeshData mesh, this);

            return mesh;
        }

        #region meshing


        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            if (baseMesh != null)
            {
                mesher.AddMeshData(baseMesh);
            }
            return true;
        }

        public void UpdateMesh(ITesselatorAPI tesselator = null)
        {
            if (tesselator == null)
            {
                tesselator = ((ICoreClientAPI)Api).Tesselator;
            }
            baseMesh = GenBaseMesh(tesselator).Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, MeshAngle, 0);
        }


        #endregion

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();

            //if (ambientSound != null)
            //{
            //    ambientSound.Stop();
            //    ambientSound.Dispose();
            //}

            if (Api?.World == null) return;


            rendererHand?.Dispose();
            rendererHand = null;
        }

        public override void OnBlockBroken(IPlayer byPlayer = null)
        {
            base.OnBlockBroken(byPlayer);
            if (Api.World is IServerWorldAccessor)
            {
                inventory?.DropAll(Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            rendererHand?.Dispose();
            ambientSound?.Dispose();
        }
    }

}
