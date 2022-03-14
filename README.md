# xinNyaBot - 芯喵
蔚蓝群 通用机器人 v1.0.0

框架采用 **QQMini Pro** 开发 主开发者为 Small_Miao

#### 环境搭建

按照教程搭建环境并设置项目安装Nuget包 [环境搭建 | QQMini.PluginSDK (minibot.cc)](https://doc.dotnet.minibot.cc/Guide/)

直接编译并放入QQMini Pro `App`文件夹内运行即可

> 请注意 需要再初次运行时填写 `xinNya_config.json`配置文件



#### 配置文件

配置文件备注如下

```json
{
    "welcomeString": [],//进群欢迎语句
    "allowGroups": [],//设置机器人生效群
    "websocketServer": "ws://centralteam.cn:232/",//群服成绩通告WebSocket服务器
    "xinguanUrl": "https://c.m.163.com/ug/api/wuhan/app/data/list-total",//新冠疫情API
    "scoreApi": "http://centralteam.cn:234/api/getMapScoreList"//群服分数获取API
}
```



##### WebSocket Api

当前群服WebSocket地址为 `ws://centralteam.cn:232` 

**链接流程**

- 链接到服务器并发送api验证包

- > 注意 链接到服务器5秒钟内不发送验证包将会强制关闭链接

- Api验证包内容如下

  | 表头(type) | 内容(data)                          | 时间戳(timespan) | 返回(return)                 | 类型   |
  | :--------- | ----------------------------------- | ---------------- | ---------------------------- | ------ |
  | auth       | `object`  `{"apikey":"你的API"}`    | unix时间戳       | `{"code":"0"}`               | 验证包 |
  | ping       | `string`  `{"pingmsg":"任意字符"}`  | unix时间戳       | `{"code":"0","msg":"pong!"}` | 心跳包 |
  | score      | `object` `{message:{score object}}` | unix时间戳       | ServerSide                   | 分数包 |

- 链接成功后保持30秒心跳包 30秒无心跳包直接断开链接

#### 当前功能

| 指令                 | 简介                            |
| -------------------- | ------------------------------- |
| 早安                 | 让芯喵和你说早安呀~             |
| 晚安                 | 让芯喵和你说晚安呀~             |
| 疫情资讯             | 提供最新一天疫情资讯            |
| 群服成绩播报(无触发) | 自动播报群服最新排行榜成绩(#10) |
| 进群欢迎             | 随机欢迎在写了哦 XD             |

### 反馈

欢迎各位来积极给芯喵加功能 如有问题可以直接**isues**

也欢迎各位来直接**PR**

