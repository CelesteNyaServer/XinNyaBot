using QQMini.PluginSDK.Core;
using QQMini.PluginSDK.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WebSocketSharp;
using Newtonsoft.Json;
using ActDieMoeDownloader.Utils;

namespace com.smallmiao.cn
{
	public class Main : PluginBase
	{
		private Config robotConfig;
		private List<QQObject> qqPlayers = new List<QQObject>();
		private WebSocket webSocket;
		public override PluginInfo PluginInfo
		{
			get
			{
				PluginInfo info = new PluginInfo();
				info.PackageId = "com.smallmiao.cn";
				info.Name = "芯喵BOT";
				info.Version = new System.Version(1, 0, 0, 0);
				info.Author = "Small_Miao";
				info.Description = "芯喵机器人";
				return info;
			}
		}
		public override void OnInitialize()
		{
			checkConfigFile();
			connectWebSocketServer();
			base.OnInitialize();
		}
		// 欢迎语句
		public override QMEventHandlerTypes OnGroupMemberBeAllowAdd(QMGroupMemberIncreaseEventArgs e)
		{
			if (robotConfig.allowGroups.Contains(e.FromGroup))
			{
				QMApi.SendGroupMessage(e.RobotQQ, e.FromGroup, $"[@{e.FromQQ.Id}] 芯喵在这里欢迎你呀~ 祝你早日成为202大佬哦喵~");

			}
			return base.OnGroupMemberBeAllowAdd(e);
		}
		// 接受群组消息
		public override QMEventHandlerTypes OnReceiveGroupMessage(QMGroupMessageEventArgs e)
		{
			if (robotConfig.allowGroups.Contains(e.FromGroup))
			{
				QQObject user = null;
				try
				{
					user = qqPlayers.Find(x => x.qqNumber == e.FromQQ.Id);
					if (user == null)
					{
						user = new QQObject()
						{
							qqNumber = e.FromQQ.Id
						};
						qqPlayers.Add(user);
					}
				}
				catch
				{
					user = new QQObject()
					{
						qqNumber = e.FromQQ.Id
					};
					qqPlayers.Add(user);
				}
				if (user.IgnoreTime != DateTime.MinValue || user.IgnoreTime > DateTime.Now)
					return base.OnReceiveGroupMessage(e);
				if (e.Message == "早安")
				{
					if (user.NightTime == DateTime.MinValue || (DateTime.Now - user.NightTime).Days >= 1)
					{
						QMApi.SendGroupMessage(e.RobotQQ, e.FromGroup, $"[@{e.FromQQ.Id}] 大骗子！你不可能睡这么长时间喵！不理你了!");
						user.IgnoreTime = DateTime.Now.AddHours(3);
						return base.OnReceiveGroupMessage(e);
					}
					if (DateTime.Now.Hour > 10)
					{
						QMApi.SendGroupMessage(e.RobotQQ, e.FromGroup, $"懒虫！都{DateTime.Now.Hour}点了 怎么才起床喵!");
						user.MorningTime = DateTime.Now;
						return base.OnReceiveGroupMessage(e);
					}
					else
					{
						QMApi.SendGroupMessage(e.RobotQQ, e.FromGroup, $"[@{e.FromQQ.Id}]早安喵~ 昨晚休息的好吗~");
						user.MorningTime = DateTime.Now;
						return base.OnReceiveGroupMessage(e);
					}
				}
				if (e.Message == "晚安")
				{
					if (user.NightTime == DateTime.MinValue || user.NightTime.Day != DateTime.Now.Day)
					{
						QMApi.SendGroupMessage(e.RobotQQ, e.FromGroup, $"[@{e.FromQQ.Id}]那 晚安吧 好好休息哦喵~");
						user.NightTime = DateTime.Now;
						QMLog.Info((user.NightTime.Day != DateTime.Now.Day).ToString());
						QMLog.Info((user.NightTime == DateTime.MinValue || user.NightTime.Day != DateTime.Now.Day).ToString());
						return base.OnReceiveGroupMessage(e);
					}
					if ((DateTime.Now - user.NightTime).Hours < 8 || (DateTime.Now - user.NightTime).Minutes <= 60 || (DateTime.Now - user.NightTime).Seconds <= 60)
					{
						QMApi.SendGroupMessage(e.RobotQQ, e.FromGroup, $"[@{e.FromQQ.Id}]诶？！ 怎么又说了一遍晚安呀！快去睡吧~");
						return base.OnReceiveGroupMessage(e);
					}
					else
					{
						QMApi.SendGroupMessage(e.RobotQQ, e.FromGroup, $"[@{e.FromQQ.Id}]睡醒了吗~ 早哦~");
						return base.OnReceiveGroupMessage(e);
					}
				}
				if (e.Message == "帮助")
				{
					QMApi.SendGroupMessage(e.RobotQQ, e.FromGroup, "这里是芯喵！ 请问有什么问题需要咱的帮助吗？咱可以做以下事情哦! \n" +
						"早安 可以向芯喵问候早安晚安呢喵\n" +
						"晚安 睡觉前可以向芯喵说声晚安吗~\n" +
						"提醒! 芯喵可以帮忙吧群服最新成绩播报到这里哦！");
				}
			}
			return base.OnReceiveGroupMessage(e);
		}

