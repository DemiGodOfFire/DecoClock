using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace decoclock.src
{
    class DecoClock: ModSystem
    {
       

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("wallclock", typeof(WallClock));
            api.RegisterBlockClass("grandfatherclock", typeof(GrandfatherClock));
            //api.RegisterBlockClass("alarmclock", typeof(AlarmClock));
            //api.RegisterBlockEntityClass("beclock", typeof(BEClock));
            api.RegisterBlockEntityClass("begrandfatherclock", typeof(BEGrandfatherClock));
          
        }
    }
}
