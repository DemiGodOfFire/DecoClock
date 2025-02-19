using DecoClock.Render;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace DecoClock
{
    internal class BEGrandfatherClock : BEVariableClock
    {
        ILoadedSound openSound = null!;
        ILoadedSound closeSound = null!;
        ILoadedSound chimeSound = null!;

        GuiDialogGrandfatherClock dialogClock = null!;
        PendulumClockRenderer rendererGrandfatherClock = null!;
        GrandfatherClockDoorRenderer rendererDoor = null!;
        public override string PathBlock => "decoclock:shapes/block/grandfatherclock/";

        public override void AddParts()
        {
            _parts.Add(new("clockwork"));
            _parts.Add(new("tickmarks", "clockwork"));
            _parts.Add(new("hourhand", "clockwork"));
            _parts.Add(new("minutehand", "clockwork"));
            _parts.Add(new("dialglass"));
            _parts.Add(new("clockparts", "clockwork"));
            _parts.Add(new("doorglass"));
        }

        public override bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            if (dialogClock == null && Api.Side == EnumAppSide.Client)
            {
                dialogClock = new GuiDialogGrandfatherClock(Core.ModId + ":grandfatherclock-title", Inventory, Pos, (ICoreClientAPI)Api);
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
            rendererDoor.Update(GetMesh("door"), MeshAngle);
            rendererGrandfatherClock.Update(
                GetItemMesh("hourhand"), -0.11f,
                GetItemMesh("minutehand"), 1f, -0.101f,
                GetItemMesh("tickmarks", TypeDial), 1.0f, -0.113f,
                GetPartItemMesh("clockparts", "pendulum"), 0.625f, -0.05f,
                GetPartItemMesh("clockparts", "weight"), 0.15f, 0.85f, -0.25f,
                MeshAngle);
            //Api.World.Logger.Warning("Mute "+ MuteSounds);

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
                Range = 16f,
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
            capi.Event.RegisterRenderer(rendererDoor =
                new GrandfatherClockDoorRenderer(capi, Pos), EnumRenderStage.Opaque);
            capi.Event.RegisterRenderer(rendererGrandfatherClock =
               new PendulumClockRenderer(capi, Pos), EnumRenderStage.Opaque);
            rendererGrandfatherClock.MinuteTick += () => { if (!MuteSounds) TickSound?.Start(); };
            rendererGrandfatherClock.HourTick += (_) => { if (!MuteSounds) chimeSound?.Start(); };
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
            rendererDoor?.Dispose();
            rendererGrandfatherClock?.Dispose();
        }


        #endregion

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            rendererDoor?.Dispose();
            rendererGrandfatherClock?.Dispose();
            openSound?.Dispose();
            closeSound?.Dispose();
            chimeSound?.Dispose();
        }

    }
}
