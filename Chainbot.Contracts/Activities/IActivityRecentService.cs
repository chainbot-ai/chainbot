﻿using Chainbot.Contracts.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Activities
{
    public interface IActivityRecentService
    {
        List<ActivityGroupOrLeafItem> Query();
        void Add(string typeOf);
    }
}