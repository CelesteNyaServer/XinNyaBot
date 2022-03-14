using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ActDieMoeDownloader.Utils
{
	public class HttpUtils
    {
        // SecurityProtocolType value
        // System default(0)
        // Ssl3(48 - 0x30)
        // Tls(192 - 0xC0)
        // Tls11(768 - 0x300) missing on Framework 4.0
        // Tls12(3072 - 0xC00) missing on Framework 4.0
        public static SecurityProtocolType TLS11 = (SecurityProtocolType)768;
        public static SecurityProtocolType TLS12 = (SecurityProtocolType)3072;

        public static string Post(string url, string content)
        {
            ServicePointManager.SecurityProtocol = TLS12;
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/json; charset=utf-8";

            #region 添加Post 参数
            byte[] data = Encoding.UTF8.GetBytes(content);
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            #endregion

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        public static string Get(string url, int Timeout)
        {
            ServicePointManager.SecurityProtocol = TLS12;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html,application/xhtml+xml,application/xml;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = Timeout;
            request.KeepAlive = true;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }
    }

    public interface IHttpClient
	{

	}
}
