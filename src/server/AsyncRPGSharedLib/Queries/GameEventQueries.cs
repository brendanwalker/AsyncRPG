using System;
using System.Collections.Generic;
using System.Linq;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Protocol;
using LitJson;

namespace AsyncRPGSharedLib.Queries
{
    public class GameEventQueries
    {
        public static void AddEvent<T>(
            AsyncRPGDataContext context,
            int game_id,
            T event_parameters) where T : GameEventParameters
        {
            // Post a new event to the events table
            {
                GameEvents dbGameEvent = new GameEvents
                {
                    GameID = game_id,
                    EventTime = DateTime.Now,
                    EventType = (int)event_parameters.GetEventType(),
                    JsonParameters = SerializeParametersByType(event_parameters)
                };

                context.GameEvents.InsertOnSubmit(dbGameEvent);
            }

            // Tell all Characters associated with the game that some new events got posted
            {
                var dbCharacterQuery =
                    from c in context.Characters
                    where c.GameID == game_id
                    select c;

                foreach (Characters dbCharacter in dbCharacterQuery)
                {
                    dbCharacter.NewEventsPosted = true;
                }
            }

            context.SubmitChanges();
        }


        public static void GetGameEventsAfterId(
            AsyncRPGDataContext context,
            int game_id,
            int last_event_id,
            out GameEvent[] event_list,
            out int new_last_event_id)
        {
            List<GameEvent> temp_event_list = new List<GameEvent>();

            var eventQuery =
                (from e in context.GameEvents
                 where e.EventID > last_event_id && e.GameID == game_id
                 select new { e.EventID, e.EventTime, e.EventType, e.JsonParameters }).Take(20);

            new_last_event_id = last_event_id;

            foreach (var dbEvent in eventQuery)
            {
                GameEvent gameEvent = new GameEvent();

                new_last_event_id = Math.Max(new_last_event_id, dbEvent.EventID);

                gameEvent.timestamp = dbEvent.EventTime.Ticks / 10000; // Convert Ticks (100ns intervals to ms)
                gameEvent.event_type = (eGameEventType)dbEvent.EventType;
                gameEvent.parameters = DeserializeParametersByType(gameEvent.event_type, dbEvent.JsonParameters);

                temp_event_list.Add(gameEvent);
            }

            event_list = temp_event_list.ToArray();
        }

        public static int GetLastGameEvent(
            AsyncRPGDataContext context,
            int game_id)
        {
            return (from e in context.GameEvents
                    where e.GameID == game_id
                    orderby e.EventID descending
                    select e.EventID).First();
        }

        public static void DeleteAllEventsForGame(
            AsyncRPGDataContext context,
            int game_id)
        {
            var query=
                from game_event in context.GameEvents
                where game_event.GameID == game_id
                select game_event;

            foreach (GameEvents game_event in query)
            {
                context.GameEvents.DeleteOnSubmit(game_event);
            }

            context.SubmitChanges();
        }

        public static GameEventParameters DeserializeParametersByType(eGameEventType eventType, string jsonString)
        {
            // HACKY COMPILE ASSERT
            // If you hit this, the number of game events changed and you need to update the below list
            {
                byte compile_assert = ((int)eGameEventType.k_game_event_type_count == 17) ? 0 : -1;

                compile_assert++;
            }

            switch (eventType)
            {
                case eGameEventType.character_joined_game:
                    return DeserializeParameters<GameEvent_CharacterJoinedGame>(jsonString);
                case eGameEventType.character_left_game:
                    return DeserializeParameters<GameEvent_CharacterLeftGame>(jsonString);
                case eGameEventType.character_died:
                    return DeserializeParameters<GameEvent_CharacterDied>(jsonString);
                case eGameEventType.character_moved:
                    return DeserializeParameters<GameEvent_CharacterMoved>(jsonString);
                case eGameEventType.character_portaled:
                    return DeserializeParameters<GameEvent_CharacterPortaled>(jsonString);
                case eGameEventType.character_attacked:
                    return DeserializeParameters<GameEvent_CharacterAttacked>(jsonString);
                case eGameEventType.mob_spawned:
                    return DeserializeParameters<GameEvent_MobSpawned>(jsonString);
                case eGameEventType.mob_died:
                    return DeserializeParameters<GameEvent_MobDied>(jsonString);
                case eGameEventType.mob_moved:
                    return DeserializeParameters<GameEvent_MobMoved>(jsonString);
                case eGameEventType.mob_attacked:
                    return DeserializeParameters<GameEvent_MobAttacked>(jsonString);
                case eGameEventType.mob_dialog:
                    return DeserializeParameters<GameEvent_MobDialog>(jsonString);
                case eGameEventType.mob_player_prop_spotted:
                    return DeserializeParameters<GameEvent_MobPlayerPropSpotted>(jsonString);
                case eGameEventType.mob_player_prop_lost_track:
                    return DeserializeParameters<GameEvent_MobPlayerPropLostTrack>(jsonString);
                case eGameEventType.mob_ai_prop_spotted:
                    return DeserializeParameters<GameEvent_MobAIPropSpotted>(jsonString);
                case eGameEventType.mob_energy_tank_prop_spotted:
                    return DeserializeParameters<GameEvent_MobEnergyTankPropSpotted>(jsonString);
                case eGameEventType.energy_tank_drained:
                    return DeserializeParameters<GameEvent_EnergyTankDrained>(jsonString);
                case eGameEventType.energy_tank_hacked:
                    return DeserializeParameters<GameEvent_EnergyTankHacked>(jsonString);
                default:
                    throw new Exception(string.Format("Unhandled event type: {0}", eventType.ToString()));
            }
        }

