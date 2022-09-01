using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;

using Newtonsoft.Json.Linq;

namespace WSL.AIYISHENG.FIN.PLUGIN
{
    [Description("应收单发票号码作废执行计划")]
    public class FPCancelSchedule : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            this._ctx = ctx;
            this.ToInterface();
        }

		private void ToInterface()
		{
			List<CancelModel> data = this.GetData();
			bool flag = data.Count <= 0;
			if (!flag)
			{
				for (int i = 0; i < data.Count; i++)
				{
					this.dBLog = new DBLog();
					this.dBLog.FInvocation = "Kingdee";
					this.dBLog.FInterfaceType = "InvoiceCancel";
					this.dBLog.FBeginTime = DateTime.Now.ToString();
					this.dBLog.Context = this._ctx;
					StringBuilder stringBuilder = new StringBuilder();
					CancelModel cancelModel = data[i];
					stringBuilder.AppendLine("");
					stringBuilder.AppendLine("接口方向：Kingdee --> ");
					stringBuilder.AppendLine("接口名称：应收单发票号码作废API");
					try
					{
						this.Require(cancelModel, stringBuilder);
						this.UpdateStatus(cancelModel.fpNo);
						this.dBLog.FStatus = "S";
						this.dBLog.FMessage = "成功";
						Logger.Info("", stringBuilder.ToString());
					}
					catch (Exception ex)
					{
						this.dBLog.FStatus = "E";
						this.dBLog.FMessage = ex.Message;
						this.dBLog.FStackMessage = ex.ToString();
						stringBuilder.AppendLine("错误信息：" + ex.Message.ToString());
						Logger.Error("", stringBuilder.ToString(), ex);
					}
					finally
					{
						this.dBLog.Insert();
					}
				}
			}
		}

		private void Require(CancelModel item, StringBuilder sb)
		{
			string text = " https://oms.aijiangkj.com/aysdmp/base/deleteOrderInvoicePush.do";
			string text2 = string.Join(",", new string[]
			{
				item.fpNo
			});
			List<string> noList = item.fpNo.Split(',').ToList();

			var o = new
			{
				invoiceCodes = noList
			};
			sb.AppendLine("请求Url：" + text);
			string text3 = o.ToJSON();
			sb.AppendLine("请求信息：" + text3);
			this.dBLog.FOperationType = text;
			this.dBLog.FBillNo = text2;
			this.dBLog.FRequestMessage = text3;
			string text4 = ApiHelper.HttpRequest(text, text3, "POST", "application/json", null);
			sb.AppendLine("返回信息：" + text4);
			this.dBLog.FResponseMessage = text4;
			this.dBLog.FEndTime = DateTime.Now.ToString();
			JObject jobject = JObject.Parse(text4);
			bool flag = jobject["success"] != null;
			if (!flag)
			{
				throw new KDException("错误", text4);
			}
			bool flag2 = !Convert.ToBoolean(jobject["success"].ToString());
			if (flag2)
			{
				throw new KDException("错误", text4);
			}
		}

		private List<CancelModel> GetData()
		{
			List<CancelModel> list = new List<CancelModel>();
			string text = "\r\n                SELECT  DISTINCT \r\n                        F.FIVNUMBER\r\n                  FROM  T_AR_RECEIVABLE A\r\n                        INNER JOIN t_AR_receivableEntry B \r\n                        ON A.FID = B.FID\r\n                        INNER JOIN T_IV_SALESICENTRY_LK C\r\n                        ON C.FSID = B.FENTRYID AND C.FSBILLID = B.FID\r\n                        INNER JOIN T_IV_SALESICENTRY D\r\n                        ON C.FENTRYID = D.FENTRYID\r\n                        INNER JOIN T_IV_SALESIC E\r\n                        ON D.FID = E.FID\r\n\t\t\t\t\t\tINNER JOIN T_IV_SALESIC_O F\r\n                        ON F.FID = E.FID\r\n                 WHERE  1=1\r\n                   AND  ISNULL(B.FORDERNUMBER,'') <> '' --销售订单号不为空\r\n                   AND  ISNULL(F.FIVNUMBER,'') <> '' --销售发票上的发票号码不为空\r\n                   AND  A.FDocumentStatus = 'C' --应收单已审核\r\n                   AND  ISNULL(E.FFPStatus,'0') = '1' --销售发票上的传递状态为已传递\r\n                   AND  F.FGTSTATUS  = '2' --发票作废\r\n                   AND  ISNULL(E.FFPCancelStatus,'0')  = '0' --销售发票上的作废传递状态为未传递\r\n                ";
			DynamicObjectCollection dynamicObjectCollection = DBUtils.ExecuteDynamicObject(this._ctx, text, null, null, CommandType.Text, new SqlParam[0]);
			foreach (DynamicObject dynamicObject in dynamicObjectCollection)
			{
				list.Add(new CancelModel
				{
					fpNo = dynamicObject["FIVNUMBER"].ToString()
				});
			}
			return list;
		}

		private void UpdateStatus(string billNo)
		{
			string text = "/*dialect*/ \r\n                UPDATE  E\r\n                   SET  E.FFPCancelStatus = '1'  \r\n                  FROM  T_IV_SALESIC_O F\r\n                        INNER JOIN T_IV_SALESIC E\r\n                        ON E.FID = F.FID\r\n                 WHERE  F.FIVNUMBER = '" + billNo + "'";
			DBUtils.Execute(this._ctx, text);
		}

		// Token: 0x04000012 RID: 18
		private Context _ctx;

		// Token: 0x04000013 RID: 19
		private DBLog dBLog = new DBLog();
	}
}
