using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReactSpa.Utils
{
    public class SendinBlue
    {
        public static string BaseUrl = "https://api.sendinblue.com/v2.0/";
        public static string AccessId = "ZxL7JOWvIwrUbC8S";
        public static int Timeout = 30000;

        private static async Task<dynamic> AuthCall(string resource, HttpMethod method, string content)
        {
            const string contentType = "application/json";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("api-key", AccessId);
                client.BaseAddress = new Uri(BaseUrl);
                client.Timeout = TimeSpan.FromMilliseconds(Timeout);
                client.DefaultRequestHeaders.Accept.Add(new
                    MediaTypeWithQualityHeaderValue(contentType));
                HttpRequestMessage msg = new HttpRequestMessage(method, resource);
                msg.Content = new StringContent(content, Encoding.UTF8, contentType);
                var res =  await client.SendAsync(msg);
                return res.Content.ReadAsStringAsync();
            }
        }

        private static async Task<dynamic> PostRequest(string resource, string content)
        {
            return await AuthCall(resource, HttpMethod.Post, content);
        }

        public static async Task<dynamic> SendMail(Object data)
        {
            return await PostRequest("email", JsonConvert.SerializeObject(data));
        }
    }
}