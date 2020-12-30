using LibCommon;
using LibCommon.Structs.DBModels;
using LibCommon.Structs.WebRequest;
using LibCommon.Structs.WebResponse;

namespace AKStreamWeb.Services
{
    public static class SystemService
    {
        /// <summary>
        /// 获取部门信息
        /// </summary>
        /// <param name="req"></param>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static ResGetDepartmentInfo GetDeptartmentInfoList(ReqGetDepartmentInfo req, out ResponseStruct rs)
        {
            rs = new ResponseStruct()
            {
                Code = ErrorNumber.None,
                Message = ErrorMessage.ErrorDic![ErrorNumber.None],
            };
            if (req == null)
            {
                rs = new ResponseStruct()
                {
                    Code = ErrorNumber.Sys_ParamsIsNotRight,
                    Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight],
                };
                return null;
            }

            if (req.IncludeSubDepartment != null && req.IncludeSubDepartment == true)
            {
                if (string.IsNullOrEmpty(req.DepartmentId) || !req.DepartmentId.ToLower().Trim().Equals("string"))
                {
                    rs = new ResponseStruct()
                    {
                        Code = ErrorNumber.Sys_ParamsIsNotRight,
                        Message = ErrorMessage.ErrorDic![ErrorNumber.Sys_ParamsIsNotRight] + ",条件要求包含下级部门数据，但部门代码条件为空",
                    };
                    return null;
                }
            }

            var ret = ORMHelper.Db.Select<VideoChannel>().Where("1=1").WhereIf(req.IncludeSubDepartment == true,
                    x => x.PDepartmentId.Equals(req.DepartmentId))
                .WhereIf(
                    (req.IncludeSubDepartment == null || req.IncludeSubDepartment == false) &&
                    !string.IsNullOrEmpty(req.DepartmentId), x => x.DepartmentId.Equals(req.DepartmentId))
                .ToList<DepartmentInfo>();
            return new ResGetDepartmentInfo()
            {
                DepartmentInfoList = ret,
            };
        }
    }
}