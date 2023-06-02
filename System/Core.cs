using Vintagestory.API.Common;

namespace DecoClock
{
    class Core : ModSystem
    {
        public static string ModId { get; private set; }

        public override void Start(ICoreAPI api)
        {
            ModId = Mod.Info.ModID;
            api.RegisterBlockClass("bigclock", typeof(ClockBlock));
            api.RegisterBlockClass("wallclock", typeof(WallClockBlock));
            api.RegisterBlockClass("grandfatherclock", typeof(GrandfatherClockBlock));
            api.RegisterBlockEntityClass("bebigclock", typeof(BEBigClock));
            api.RegisterBlockEntityClass("bewallclock", typeof(BEWallClock));
            api.RegisterBlockEntityClass("begrandfatherclock", typeof(BEGrandfatherClock));
        }
    }
}
