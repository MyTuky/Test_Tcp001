using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Test_Tcp001
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                ShowUsage();
            }
            Task<string> t1 = RequestHtmlAsync(args[0]);
            Console.WriteLine(t1.Result);
            Console.ReadLine();
        }
        private static void ShowUsage()
        {
            Console.WriteLine("Usage:HttpClientUsingTcp hostname");
        }
        private const int ReadBufferSide = 1024;
        /// <summary>
        /// 最关键的一个方法！！！
        /// 传入主机名，请求HTTP回应
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        public static async Task<string> RequestHtmlAsync(string hostname)
        {
            try
            {
                using (var client = new TcpClient())//创建一个TCP客户端对象
                {
                    await client.ConnectAsync(hostname, 80);//在HTTP默认接口上建立TCP连接
                    NetworkStream stream = client.GetStream();//通过客户端对象获取一个流，用来传输数据
                    //要传输到HTTP的协议内容
                    string header = "GET / HTTP/1.1\r\n" +
                        $"Host: {hostname}:80\r\n"+
                            "Connection: close\r\n" +
                            "\r\n";
                    byte[] buffer = Encoding.UTF8.GetBytes(header);
                    await stream.WriteAsync(buffer, 0, buffer.Length);//将数据写到流中，准备发送，一般会先放在本地缓存之后才发送
                    await stream.FlushAsync();//立即向服务器发送数据，不在本地缓存
                    var ms = new MemoryStream();
                    buffer = new byte[ReadBufferSide];
                    int read = 0;
                    do
                    {
                        read = await stream.ReadAsync(buffer, 0, ReadBufferSide);
                        ms.Write(buffer, 0, read);
                        Array.Clear(buffer, 0, buffer.Length);
                    } while (read > 0);
                    ms.Seek(0, SeekOrigin.Begin);
                    var reader = new StreamReader(ms);
                    return reader.ReadToEnd();
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
