using DecoClock.Render;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace DecoClock
{
    internal class BECuckooClock : BEClock
    {
        //ILoadedSound openSound = null!;
        //ILoadedSound closeSound = null!;
        ILoadedSound cuckooSound = null!;

        GuiDialogCuckooClock dialogClock = null!;
        PendulumClockRenderer rendererCuckooClock = null!;
        CuckooRenderer rendererCuckoo = null!;
        public override string PathBlock => "decoclock:shapes/block/cuckooclock/";

        public override void AddParts()
        {
            _parts.Add(new("clockwork"));
            _parts.Add(new("tickmarks", "clockwork"));
            _parts.Add(new("hourhand", "clockwork"));
            _parts.Add(new("minutehand", "clockwork"));
            _parts.Add(new("dialglass"));
            _parts.Add(new("clockparts", "clockwork"));
            _parts.Add(new("cuckoo"));
        }

        public override bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            if (dialogClock == null && Api.Side == EnumAppSide.Client)
            {
                dialogClock = new(Core.ModId + ":cuckooclock-title", Inventory, Pos, (ICoreClientAPI)Api);
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
            rendererCuckoo.Update(
                GetItemMesh("cuckoo"), 0.675f, 0.3f,
                GetMesh("doorR"), GetMesh("doorL"), 0.0625f, 0.15625f, 0.185f,
                MeshAngle);
            rendererCuckooClock.Update(
                GetItemMesh("hourhand"), -0.181f,
                GetItemMesh("minutehand"), -0.1875f, -0.174f,
                GetItemMesh("tickmarks", TypeDial), -0.1875f, -0.1875f,
                GetPartItemMesh("clockparts", "pendulum"), -0.5f, -0.05f,
                GetPartItemMesh("clockparts", "weight"), 0.15f, -0.25f, -0.25f,
                MeshAngle);
           // Api.World.Logger.Warning("Mute " + MuteSounds);

        }

        #endregion

        public override void LoadSound(ICoreClientAPI capi)
        {
            base.LoadSound(capi);
            cuckooSound ??= ((IClientWorldAccessor)capi.World).LoadSound(new SoundParams
            {
                Location = new AssetLocation("decoclock:sounds/cuckoo-clock-chime"),
                ShouldLoop = false,
                Position = Pos.ToVec3f().Add(0.5f, 1.5f, 0.5f),
                DisposeOnFinish = false,
                Volume = 1f,
                Range = 48f,
                SoundType = EnumSoundType.Ambient
            });
        }

        public override void RegisterRenderer(ICoreClientAPI capi)
        {
            capi.Event.RegisterRenderer(rendererCuckoo =
               new(capi, Pos), EnumRenderStage.Opaque);
            capi.Event.RegisterRenderer(rendererCuckooClock =
               new(capi, Pos), EnumRenderStage.Opaque);
            rendererCuckooClock.MinuteTick += () => { if (!MuteSounds) TickSound?.Start(); };
            rendererCuckooClock.HourTick += (_) => { rendererCuckoo?.Cu(); if (!MuteSounds) cuckooSound?.Start(); };
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            cuckooSound?.Stop();
            cuckooSound?.Dispose();
            dialogClock?.TryClose();
            rendererCuckoo?.Dispose();
            rendererCuckooClock?.Dispose();
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            cuckooSound?.Dispose();
            rendererCuckoo?.Dispose();
            rendererCuckooClock?.Dispose();
        }
    }
}
