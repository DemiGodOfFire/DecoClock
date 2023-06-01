using Vintagestory.API.Common;

namespace DecoClock
{
    class Core : ModSystem
    {
        public static string ModId { get; private set; }

        public override void Start(ICoreAPI api)
        {
            ModId = Mod.Info.ModID;
            api.RegisterBlockClass("rotatableclock", typeof(GrandfatherClockBlock));
            api.RegisterBlockClass("wallclock", typeof(WallClockBlock));
            api.RegisterBlockClass("bewallclock", typeof(BEClock));

            //api.RegisterBlockClass("alarmclock", typeof(AlarmClock));
            //api.RegisterBlockEntityClass("beclock", typeof(BEClock));
            api.RegisterBlockEntityClass("begrandfatherclock", typeof(BEGrandfatherClock));

        }
    }
}
