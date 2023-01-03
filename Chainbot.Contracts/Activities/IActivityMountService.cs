﻿using Chainbot.Contracts.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Activities
{
    public interface IActivityMountService
    {
        List<ActivityGroupOrLeafItem> Transform(string activityConfigXml);

        List<ActivityGroupOrLeafItem> Mount(List<ActivityGroupOrLeafItem> activitiesCurrent
            , List<ActivityGroupOrLeafItem> activitiesToMount);
    }
}
