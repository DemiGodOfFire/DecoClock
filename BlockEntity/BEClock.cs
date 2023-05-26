using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DecoClock
{
    internal class BEClock : BlockEntity, ITexPositionSource
    {

        InventoryClock? inventory;
        GuiDialogClock? dialogClock;
        ILoadedSound ambientSound;
        ClockHandRenderer rendererHand;
        ITexPositionSource textureSource;
        MeshData baseMesh;
        public Size2i AtlasSize => textureSource.AtlasSize;
        protected List<ClockItem> Parts { get { if (_parts.Count == 0) { AddParts(); } return _parts; } }


        ILoadedSound? openSound;
        ILoadedSound? closeSound;

        BlockEntityAnimationUtil? animUtil
        {
            get { return GetBehavior<BEBehaviorAnimatable>()?.animUtil; }
        }

        private List<ClockItem> _parts = new();
        public float MeshAngle;

        protected virtual void AddParts()
        {
            _parts.Add(new("clockwork"));
            _parts.Add(new("tickmarks"));
            _parts.Add(new("hourhand"));
            _parts.Add(new("minutehand"));
            _parts.Add(new("dialglass"));
            _parts.Add(new("clockparts"));
        }


        public virtual TextureAtlasPosition this[string textureCode]
        {
            //get
            //{
            //    if (inventory.TryGetPart("clockparts") != null)
            //    {
            //        if (textureCode == "thread") return textureSource["string"];
            //    }

            //    ItemStack stack = inventory.TryGetPart(textureCode);
            //    if (stack is not null)
            //    {
            //        var capi = Api as ICoreClientAPI;
            //        if (stack.Class == EnumItemClass.Item)
            //        {
            //            var tex = stack.Item.FirstTexture;
            //            AssetLocation texturePath = tex.Baked.BakedName;
            //            // return capi.ItemTextureAtlas[tex.Base];
            //            //   return capi.ItemTextureAtlas.GetPosition(stack.Item);
            //            return GetOrCreateTexPos(texturePath, capi);
            //        }
            //        else
            //        {
            //            // var tex = stack.Block.FirstTexture;
            //            return capi.BlockTextureAtlas.GetPosition(stack.Block);
            //        }
            //    }
            //    return textureSource[textureCode];
            //}
            get
            {
                if (inventory.TryGetPart("clockparts") != null)
                {
                    if (textureCode == "thread") { return textureSource["string"]; }
                }

                ItemStack stack = inventory.TryGetPart(textureCode);
                if (stack is not null)
                {
                    var capi = Api as ICoreClientAPI;
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
            else
            {
                InitInventory();
            }

            if (openSound == null && api.Side == EnumAppSide.Client)
            {
                openSound = ((IClientWorldAccessor)api.World).LoadSound(new SoundParams
                {
                    Location = new AssetLocation("sounds/block/chestopen"),
                    ShouldLoop = false,
                    Position = Pos.ToVec3f().Add(0.5f, 0.25f, 0.5f),
                    DisposeOnFinish = false,
                    Volume = 1f,
                    Range = 16f
                });
            }

            if (closeSound == null && api.Side == EnumAppSide.Client)
            {
                closeSound = ((IClientWorldAccessor)api.World).LoadSound(new SoundParams
                {
                    Location = new AssetLocation("sounds/block/chestclose"),
                    ShouldLoop = false,
                    Position = Pos.ToVec3f().Add(0.5f, 0.25f, 0.5f),
                    DisposeOnFinish = false,
                    Volume = 1f,
                    Range = 16f
                });
            }

            if (api.Side == EnumAppSide.Client)
            {
                //   (api as ICoreClientAPI).Event.RegisterRenderer(rendererHand= new ClockHandRenderer(api as ICoreClientAPI,
                //       inventory.TryGetPart("hourhand"),
                //       inventory.TryGetPart("minutehand"),
                //       Pos),EnumRenderStage.Opaque);
                //(api as ICoreClientAPI).Event.RegisterRenderer(rendererHand = new ClockHandRenderer(api as ICoreClientAPI, Pos)
                //    , EnumRenderStage.Opaque);
                rendererHand = new ClockHandRenderer(api as ICoreClientAPI, Pos);
                (api as ICoreClientAPI).Event.RegisterRenderer(rendererHand, EnumRenderStage.Opaque, "clock");
            }
            if (api is ICoreClientAPI capi)
            {
                textureSource = capi.Tesselator.GetTexSource(Block);
                // animUtil.InitializeAnimator(Core.ModId + "clock");
                //animUtil.StartAnimation(new AnimationMetaData
                //{
                //    Animation = "idle",
                //    Code = "idle",
                //    AnimationSpeed = 1.8f,
                //    EaseOutSpeed = 6,
                //    EaseInSpeed = 15
                //});
                var angle = MeshAngle;
                if (MeshAngle != 0)
                {
                   
                }
                UpdateMesh();
                //               rendererHand = new ClockHandRenderer(capi, Pos);
            }
        }

        public bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            if (dialogClock == null && Api.Side == EnumAppSide.Client)
            {
                dialogClock = new GuiDialogClock(inventory, Pos, (ICoreClientAPI)Api);
                dialogClock.OnOpened += () => { openSound?.Start(); };
                dialogClock.OnClosed += () =>
                {
                    closeSound?.Start();
                    // animUtil?.StopAnimation("opendoor");
                };
            }


            if (Api.Side == EnumAppSide.Client)
            {
                dialogClock?.TryOpen();

                //animUtil.StartAnimation(new AnimationMetaData
                //{
                //    Animation = "opendoor",
                //    Code = "opendoor",
                //    AnimationSpeed = 1.8f,
                //    EaseOutSpeed = 6,
                //    EaseInSpeed = 15,
                //    SupressDefaultAnimation = true

                //});
            }
            /*ItemSlot handslot = byPlayer.InventoryManager.ActiveHotbarSlot;
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
            }*/

            return false;
        }

        #region meshing

        public MeshData GenBaseMesh(ITesselatorAPI tesselator)
        {
            //ITesselatorAPI tesselator = ((ICoreClientAPI)Api).Tesselator;
            AssetLocation assetLocation = Block.Shape.Base.WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json");
            Shape shape = Api.Assets.TryGet(assetLocation).ToObject<Shape>();
            tesselator.TesselateShape("BeClock", shape, out MeshData mesh, this);



            return mesh;
        }

        public MeshData GenMesh(string type, ITesselatorAPI? tesselator = null)
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

        public MeshData? GetItemMesh(string part)
        {
            if (inventory != null)
            {
                var inv = inventory.TryGetPart(part);
                if(inv!=null)
                {

                    //// var type = inventory.TryGetPart(part)?.Item.Code.EndVariant();
                    ITesselatorAPI tesselatorHand = ((ICoreClientAPI)Api).Tesselator;
                    string path = $"decoclock:shapes/block/grandfatherclock/{part}.json";
                    Shape shape = Api.Assets.TryGet(path).ToObject<Shape>();
                    tesselatorHand.TesselateShape("handClock", shape, out MeshData mesh, this);
                    return mesh;
                }
            }
            return null;
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            if (baseMesh != null)
            {
                mesher.AddMeshData(baseMesh);
            }
            return true;
        }

        public void UpdateMesh(ITesselatorAPI? tesselator = null)
        {
            if (tesselator == null)
            {
                tesselator = ((ICoreClientAPI)Api).Tesselator;
            }
            MeshData mesh = GenBaseMesh(tesselator);
            Vec3f rotationMesh = new(0, MeshAngle * 180 / (float)Math.PI, 0);
            //((ICoreClientAPI)Api).Render.UpdateMesh(animUtil.renderer.meshref, mesh);
            //animUtil.renderer.rotationDeg = rotationMesh;
            baseMesh = mesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, MeshAngle, 0);
            rendererHand.Update(GetItemMesh("hourhand"), GetItemMesh("minutehand"), MeshAngle);

        }

        #endregion

        public void DropContents(Vec3d atPos)
        {
            inventory?.DropAll(Pos.ToVec3d().Add(0.5, 0.5, 0.5));
        }

        #region Events


        public override void OnReceivedClientPacket(IPlayer player, int packetid, byte[] data)
        {
            if (packetid < 1000)
            {
                inventory.InvNetworkUtil.HandleClientPacket(player, packetid, data);

                // Tell server to save this chunk to disk again
                Api.World.BlockAccessor.GetChunkAtBlockPos(Pos.X, Pos.Y, Pos.Z).MarkModified();
                MarkDirty(true);
                return;
            }
        }




        public void OpenDoor()
        {

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

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();

            //if (ambientSound != null)
            //{
            //    ambientSound.Stop();
            //    ambientSound.Dispose();
            //}
            // animUtil?.Dispose();
            openSound?.Dispose();

            if (Api?.World == null) return;


            rendererHand?.Dispose();
        }

        public override void OnBlockBroken(IPlayer? byPlayer = null)
        {
            base.OnBlockBroken(byPlayer);
            if (Api.World is IServerWorldAccessor)
            {
                inventory?.DropAll(Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }
        }

        #endregion

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            rendererHand?.Dispose();
            ambientSound?.Dispose();
            openSound?.Dispose();
            //animUtil?.Dispose();
        }
    }

}
