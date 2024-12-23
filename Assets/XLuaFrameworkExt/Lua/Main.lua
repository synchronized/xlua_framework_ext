--主入口函数(从这里开始lua逻辑)
require "_define"
require "_logger"
require "_utils"
require "queue"
require "stack"
require "list"
require "event"


require "System.Timer"
require "System.coroutine"

require "Framework.Common.LuaUtil"
require "Framework.Common.StringUtil"
require "Framework.Common.TableUtil"
require "Framework.Common.EventManager"
require "Framework.Common.CommandManager"
require "Framework.UnityHelper"
require "Framework.BTween"

HotfixMain = require "XLua.HotfixMain"
local UIManager = require "Framework.Core.UIManager"

function Main()
    local StartGame = require "StartGame"
    StartGame.Run()
end

function OnApplicationQuit()
    UIManager.Shutdown()
end
