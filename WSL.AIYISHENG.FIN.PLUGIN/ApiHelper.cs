using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace WSL.AIYISHENG.FIN.PLUGIN
{
	public static class ApiHelper
	{
		// Token: 0x0600004C RID: 76 RVA: 0x000039F0 File Offset: 0x00001BF0
		public static string HttpRequest(string url, string data, string method = "PUT", string contentType = "application/json", Encoding encoding = null)
		{
			byte[] bytes = Encoding.GetEncoding("UTF-8").GetBytes(data);
			bool flag = encoding == null;
			if (flag)
			{
				encoding = Encoding.UTF8;
			}
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.Method = method;
			httpWebRequest.Timeout = 150000;
			httpWebRequest.AllowAutoRedirect = false;
			bool flag2 = !string.IsNullOrEmpty(contentType);
			if (flag2)
			{
				httpWebRequest.ContentType = contentType;
			}
			bool flag3 = url.StartsWith("https", StringComparison.OrdinalIgnoreCase);
			if (flag3)
			{
				ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ApiHelper.CheckValidationResult);
			}
			string result = null;
			try
			{
				bool flag4 = bytes != null;
				if (flag4)
				{
					httpWebRequest.ContentLength = (long)bytes.Length;
					Stream requestStream = httpWebRequest.GetRequestStream();
					requestStream.Write(bytes, 0, bytes.Length);
					requestStream.Close();
				}
				else
				{
					httpWebRequest.ContentLength = 0L;
				}
				using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
				{
					Stream responseStream = httpWebResponse.GetResponseStream();
					byte[] bytes2 = ApiHelper.ReadFully(responseStream);
					responseStream.Close();
					result = Encoding.UTF8.GetString(bytes2);
				}
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
			}
			return result;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00003B50 File Offset: 0x00001D50
		public static byte[] ReadFully(Stream stream)
		{
			byte[] array = new byte[512];
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				for (; ; )
				{
					int num = stream.Read(array, 0, array.Length);
					bool flag = num <= 0;
					if (flag)
					{
						break;
					}
					memoryStream.Write(array, 0, num);
				}
				result = memoryStream.ToArray();
			}
			return result;
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00003BC4 File Offset: 0x00001DC4
		private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
		{
			return true;
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00003BD8 File Offset: 0x00001DD8
		public static string HttpPost(string url, string body)
		{
			Encoding utf = Encoding.UTF8;
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.Method = "POST";
			httpWebRequest.Accept = "text/html, application/xhtml+xml, */*";
			httpWebRequest.ContentType = "application/json";
			byte[] bytes = utf.GetBytes(body);
			httpWebRequest.ContentLength = (long)bytes.Length;
			httpWebRequest.GetRequestStream().Write(bytes, 0, bytes.Length);
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			string result;
			using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8))
			{
				result = streamReader.ReadToEnd();
			}
			return result;
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00003C8C File Offset: 0x00001E8C
		public static string HttpPut(string url, string body)
		{
			Encoding utf = Encoding.UTF8;
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.Method = "PUT";
			httpWebRequest.Accept = "text/html, application/xhtml+xml, */*";
			httpWebRequest.ContentType = "application/json";
			byte[] bytes = utf.GetBytes(body);
			httpWebRequest.ContentLength = (long)bytes.Length;
			httpWebRequest.GetRequestStream().Write(bytes, 0, bytes.Length);
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			string result;
			using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8))
			{
				result = streamReader.ReadToEnd();
			}
			return result;
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00003D40 File Offset: 0x00001F40
		public static string HttpGet(string url, Dictionary<string, object> dic, out int count)
		{
			string result = "";
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(url);
			bool flag = dic.Count > 0;
			if (flag)
			{
				stringBuilder.Append("?");
				int num = 0;
				foreach (KeyValuePair<string, object> keyValuePair in dic)
				{
					bool flag2 = num > 0;
					if (flag2)
					{
						stringBuilder.Append("&");
					}
					stringBuilder.AppendFormat("{0}={1}", keyValuePair.Key, keyValuePair.Value);
					num++;
				}
			}
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(stringBuilder.ToString());
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			WebHeaderCollection headers = httpWebResponse.Headers;
			count = Convert.ToInt32(headers["X-Total-Count"]);
			Stream responseStream = httpWebResponse.GetResponseStream();
			try
			{
				using (StreamReader streamReader = new StreamReader(responseStream))
				{
					result = streamReader.ReadToEnd();
				}
			}
			finally
			{
				responseStream.Close();
			}
			return result;
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00003E8C File Offset: 0x0000208C
		private static CspParameters GetCspKey()
		{
			return new CspParameters
			{
				KeyContainerName = "chait"
			};
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00003EB0 File Offset: 0x000020B0
		public static string Encrypt(string palinData)
		{
			bool flag = string.IsNullOrWhiteSpace(palinData);
			string result;
			if (flag)
			{
				result = null;
			}
			else
			{
				using (RSACryptoServiceProvider rsacryptoServiceProvider = new RSACryptoServiceProvider(ApiHelper.GetCspKey()))
				{
					byte[] bytes = Encoding.UTF8.GetBytes(palinData);
					byte[] inArray = rsacryptoServiceProvider.Encrypt(bytes, false);
					result = Convert.ToBase64String(inArray);
				}
			}
			return result;
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00003F14 File Offset: 0x00002114
		public static string RSAEncrypt(string publickey, string content)
		{
			RSACryptoServiceProvider rsacryptoServiceProvider = new RSACryptoServiceProvider();
			rsacryptoServiceProvider.FromXmlString(publickey);
			byte[] inArray = rsacryptoServiceProvider.Encrypt(Encoding.UTF8.GetBytes(content), false);
			return Convert.ToBase64String(inArray);
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00003F50 File Offset: 0x00002150
		public static string GetPasswordSalt()
		{
			byte[] array = new byte[16];
			using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
			{
				randomNumberGenerator.GetBytes(array);
			}
			return Convert.ToBase64String(array);
		}
	}
}
