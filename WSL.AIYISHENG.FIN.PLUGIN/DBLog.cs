using System;

using Kingdee.BOS;
using Kingdee.BOS.App.Data;

namespace WSL.AIYISHENG.FIN.PLUGIN
{
	public class DBLog
	{
		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000010 RID: 16 RVA: 0x0000229C File Offset: 0x0000049C
		// (set) Token: 0x06000011 RID: 17 RVA: 0x000022A4 File Offset: 0x000004A4
		public string FInvocation { get; set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000012 RID: 18 RVA: 0x000022AD File Offset: 0x000004AD
		// (set) Token: 0x06000013 RID: 19 RVA: 0x000022B5 File Offset: 0x000004B5
		public string FInterfaceType { get; set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000014 RID: 20 RVA: 0x000022BE File Offset: 0x000004BE
		// (set) Token: 0x06000015 RID: 21 RVA: 0x000022C6 File Offset: 0x000004C6
		public string FOperationType { get; set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000016 RID: 22 RVA: 0x000022CF File Offset: 0x000004CF
		// (set) Token: 0x06000017 RID: 23 RVA: 0x000022D7 File Offset: 0x000004D7
		public string FEndTime { get; set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000018 RID: 24 RVA: 0x000022E0 File Offset: 0x000004E0
		// (set) Token: 0x06000019 RID: 25 RVA: 0x000022E8 File Offset: 0x000004E8
		public string FRequestMessage { get; set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600001A RID: 26 RVA: 0x000022F1 File Offset: 0x000004F1
		// (set) Token: 0x0600001B RID: 27 RVA: 0x000022F9 File Offset: 0x000004F9
		public string FResponseMessage { get; set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600001C RID: 28 RVA: 0x00002302 File Offset: 0x00000502
		// (set) Token: 0x0600001D RID: 29 RVA: 0x0000230A File Offset: 0x0000050A
		public string FStatus { get; set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600001E RID: 30 RVA: 0x00002313 File Offset: 0x00000513
		// (set) Token: 0x0600001F RID: 31 RVA: 0x0000231B File Offset: 0x0000051B
		public string FMessage { get; set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000020 RID: 32 RVA: 0x00002324 File Offset: 0x00000524
		// (set) Token: 0x06000021 RID: 33 RVA: 0x0000232C File Offset: 0x0000052C
		public string FStackMessage { get; set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000022 RID: 34 RVA: 0x00002335 File Offset: 0x00000535
		// (set) Token: 0x06000023 RID: 35 RVA: 0x0000233D File Offset: 0x0000053D
		public string FBillNo { get; set; }

		// Token: 0x06000024 RID: 36 RVA: 0x00002348 File Offset: 0x00000548
		public void Insert()
		{
			bool flag = this.FStackMessage == null;
			if (flag)
			{
				this.FStackMessage = "";
			}
			bool flag2 = this.FMessage == null;
			if (flag2)
			{
				this.FMessage = "";
			}
			bool flag3 = this.FRequestMessage == null;
			if (flag3)
			{
				this.FRequestMessage = "";
			}
			bool flag4 = string.IsNullOrEmpty(this.FBillNo);
			if (flag4)
			{
				this.FBillNo = "";
			}
			string text = string.Concat(new string[]
			{
				"/*dialect*/\r\n                            INSERT INTO SCS_T_InterfaceLog\r\n                            (FInvocation,FInterfaceType,FOperationType,FBeginTime,\r\n                            FEndTime,FRequestMessage,FResponseMessage,FStatus,\r\n                            FMessage,FStackMessage,FBillNo)\r\n                            SELECT \r\n                            '",
				this.FInvocation,
				"','",
				this.FInterfaceType,
				"','",
				this.FOperationType,
				"','",
				this.FBeginTime,
				"',\r\n                            '",
				this.FEndTime,
				"','",
				this.FRequestMessage.Replace("'", "''"),
				"','",
				this.FResponseMessage,
				"','",
				this.FStatus,
				"',\r\n                            '",
				this.FMessage.Replace("'", "''"),
				"','",
				this.FStackMessage.Replace("'", "''"),
				"',\r\n                            '",
				this.FBillNo,
				"'"
			});
			DBUtils.Execute(this.Context, text);
		}

		// Token: 0x04000006 RID: 6
		public Context Context;

		// Token: 0x0400000A RID: 10
		public string FBeginTime = DateTime.Now.ToString();
	}
}
