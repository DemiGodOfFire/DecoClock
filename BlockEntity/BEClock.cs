using System;
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

        //{
        //    new("hourhand"),
        //    new("minutehand"),
        //    new("dialglass"),
        //    new("clockparts"),
        //    new("clockwork"),
        //    new("tickmarks")
        //};

        string curMatMHand = "silver";
        string curMatHHand = "meteoriciron";



        public float MeshAngle;




        #region Getters

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

                if (textureCode == "thread") return textureSource["string"];

                ItemStack stack = inventory.TryGetPart(textureCode);
                if (stack is not null)
                {
                    var capi = (ICoreClientAPI)Api;
                    if (stack.Class == EnumItemClass.Item)
                    {
                        var tex = stack.Item.FirstTexture;
                        AssetLocation texturePath = tex.Baked.BakedName;
                        // return capi.ItemTextureAtlas[tex.Base];
                        //   return capi.ItemTextureAtlas.GetPosition(stack.Item);
                        return getOrCreateTexPos(texturePath, capi);
                    }
                    else
                    {
                        // var tex = stack.Block.FirstTexture;
                        return capi.BlockTextureAtlas.GetPosition(stack.Block);
                        //return capi.BlockTextureAtlas.GetPosition(stack.Block);
                    }
                }
                return textureSource[textureCode];
            }
        }

        protected TextureAtlasPosition getOrCreateTexPos(AssetLocation texturePath, ICoreClientAPI capi)
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

        MeshData ClockHourHandMesh
        {
            get
            {
                object value = null;
                Api.ObjectCache.TryGetValue("clockhourhandmesh-" + curMatHHand, out value);
                return (MeshData)value;
            }
            set { Api.ObjectCache["clockhourhandmesh-" + curMatHHand] = value; }
        }

        MeshData ClockMinuteHandMesh
        {
            get
            {
                object value = null;
                Api.ObjectCache.TryGetValue("clockminutehandmesh-" + curMatMHand, out value);
                return (MeshData)value;
            }
            set { Api.ObjectCache["clockminutehandmesh-" + curMatMHand] = value; }
        }

        #endregion


        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            MeshAngle = tree.GetFloat("meshAngle", MeshAngle);

            InitInventory();
            inventory.FromTreeAttributes(tree);
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
            if (api is ICoreClientAPI capi)
            {
                textureSource = capi.Tesselator.GetTexSource(Block);
            }
            //AddParts();
            if (inventory != null)
            {
                inventory.LateInitialize(inventory.InventoryID, api);
            }
            else InitInventory();
        }



        //private void OneMinute(float dt)
        //{
        //    float hourOfDay = api.World.Calendar.HourOfDay;
        //    float hourUpdate = api.World.Calendar.FullHourOfDay / api.World.Calendar.HoursPerDay * 24;


        //    float minutesFloat = hourOfDay - hour;
        //    float minutesUpdate = minutesFloat * 60f;

        //    if (minuteHandRotate = minutes != minutesUpdate)
        //    {
        //        minutes = minutesUpdate;
        //    }
        //    else return;

        //    if (hourHandRotate = hour != hourUpdate)
        //    {
        //        hour = hourUpdate;
        //    }

        //    int hourM12 = (int)hourOfDay % 12;
        //    UpdateHandState();

        //}

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

        //void UpdateHandState()
        //{
        //    if (Api?.World == null) return;

        //    if (rendererHand != null)
        //    {
        //        rendererHand.AngleRad = MinuteAngle();
        //    }

        //    Api.World.BlockAccessor.MarkBlockDirty(Pos, OnRetesselatedMinuteHand);

        //    //if (nowGrinding)
        //    //{
        //    //    ambientSound?.Start();
        //    //}
        //    //else
        //    //{
        //    //    ambientSound?.Stop();
        //    //}

        //    if (Api.Side == EnumAppSide.Server)
        //    {
        //        MarkDirty();
        //    }

        //}

        //private void OnRetesselatedMinuteHand()
        //{
        //    if (rendererHand == null) return; // Maybe already disposed


        //    rendererHand.ShouldRender = minuteHandRotate;

        //}

        //private float MinuteAngle()
        //{
        //    return 2 * (float)Math.PI * minutes / 60; // check values
        //}

        internal MeshData GenBaseMesh()
        {
            ITesselatorAPI mesher = ((ICoreClientAPI)Api).Tesselator;
            AssetLocation assetLocation = Block.Shape.Base.WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json");
            Shape shape = Api.Assets.TryGet(assetLocation).ToObject<Shape>();
            mesher.TesselateShape("BeClock", shape, out MeshData mesh, this);

            return mesh;
        }

        internal MeshData GenMesh(string type)
        {
            var capi = Api as ICoreClientAPI;
            Shape shape = Api.Assets.TryGet("decoclock:shapes/block/grandfatherclock/" + type + ".json").ToObject<Shape>();
            capi.Tesselator.TesselateShape("BeClock", shape, out MeshData mesh, this);
            return mesh;
        }

        #region meshing


        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            if (baseMesh is null)
            {
                UpdateMesh();
            }
            mesher.AddMeshData(baseMesh);

            return true;
        }

        private void UpdateMesh()
        {
            baseMesh = GenBaseMesh().Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, MeshAngle, 0);
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

        public override void OnBlockBroken(IPlayer byPlayer = null)                 // need OnBlockBroken or GetDrops?
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
