using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Text;

namespace MqttNetServer
{
    class Program
    {
        public static IMqttServer mqttServer;
        static void Main(string[] args)
        {
            StartMqttServer();
        }

        //启动Mqtt服务器
        private static async void StartMqttServer()
        {
            try
            {
                //验证客户端信息
                var options = new MqttServerOptions
                {
                    //连接验证
                    ConnectionValidator = new MqttServerConnectionValidatorDelegate(p =>
                    {
                        if (p.ClientId == "SpecialClient")
                        {
                            if (p.Username != "USER" || p.Password != "PASS")
                            {
                                p.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                            }
                        }
                    })
                };

                //设置端口号
                options.DefaultEndpointOptions.Port = 8048;

                //创建Mqtt服务器
                mqttServer = new MqttFactory().CreateMqttServer();

                //开启订阅事件
                mqttServer.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate(MqttNetServer_SubscribedTopic);

                //取消订阅事件
                mqttServer.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(MqttNetServer_UnSubscribedTopic);

                //客户端消息事件
                mqttServer.UseApplicationMessageReceivedHandler(MqttServe_ApplicationMessageReceived);

                //客户端连接事件
                mqttServer.UseClientConnectedHandler(MqttNetServer_ClientConnected);

                //客户端断开事件
                mqttServer.UseClientDisconnectedHandler(MqttNetServer_ClientDisConnected);

                //启动服务器
                await mqttServer.StartAsync(options);

                Console.WriteLine("服务器启动成功！输入任意内容并回车停止服务！");
                Console.ReadLine();

                await mqttServer.StopAsync();
            }
            catch (Exception e)
            {
                Console.Write($"服务器启动失败 Msg：{e}");
            }
        }

        /// <summary>
        /// 客户订阅
        /// </summary>
        /// <param name="e"></param>
        private static void MqttNetServer_SubscribedTopic(MqttServerClientSubscribedTopicEventArgs e)
        {
            //客户端Id
            var ClientId = e.ClientId;
            var Topic = e.TopicFilter.Topic;
            Console.WriteLine($"客户端[{ClientId}]已订阅主题：{Topic}");
        }

        /// <summary>
        /// 客户取消订阅
        /// </summary>
        /// <param name="e"></param>
        private static void MqttNetServer_UnSubscribedTopic(MqttServerClientUnsubscribedTopicEventArgs e)
        {
            //客户端Id
            var ClientId = e.ClientId;
            var Topic = e.TopicFilter;
            Console.WriteLine($"客户端[{ClientId}]已取消订阅主题：{Topic}");
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="e"></param>
        private static void MqttServe_ApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            var ClientId = e.ClientId;
            var Topic = e.ApplicationMessage.Topic;
            var Payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            var Qos = e.ApplicationMessage.QualityOfServiceLevel;
            var Retain = e.ApplicationMessage.Retain;
            Console.WriteLine($"客户端[{ClientId}]>> 主题：[{Topic}] 负载：[{Payload}] Qos：[{Qos}] 保留：[{Retain}]");
        }

        /// <summary>
        /// 客户连接
        /// </summary>
        /// <param name="e"></param>
        private static void MqttNetServer_ClientConnected(MqttServerClientConnectedEventArgs e)
        {
            var ClientId = e.ClientId;
            Console.WriteLine($"客户端[{ClientId}]已连接");
        }

        /// <summary>
        /// 客户连接断开
        /// </summary>
        /// <param name="e"></param>
        private static void MqttNetServer_ClientDisConnected(MqttServerClientDisconnectedEventArgs e)
        {
            var ClientId = e.ClientId;
            Console.WriteLine($"客户端[{ClientId}]已断开连接");
        }
    }
}
