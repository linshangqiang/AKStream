using System.Collections.Generic;
using AKStreamKeeper.Services;
using LibCommon;
using LibCommon.Structs;
using LibCommon.Structs.WebResponse;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AKStreamKeeper.Controllers
{
    [ApiController]
    [Route("/CutMergeService")]
    [SwaggerTag("裁剪与合并视频相关接口")]
    public class CutMergeServiceController : ControllerBase
    {
        /// <summary>
        /// 获取合并裁剪任务积压列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBacklogTaskList")]
        public List<CutMergeTaskStatusResponse> GetBacklogTaskList()
        {
            ResponseStruct rs;
            var ret = CutMergeService.GetBacklogTaskList(out rs);
            if (rs.Code != ErrorNumber.None)
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }


        /// <summary>
        /// 获取合并剪辑任务的进度信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetMergeTaskStatus")]
        public CutMergeTaskStatusResponse GetMergeTaskStatus(string taskId)
        {
            ResponseStruct rs;
            var ret = CutMergeService.GetMergeTaskStatus(taskId, out rs);
            if (rs.Code != ErrorNumber.None)
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }

        /// <summary>
        /// 添加一个裁剪合并任务
        /// </summary>
        /// <param name="rcmv"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Route("AddCutOrMergeTask")]
        [HttpPost]
        public CutMergeTaskResponse AddCutOrMergeTask(CutMergeTask task)
        {
            ResponseStruct rs;
            var ret = CutMergeService.AddCutOrMergeTask(task, out rs);
            if (rs.Code != ErrorNumber.None)
            {
                throw new AkStreamException(rs);
            }

            return ret;
        }
    }
}