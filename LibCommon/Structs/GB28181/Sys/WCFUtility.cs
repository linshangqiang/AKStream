namespace LibCommon.Structs.GB28181.Sys
{
    public class WCFUtility
    {
        public static bool DoesWCFServiceExist(string serviceName)
        {
            //ServiceModelSectionGroup serviceModelSectionGroup = ServiceModelSectionGroup.GetSectionGroup(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None));
            //foreach (ServiceElement serviceElement in serviceModelSectionGroup.Services.Services)
            //{
            //    if (serviceElement.Name == serviceName)
            //    {
            //        return true;
            //    }
            //}

            return false;
        }
    }
}