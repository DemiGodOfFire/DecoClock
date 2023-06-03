using DecoClock.Render;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace DecoClock
{
    internal class BEGrandfatherClock : BEClock
    {
        ILoadedSound openSound = null!;
        ILoadedSound closeSound = null!;
        ILoadedSound chimeSound = null!;

        GuiDialogGrandfatherClock dialogClock = null!;
        ClockHandRenderer rendererHand = null!;
        PendulumRenderer rendererPendulum = null!;
        GrandfatherClockDoorRenderer rendererDoor = null!;
        public override string PathBlock => "decoclock:shapes/block/grandfatherclock/";


        private bool isWork;
        public float timeWork;
        
        public override void AddParts()
        {
            _parts.Add(new("clockwork"));
            _parts.Add(new("tickmarks"));
            _parts.Add(new("hourhand"));
            _parts.Add(new("minutehand"));
            _parts.Add(new("dialglass"));
            _parts.Add(new("clockparts"));
            _parts.Add(new("doorglass"));
        }

      

        public override bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            if (dialogClock == null && Api.Side == EnumAppSide.Client)
            {
                dialogClock = new GuiDialogGrandfatherClock(Core.ModId + ":fatherclock-title", Inventory, Pos, (ICoreClientAPI)Api);
                dialogClock.OnOpened += () =>
                {
                    openSound?.Start();
                    rendererDoor.Open();
                };
                dialogClock.OnClosed += () =>
                {
                    closeSound?.Start();
                    rendererDoor.Close();
                };
            }

            if (Api.Side == EnumAppSide.Client)
            {
                dialogClock?.TryOpen();
            }
            return false;
        }

        #region meshing


        public override void UpdateMesh(ITesselatorAPI? tesselator = null)
        {
            base.UpdateMesh(tesselator);
            rendererHand.Update(GetItemMesh("hourhand"), GetItemMesh("minutehand"), MeshAngle, IsWork());
            rendererDoor.Update(GetMesh("door"), MeshAngle);
            rendererPendulum.Update(GetItemMesh("clockparts", "pendulum"),
                GetItemMesh("clockparts", "weight"), MeshAngle);
            
        }
       

        #endregion

        public override void LoadSound(ICoreClientAPI capi)
        {
            base.LoadSound(capi);
            chimeSound ??= ((IClientWorldAccessor)capi.World).LoadSound(new SoundParams
            {
                Location = new AssetLocation("decoclock:sounds/chimeend1"),
                ShouldLoop = false,
                Position = Pos.ToVec3f().Add(0.5f, 1.5f, 0.5f),
                DisposeOnFinish = false,
                Volume = 1f,
                Range = 48f,
                SoundType = EnumSoundType.Ambient
            });

            openSound ??= ((IClientWorldAccessor)capi.World).LoadSound(new SoundParams
            {
                Location = new AssetLocation("sounds/block/chestopen"),
                ShouldLoop = false,
                Position = Pos.ToVec3f().Add(0.5f, 0.25f, 0.5f),
                DisposeOnFinish = false,
                Volume = 1f,
                Range = 16f
            });

            closeSound ??= ((IClientWorldAccessor)capi.World).LoadSound(new SoundParams
            {
                Location = new AssetLocation("sounds/block/chestclose"),
                ShouldLoop = false,
                Position = Pos.ToVec3f().Add(0.5f, 0.25f, 0.5f),
                DisposeOnFinish = false,
                Volume = 1f,
                Range = 16f
            });
        }

        public override void RegisterRenderer(ICoreClientAPI capi)
        {
            capi.Event.RegisterRenderer(rendererHand =
                new ClockHandRenderer(capi, Pos), EnumRenderStage.Opaque);
            capi.Event.RegisterRenderer(rendererDoor =
                new GrandfatherClockDoorRenderer(capi, Pos), EnumRenderStage.Opaque);
            capi.Event.RegisterRenderer(rendererPendulum =
               new PendulumRenderer(capi, Pos), EnumRenderStage.Opaque);
            rendererHand.MinuteTick += () => { TickSound?.Start(); };
            rendererHand.HourTick += (_) => { chimeSound?.Start(); };
        }

        private bool IsWork()
        {
            return (Inventory.IsExist("clockwork") && Inventory.IsExist("clockparts"));
        }

        #region Events

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            openSound?.Stop();
            openSound?.Dispose();
            closeSound?.Stop();
            closeSound?.Dispose();
            chimeSound?.Stop();
            chimeSound?.Dispose();
            dialogClock?.TryClose();
            rendererHand?.Dispose();
            rendererDoor?.Dispose();
            rendererPendulum?.Dispose();
        }


        #endregion

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            rendererHand?.Dispose();
            rendererDoor?.Dispose();
            rendererPendulum?.Dispose();
            openSound?.Dispose();
            closeSound?.Dispose();
            chimeSound?.Dispose();
        }

    }
}