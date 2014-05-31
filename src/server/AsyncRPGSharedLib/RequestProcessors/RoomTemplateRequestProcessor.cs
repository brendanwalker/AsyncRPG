using System;
using System.Text;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Common;


namespace AsyncRPGSharedLib.RequestProcessors
{
    public class RoomTemplateRequestProcessor : RequestProcessor
    {
        public StringBuilder RoomTemplateReport { get; private set; }

        public RoomTemplateRequestProcessor()
        {
            RoomTemplateReport = new StringBuilder();
        }

        protected override bool ProcessRequestInternal(
            RequestCache requestCache,
            out string result_code)
        {
            RoomTemplateSet roomTemplateSet= 
                WorldBuilderCache.GetWorldBuilder(requestCache.DatabaseContext, requestCache.SessionCache).RoomTemplates;

            foreach (RoomTemplate roomTemplate in roomTemplateSet.RoomTemplateDictionary.Values)
            {
                RoomTemplateReport.AppendFormat("[{0}]\n", roomTemplate.TemplateName);
                roomTemplate.NavMeshTemplate.ToStringData(RoomTemplateReport);
            }

            result_code = SuccessMessages.GENERAL_SUCCESS;

            return true;
        }
    }
}
