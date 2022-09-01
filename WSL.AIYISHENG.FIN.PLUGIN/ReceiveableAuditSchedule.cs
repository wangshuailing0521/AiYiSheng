using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;

using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;

namespace WSL.AIYISHENG.FIN.PLUGIN
{
	[Description("应收单自动审核执行计划")]
	public class ReceiveableAuditSchedule : IScheduleService
	{
		private Context _ctx;

		public void Run(Context ctx, Schedule schedule)
		{
			_ctx = ctx;
			Execute();
		}

		private void Execute()
		{
			DynamicObjectCollection data = GetData();
			if (data.Count <= 0)
			{
				return;
			}
			FormMetadata formMetadata = ServiceHelper.GetService<IMetaDataService>().Load(_ctx, "AR_receivable") as FormMetadata;
			BusinessInfo businessInfo = formMetadata.BusinessInfo;
			for (int i = 0; i < data.Count; i++)
			{
				DynamicObject dynamicObject = data[i];
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("");
				stringBuilder.AppendLine("应收单自动审核执行计划");
				try
				{
					stringBuilder.AppendLine(dynamicObject["FBILLNO"].ToString());
					string key = dynamicObject["FID"].ToString();
					List<KeyValuePair<object, object>> list = new List<KeyValuePair<object, object>>();
					list.Add(new KeyValuePair<object, object>(key, ""));
					AuditBill(businessInfo, list);
				}
				catch (Exception ex)
				{
					stringBuilder.AppendLine("错误信息：" + ex.Message.ToString());
					Logger.Error("", stringBuilder.ToString(), ex);
				}
			}
		}

		private void AuditBill(BusinessInfo businessInfo, List<KeyValuePair<object, object>> pkIds)
		{
			List<object> list = new List<object>();
			foreach (KeyValuePair<object, object> pkId in pkIds)
			{
				list.Add(Convert.ToInt64(pkId.Key));
			}
			IOperationResult operationResult = ServiceHelper.GetService<ISubmitService>().Submit(_ctx, businessInfo, list.ToArray(), "Submit");
			List<object> list2 = new List<object>();
			list2.Add("1");
			list2.Add("");
			IOperationResult operationResult2 = ServiceHelper.GetService<ISetStatusService>().SetBillStatus(_ctx, businessInfo, pkIds, list2, "Audit", OperateOption.Create());
		}

		private DynamicObjectCollection GetData()
		{
			string strSQL = "\r\n                SELECT   \r\n                        A.FBILLNO\r\n                       ,A.FID\r\n                  FROM  T_AR_RECEIVABLE A\r\n                 WHERE  1=1\r\n                   AND  A.FDocumentStatus = 'A'";
			return DBUtils.ExecuteDynamicObject(_ctx, strSQL, null, null, CommandType.Text);
		}
	}

}
