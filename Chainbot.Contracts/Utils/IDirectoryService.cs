using Chainbot.Contracts.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Utils
{
    public interface IDirectoryService
    {
        List<DirOrFileItem> Query(string path, Predicate<DirOrFileItem> match = null);
    }
}
