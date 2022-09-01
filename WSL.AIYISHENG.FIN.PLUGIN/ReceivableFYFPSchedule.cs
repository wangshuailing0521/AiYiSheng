using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
	[Description("应收单返回费用发票号码执行计划")]
	public class ReceivableFYFPSchedule : IScheduleService
	{
		private Context _ctx;

		private DBLog dBLog = new DBLog();

		public void Run(Context ctx, Schedule schedule)
		{
			_ctx = ctx;
			ToInterface();
		}

		private void ToInterface()
		{
			List<FPModel> data = GetData();
			if (data.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < data.Count; i++)
			{
				dBLog = new DBLog();
				dBLog.FInvocation = "Kingdee";
				dBLog.FInterfaceType = "FYInvoiceNoReturn";
				dBLog.FBeginTime = DateTime.Now.ToString();
				dBLog.Context = _ctx;
				StringBuilder stringBuilder = new StringBuilder();
				FPModel fPModel = data[i];
				stringBuilder.AppendLine("");
				stringBuilder.AppendLine("接口方向：Kingdee --> ");
				stringBuilder.AppendLine("接口名称：应收单返回费用发票号码API");
				try
				{
					Require(fPModel, stringBuilder);
					UpdateStatus(fPModel.ReceivableNo);
					dBLog.FStatus = "S";
					dBLog.FMessage = "成功";
					Logger.Info("", stringBuilder.ToString());
				}
				catch (Exception ex)
				{
					dBLog.FStatus = "E";
					dBLog.FMessage = ex.Message;
					dBLog.FStackMessage = ex.ToString();
					stringBuilder.AppendLine("错误信息：" + ex.Message.ToString());
					Logger.Error("", stringBuilder.ToString(), ex);
				}
				finally
				{
					dBLog.Insert();
				}
			}
		}

		private void Require(FPModel item, StringBuilder sb)
		{
			string text = " https://oms.aijiangkj.com/aysdmp/base/processOrderInvoicePush.do";
			string orderNo = item.OrderNo;
			string invoiceCode = string.Join(",", item.FPNo);
			var o = new
			{
				invoiceCode = invoiceCode,
				orderCodes = new List<string> { orderNo },
				invoiceType = "2"
			};
			sb.AppendLine("请求Url：" + text);
			string text2 = o.ToJSON();
			sb.AppendLine("请求信息：" + text2);
			dBLog.FOperationType = text;
			dBLog.FBillNo = orderNo;
			dBLog.FRequestMessage = text2;
			string text3 = ApiHelper.HttpRequest(text, text2, "POST");
			sb.AppendLine("返回信息：" + text3);
			dBLog.FResponseMessage = text3;
			dBLog.FEndTime = DateTime.Now.ToString();
			JObject val = JObject.Parse(text3);
			if (val["success"] != null)
			{
				if (!Convert.ToBoolean(((object)val["success"]).ToString()))
				{
					throw new KDException("错误", text3);
				}
				return;
			}
			throw new KDException("错误", text3);
		}

		private List<FPModel> GetData()
		{
			List<FPModel> list = new List<FPModel>();
			string text = "";
			text = "\r\n                SELECT  DISTINCT \r\n                        B.FORDERNUMBER\r\n                       ,A.FBILLNO\r\n                       --,E.FIVNUMBER\r\n                  FROM  T_AR_RECEIVABLE A\r\n                        INNER JOIN t_AR_receivableEntry B \r\n                        ON A.FID = B.FID\r\n                        INNER JOIN T_IV_SALEEXINVENTRY_LK C\r\n                        ON C.FSID = B.FENTRYID AND C.FSBILLID = B.FID\r\n                        INNER JOIN T_IV_SALEEXINVENTRY D\r\n                        ON C.FENTRYID = D.FENTRYID\r\n                        INNER JOIN T_IV_SALEEXINV E\r\n                        ON D.FID = E.FID\r\n                 WHERE  1=1\r\n                   AND  ISNULL(B.FORDERNUMBER,'') <> '' --销售订单号不为空\r\n                   AND  ISNULL(E.FIVNUMBER,'') <> '' --销售发票上的发票号码不为空\r\n                   AND  A.FDocumentStatus = 'C' --应收单已审核\r\n                   AND  ISNULL(E.FFPStatus,'0') = '0' --销售发票上的传递状态为未传递\r\n                   AND  A.FBILLTYPEID  = '00505694799c955111e325bc9e6eb056'  --费用应收单\r\n                   AND  A.FCANCELSTATUS  = 'A' \r\n                ";
			DynamicObjectCollection dynamicObjectCollection = DBUtils.ExecuteDynamicObject(_ctx, text, null, null, CommandType.Text);
			foreach (DynamicObject item in dynamicObjectCollection)
			{
				FPModel fPModel = new FPModel();
				fPModel.OrderNo = item["FORDERNUMBER"].ToString();
				fPModel.ReceivableNo = item["FBILLNO"].ToString();
				fPModel.FPNo = new List<string>();
				list.Add(fPModel);
			}
			foreach (FPModel item2 in list)
			{
				text = "\r\n                SELECT  DISTINCT \r\n                        B.FORDERNUMBER\r\n                       ,A.FBILLNO\r\n                       ,E.FIVNUMBER\r\n                  FROM  T_AR_RECEIVABLE A\r\n                        INNER JOIN t_AR_receivableEntry B \r\n                        ON A.FID = B.FID\r\n                        INNER JOIN T_IV_SALEEXINVENTRY_LK C\r\n                        ON C.FSID = B.FENTRYID AND C.FSBILLID = B.FID\r\n                        INNER JOIN T_IV_SALEEXINVENTRY D\r\n                        ON C.FENTRYID = D.FENTRYID\r\n                        INNER JOIN T_IV_SALEEXINV E\r\n                        ON D.FID = E.FID\r\n                 WHERE  1=1\r\n                   AND  ISNULL(B.FORDERNUMBER,'') <> '' --销售订单号不为空\r\n                   AND  ISNULL(E.FIVNUMBER,'') <> '' --销售发票上的发票号码不为空\r\n                   AND  A.FDocumentStatus = 'C' --应收单已审核\r\n                   AND  B.FORDERNUMBER = '" + item2.OrderNo + "'\r\n                   AND  A.FBILLNO = '" + item2.ReceivableNo + "'\r\n                ";
				DynamicObjectCollection dynamicObjectCollection2 = DBUtils.ExecuteDynamicObject(_ctx, text, null, null, CommandType.Text);
				foreach (DynamicObject item3 in dynamicObjectCollection2)
				{
					item2.FPNo.Add(item3["FIVNUMBER"].ToString());
				}
			}
			return list;
		}

		private void UpdateStatus(string billNo)
		{
			string strSQL = "/*dialect*/ \r\n                UPDATE  E\r\n                   SET  E.FFPStatus = '1'  \r\n                  FROM  T_AR_RECEIVABLE A\r\n                        INNER JOIN t_AR_receivableEntry B \r\n                        ON A.FID = B.FID\r\n                        INNER JOIN T_IV_SALEEXINVENTRY_LK C\r\n                        ON C.FSID = B.FENTRYID AND C.FSBILLID = B.FID\r\n                        INNER JOIN T_IV_SALEEXINVENTRY D\r\n                        ON C.FENTRYID = D.FENTRYID\r\n                        INNER JOIN T_IV_SALEEXINV E\r\n                        ON D.FID = E.FID\r\n                 WHERE  A.FBILLNO = '" + billNo + "'";
			DBUtils.Execute(_ctx, strSQL);
		}
	}

}
