using System;
using System.Collections.Generic;
using System.Linq;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Navigation;

namespace AsyncRPGSharedLib.Queries
{
    class EnergyTankQueries
    {
        public static List<EnergyTank> GetEnergyTanks(
            AsyncRPGDataContext db_context,
            RoomKey roomKey)
        {
            List<EnergyTank> energyTanks = new List<EnergyTank>();

            var roomEnergyTankQuery =
                from e in db_context.EnergyTanks
                where e.GameID == roomKey.game_id && e.RoomX == roomKey.x && e.RoomY == roomKey.y && e.RoomZ == roomKey.z
                select e;

            foreach (EnergyTanks dbEnergyTank in roomEnergyTankQuery)
            {
                EnergyTank energyTank = EnergyTank.CreateEnergyTank(dbEnergyTank);

                energyTanks.Add(energyTank);
            }

            return energyTanks;
        }

        public static EnergyTank GetEnergyTank(
            AsyncRPGDataContext db_context,
            int energyTankID)
        {
            return 
                EnergyTank.CreateEnergyTank(
                    (from e in db_context.EnergyTanks 
                     where e.EnergyTankID == energyTankID 
                     select e).Single());
        }

        public static void UpdateEnergyTank(
            AsyncRPGDataContext db_context,
            EnergyTank energyTank)
        {
            var dbEnergyTank =
                (from e in db_context.EnergyTanks
                 where e.EnergyTankID == energyTank.ID
                 select e).SingleOrDefault();

            dbEnergyTank.GameID = energyTank.RoomKey.game_id;
            dbEnergyTank.RoomX = energyTank.RoomKey.x;
            dbEnergyTank.RoomY = energyTank.RoomKey.y;
            dbEnergyTank.RoomZ = energyTank.RoomKey.z;
            dbEnergyTank.X = energyTank.Position.x;
            dbEnergyTank.Y = energyTank.Position.y;
            dbEnergyTank.Z = energyTank.Position.z;
            dbEnergyTank.Ownership = (int)energyTank.Faction;
            dbEnergyTank.Energy = energyTank.Energy;

            db_context.SubmitChanges();
        }

        public static void UpdateEnergyTankEnergy(
            AsyncRPGDataContext db_context,
            int energyTankID,
            int newEnergy)
        {
            var energyTankQuery =
                (from e in db_context.EnergyTanks
                where e.EnergyTankID == energyTankID
                select e).SingleOrDefault();

            db_context.SubmitChanges();
        }

        public static void UpdateEnergyTankOwnership(
            AsyncRPGDataContext db_context,
            int energyTankID,
            GameConstants.eFaction newOwnership)
        {
            var energyTankQuery =
                (from e in db_context.EnergyTanks
                 where e.EnergyTankID == energyTankID
                 select e).SingleOrDefault();

            energyTankQuery.Ownership = (int)newOwnership;

            db_context.SubmitChanges();
        }

        public static void UpdateEnergyTankOwnership(
            AsyncRPGDataContext db_context,
            EnergyTank energyTank,
            GameConstants.eFaction newOwnership)
        {
            var energyTankQuery =
                (from e in db_context.EnergyTanks
                    where e.EnergyTankID == energyTank.ID
                    select e).SingleOrDefault();

            energyTankQuery.Ownership = (int)newOwnership;

            db_context.SubmitChanges();

            energyTank.Faction = newOwnership;
        }
    }
}