		public void checkConfigFile()
		{
			if (File.Exists("xinNya_config.json"))
			{
				robotConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText("xinNya_config.json"));
			}
			else
			{
				StreamWriter sw = new StreamWriter(File.Create("xinNya_config.json"));
				sw.Write(JsonConvert.SerializeObject(new Config()
				{
					allowGroups = new List<long>() { 943024085 },
					websocketServer = "ws://centralteam.cn:232/",
					welcomeString = new List<string>() { },
					xinguanUrl = ""
				}));
				sw.Close();
				QMLog.Error("芯喵 设置文件丢失! 请注意设置");
			}
		}
		public void connectWebSocketServer()
		{
			QMLog.Info("Try To Connect WebSocket Server!");
			webSocket = new WebSocket(robotConfig.websocketServer);
			webSocket.OnOpen += WebSocket_OnOpen;
			webSocket.OnMessage += WebSocket_OnMessage;
			webSocket.OnError += WebSocket_OnError;
			webSocket.OnClose += WebSocket_OnClose;
			webSocket.Connect();
		}

		private void WebSocket_OnClose(object sender, CloseEventArgs e)
		{
			QMLog.Warning("WebSocket Server Disconnect! Retry Connect!");
			connectWebSocketServer();
			//retry!
		}

		private void WebSocket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
		{
			QMLog.Error("WebSocket Connect Error!" + e.Exception);
		}

		private void WebSocket_OnMessage(object sender, MessageEventArgs e)
		{
			QMLog.Info("Recive WebSocket Message:" + e.Data);
			dynamic info = JsonConvert.DeserializeObject(e.Data);
			Message message = JsonConvert.DeserializeObject<Message>((string)info.message);
			dynamic scoreData = JsonConvert.DeserializeObject(HttpUtils.Get($"{robotConfig.scoreApi}?map_sid={message.map_sid}&mp_b_side={message.is_b_side}&mp_c_side={message.is_c_side}&mp_speed_strawberry={message.is_speed_strawberry}&mp_golden_strawberry={message.gold_strawberry}", 1500));
			int index = 0;
			foreach(var item in scoreData.data)
			{
				if((string)item.mp_player_time == message.player_score && message.player_name == (string)item.mp_player_name)
				{
					goto a;
				}
				index++;
			}
			index = -1;
		a:
			QMLog.Info("Score Index" + index);
			if (index < 10&& index!=-1)
			{
				foreach (var item in robotConfig.allowGroups)
				{
					try
					{
						QMApi.SendGroupMessage(3032444979, item, $"恭喜玩家 {message.player_name} 在地图 {message.map_name}{(message.is_b_side == "1" ? "_B面" : "")}{(message.is_c_side == "1" ? "_C面" : "")}达成了{(message.gold_strawberry == "1" ? "金草莓" : "")} {(message.is_speed_strawberry == "1" ? "速梅" : "")} {message.player_score} 秒 第{index+1}名的好成绩!");

					}
					catch (Exception ex)
					{
						QMLog.Error(ex.Message);
					}
				}
			}

		}

		private void WebSocket_OnOpen(object sender, EventArgs e)
		{
			QMLog.Info("Success Connect Socket Server!");
		}
	}
	public class Config
	{
		public List<String> welcomeString { get; set; }
		public List<long> allowGroups { get; set; }
		public string websocketServer { get; set; }
		public string xinguanUrl { get; set; }
		public string scoreApi { get; set; }
	}
	public class QQObject
	{
		public long qqNumber { get; set; }
		public DateTime NightTime { get; set; } = DateTime.MinValue;
		public DateTime MorningTime { get; set; } = DateTime.MinValue;
		public DateTime IgnoreTime { get; set; } = DateTime.MinValue;
	}
	public class eventObject
	{
		public string type { get; set; }
		public string message { get; set; }
	}
	public class Message
	{
		public string map_sid { get; set; }
		public string map_name { get; set; }
		public string player_score { get; set; }
		public string player_name { get; set; }
		public string is_b_side { get; set; }
		public string is_c_side { get; set; }
		public string is_speed_strawberry { get; set; }
		public string gold_strawberry { get; set; }
	}
}
