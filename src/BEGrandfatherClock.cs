﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace decoclock.src
{
    internal class BEGrandfatherClock : BlockEntity, ITexPositionSource
    {
        ICoreAPI api;
        GrandfatherClock ownBlock;
        InventoryGeneric inventory;
        ILoadedSound ambientSound;
        ClockHandRenderer rendererHourHand;
        ClockHandRenderer rendererMinuteHand;
        MeshData currentMesh;
        ITexPositionSource tmpTextureSource;

        BlockEntityAnimationUtil AnimUtil => GetBehavior<BEBehaviorAnimatable>()?.animUtil;

        public Size2i AtlasSize { get; set; }

        

        string curMatMHand = "silver";
        string curMatHHand = "meteoriciron";
        string curMatClockParts = "brass";
        string curMatClockWork = "gold";


        bool minuteHand;
        bool hourHand;
        bool clockParts;
        bool clockWork;
        bool minuteHandRotate;
        bool hourHandRotate;
        public float MeshAngle;
       
        float hour;
        float minutes;
        long? listenerid;

        #region Getters

        public string Material
        {
            get { return Block.LastCodePart(); }
        }

        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                if (textureCode == "hour") return tmpTextureSource["metal-" + curMatHHand];
                if (textureCode == "minute") return tmpTextureSource["metal-" + curMatMHand];
                if (textureCode == "parts") return tmpTextureSource["metal-" + curMatClockParts];
                if (textureCode == "parts") return tmpTextureSource["metal-" + curMatClockParts];
                if (textureCode == "clockWork") return tmpTextureSource["metal-" + curMatClockWork];
                return tmpTextureSource[textureCode];
            }
        }

        MeshData ClockBaseMesh
        {
            get
            {
                object value;
                Api.ObjectCache.TryGetValue("clockbasemesh-" + Material, out value);
                return (MeshData)value;
            }
            set { Api.ObjectCache["clockbasemesh-" + Material] = value; }
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
            if (Api != null && Api.Side == EnumAppSide.Client)
            {
            
               currentMesh = GenMesh();
                MarkDirty(true);
            }

            inventory.FromTreeAttributes(tree.GetTreeAttribute("inventory"));

            if (Api != null)
            {
                inventory.AfterBlocksLoaded(Api.World);
            }

        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetFloat("meshAngle", MeshAngle);
        }

        private void InitInventory()
        {
            if (inventory == null)
            {
                inventory = new InventoryGeneric(4, "DecoClock-grandfather_clock", Pos+"", api, null);

            }
        }

        public override void Initialize(ICoreAPI api)
        {

            ownBlock = Block as GrandfatherClock;

            if (api is ICoreClientAPI capi) {
                tmpTextureSource = capi.Tesselator.GetTexSource(Block);
            }
            
            

            this.api = api;
            base.Initialize(api);
            InitInventory();

            if (api.Side == EnumAppSide.Client)
            {
                base.Initialize(api);

                ownBlock = Block as GrandfatherClock;
                if (Api.Side == EnumAppSide.Client)
                {
                    currentMesh = GenMesh();
                    MarkDirty(true);
                }
            }



            if (minuteHand && hourHand && clockWork && clockParts)
            {
                SetupGameTickers();
            }

            if (api.Side == EnumAppSide.Client)
            {
                //rendererHourHand = new ClockHandRenderer(api as ICoreClientAPI, Pos, GenMesh("hour_hand"));

                //(api as ICoreClientAPI).Event.RegisterRenderer(rendererHourHand, EnumRenderStage.Opaque, "grandfather_clock");

                rendererMinuteHand = new ClockHandRenderer(api as ICoreClientAPI, Pos, GenMesh("minute_hand"));

                (api as ICoreClientAPI).Event.RegisterRenderer(rendererMinuteHand, EnumRenderStage.Opaque, "grandfather_clock");

                if (ClockBaseMesh == null)
                {
                    ClockBaseMesh = GenMesh("base");
                }
                if (ClockMinuteHandMesh == null)
                {
                    ClockMinuteHandMesh = GenMesh("minute_hand");
                }
                if (ClockHourHandMesh == null)
                {
                    ClockHourHandMesh = GenMesh("hour_hand");
                }
                
            }
        }

        private void SetupGameTickers()
        {
            listenerid = RegisterGameTickListener(OneMinute, 1000);
        }

        private void RemoveGameTickers()
        {
            if (listenerid != null) UnregisterGameTickListener((long)listenerid);
        }

        private void OneMinute(float dt)
        {
            float hourOfDay = api.World.Calendar.HourOfDay;
            float hourUpdate = (api.World.Calendar.FullHourOfDay / api.World.Calendar.HoursPerDay * 24);
          

            float minutesFloat = hourOfDay - hour;
            float minutesUpdate = (minutesFloat * 60f);

            if( minuteHandRotate = (minutes != minutesUpdate))
            {
                minutes = minutesUpdate;
            }
            else return;
            
            if (hourHandRotate = hour != hourUpdate)
            {
                hour = hourUpdate;
            }

            int hourM12 = (int)hourOfDay % 12;
            UpdateHandState();
            
        }

        void UpdateHandState() 
        {
            if (Api?.World == null) return;
            
            if (rendererMinuteHand != null)
                {
                    rendererMinuteHand.AngleRad = MinuteAngle();
                }

                Api.World.BlockAccessor.MarkBlockDirty(Pos, OnRetesselatedMinuteHand);

                //if (nowGrinding)
                //{
                //    ambientSound?.Start();
                //}
                //else
                //{
                //    ambientSound?.Stop();
                //}

                if (Api.Side == EnumAppSide.Server)
                {
                    MarkDirty();
                }
                        
        }

        private void OnRetesselatedMinuteHand()
        {
            if (rendererMinuteHand == null) return; // Maybe already disposed

           
            rendererMinuteHand.ShouldRender = minuteHandRotate;

        }

        private float MinuteAngle()
        {
            return 2 * (float)(Math.PI) * minutes / 60; // check values
        }

        internal MeshData GenMesh(string type = "base")
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            if (block.BlockId == 0) return null;
            MeshData mesh;
            ITesselatorAPI mesher = ((ICoreClientAPI)Api).Tesselator;

            mesher.TesselateShape(block, Api.Assets.TryGet("decoclock:shapes/block/grandfather_clock/" + type + ".json").ToObject<Shape>(), out mesh);

            return mesh;
        }

        #region meshing

       
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            if (Block == null) return false;
            mesher.AddMeshData(currentMesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, MeshAngle, 0));
           
            //bool skipmesh = base.OnTesselation(mesher, tesselator);
            //if (skipmesh) return true;


            //if (ownMesh == null)
            //{
            //    return true;
            //}

            // mesher.AddMeshData(ownMesh);
            //            if (quantityPlayersGrinding == 0 && !automated)
            //            {
            //                mesher.AddMeshData(
            //                    this.clockHandMesh.Clone()
            //                    .Rotate(new API.MathTools.Vec3f(0.5f, 0.5f, 0.5f), 0, renderer.AngleRad, 0)
            //                    .Translate(0 / 16f, 11 / 16f, 0 / 16f)
            //                );
            //            }


            return true;
        }


        #endregion

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            RemoveGameTickers();// ???
            //if (ambientSound != null)
            //{
            //    ambientSound.Stop();
            //    ambientSound.Dispose();
            //}

            if (Api?.World == null) return;

           
            rendererMinuteHand?.Dispose();
            
            rendererMinuteHand = null;
        }

        public override void OnBlockBroken(IPlayer byPlayer = null)                 // need OnBlockBroken or GetDrops?
        {

            if (Api.World is IServerWorldAccessor)
            {
                inventory.DropAll(Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }
        }



        ~BEGrandfatherClock()
        {
            if (ambientSound != null) ambientSound.Dispose();
        }


        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            RemoveGameTickers();
            rendererMinuteHand?.Dispose();

        }
    }
    
}
