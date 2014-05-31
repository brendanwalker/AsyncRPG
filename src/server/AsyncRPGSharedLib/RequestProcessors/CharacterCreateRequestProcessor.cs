using System;
using System.Linq;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Utility;
using AsyncRPGSharedLib.Queries;

namespace AsyncRPGSharedLib.RequestProcessors
{
    public class CharacterCreateRequestProcessor : RequestProcessor
    {
        // Request Data
        int m_account_id;
        string m_name;
        GameConstants.eGender m_gender;
        GameConstants.eArchetype m_archetype;
        int m_picture_id;

        // Result
        int[] m_characterIDs;

        public CharacterCreateRequestProcessor(
            int account_id,
            string name,
            GameConstants.eGender gender,
            GameConstants.eArchetype archetype,
            int picture_id)
        {
            m_account_id = account_id;
            m_name = name;
            m_gender = gender;
            m_archetype = archetype;
            m_picture_id = picture_id;
        }

        public int[] AccountCharacterIDs
        {
            get { return m_characterIDs; }
        }

        protected override bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            int archetypeCount = EnumUtilities.GetEnumValues<GameConstants.eArchetype>().Count();

            CharacterQueries.CreateCharacter(
                requestCache.DatabaseContext,
                m_account_id,
                m_name,
                m_gender,
                m_archetype,
                m_picture_id);

            CharacterQueries.GetCharacterIDList(
                requestCache.DatabaseContext,
                m_account_id, 
                out m_characterIDs);

            result_code = SuccessMessages.GENERAL_SUCCESS;

            return true;
        }
    }
}
