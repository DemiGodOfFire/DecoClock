using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace DecoClock
{
    class Core : ModSystem
    {
        public static string ModId { get; private set; }

        public override void Start(ICoreAPI api)
        {
            ModId = Mod.Info.ModID;
            api.RegisterBlockClass("grandfatherclock", typeof(ClockBlock));
            api.RegisterBlockClass("wallclock", typeof(ClockBlock));
            //api.RegisterBlockClass("alarmclock", typeof(AlarmClock));
            //api.RegisterBlockEntityClass("beclock", typeof(BEClock));
            api.RegisterBlockEntityClass("begrandfatherclock", typeof(BEClock));

        }
    }
}
