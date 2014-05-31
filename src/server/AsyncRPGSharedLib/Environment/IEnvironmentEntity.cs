using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;

namespace AsyncRPGSharedLib.Environment
{
    public interface IEnvironmentEntity
    {
        int ID { get; }
        Point3d Position { get; }
        GameConstants.eFaction Faction { get; }
        int Energy { get; }
        int Health { get; }
    }
}