        public static string SerializeParametersByType(GameEventParameters jsonObject)
        {
            // HACKY COMPILE ASSERT
            // If you hit this, the number of game events changed and you need to update the below list
            {
                byte compile_assert = ((int)eGameEventType.k_game_event_type_count == 17) ? 0 : -1;

                compile_assert++;
            }

            switch (jsonObject.GetEventType())
            {
                case eGameEventType.character_joined_game:
                    return SerializeParameters<GameEvent_CharacterJoinedGame>((GameEvent_CharacterJoinedGame)jsonObject);
                case eGameEventType.character_left_game:
                    return SerializeParameters<GameEvent_CharacterLeftGame>((GameEvent_CharacterLeftGame)jsonObject);
                case eGameEventType.character_died:
                    return SerializeParameters<GameEvent_CharacterDied>((GameEvent_CharacterDied)jsonObject);
                case eGameEventType.character_moved:
                    return SerializeParameters<GameEvent_CharacterMoved>((GameEvent_CharacterMoved)jsonObject);
                case eGameEventType.character_portaled:
                    return SerializeParameters<GameEvent_CharacterPortaled>((GameEvent_CharacterPortaled)jsonObject);
                case eGameEventType.character_attacked:
                    return SerializeParameters<GameEvent_CharacterAttacked>((GameEvent_CharacterAttacked)jsonObject);
                case eGameEventType.mob_spawned:
                    return SerializeParameters<GameEvent_MobSpawned>((GameEvent_MobSpawned)jsonObject);
                case eGameEventType.mob_died:
                    return SerializeParameters<GameEvent_MobDied>((GameEvent_MobDied)jsonObject);
                case eGameEventType.mob_moved:
                    return SerializeParameters<GameEvent_MobMoved>((GameEvent_MobMoved)jsonObject);
                case eGameEventType.mob_attacked:
                    return SerializeParameters<GameEvent_MobAttacked>((GameEvent_MobAttacked)jsonObject);
                case eGameEventType.mob_dialog:
                    return SerializeParameters<GameEvent_MobDialog>((GameEvent_MobDialog)jsonObject);
                case eGameEventType.mob_player_prop_spotted:
                    return SerializeParameters<GameEvent_MobPlayerPropSpotted>((GameEvent_MobPlayerPropSpotted)jsonObject);
                case eGameEventType.mob_player_prop_lost_track:
                    return SerializeParameters<GameEvent_MobPlayerPropLostTrack>((GameEvent_MobPlayerPropLostTrack)jsonObject);
                case eGameEventType.mob_ai_prop_spotted:
                    return SerializeParameters<GameEvent_MobAIPropSpotted>((GameEvent_MobAIPropSpotted)jsonObject);
                case eGameEventType.mob_energy_tank_prop_spotted:
                    return SerializeParameters<GameEvent_MobEnergyTankPropSpotted>((GameEvent_MobEnergyTankPropSpotted)jsonObject);
                case eGameEventType.energy_tank_drained:
                    return SerializeParameters<GameEvent_EnergyTankDrained>((GameEvent_EnergyTankDrained)jsonObject);
                case eGameEventType.energy_tank_hacked:
                    return SerializeParameters<GameEvent_EnergyTankHacked>((GameEvent_EnergyTankHacked)jsonObject);
                default:
                    throw new Exception(string.Format("Unhandled event type: {0}", jsonObject.GetEventType().ToString()));
            }
        }

        private static T DeserializeParameters<T>(string jsonString) where T : GameEventParameters
        {
            T jsonObject = null;

            try
            {
                if (jsonString.Length == 0)
                {
                    throw new ArgumentException();
                }

                jsonObject = JsonMapper.ToObject<T>(jsonString);
            }
            catch (System.Exception)
            {
                jsonObject = null;
            }

            return jsonObject;
        }

        private static string SerializeParameters<T>(T jsonObject) where T : GameEventParameters
        {
            return JsonMapper.ToJson(jsonObject);
        }
    }
}