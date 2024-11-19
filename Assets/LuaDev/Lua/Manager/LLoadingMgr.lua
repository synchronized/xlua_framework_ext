
local class = require "30log"
local Timer = require "System.Timer"

-- Load管理器
---@class LLoadingManager @ ---------------------------------------------------------------
LLoadingManager = LLoadingManager or class("LLoadingManager")

function LLoadingManager:init()
    self.isLoading = false
    self.num = 0
    self.timerSet = {}
    self.timerCounter = 0
end

-- 是否锁定
function LLoadingManager:IsLoading()
    return self.isLoading
end

function LLoadingManager:OpenLoadingUI()
    self.isLoading = true
    CommandManager.Execute(CommandID.OpenUI, "CommonMgr", "Loading")
end

function LLoadingManager:CloseLoadingUI()
    if self.num > 0 then
        return
    end

    self.isLoading = false
    CommandManager.Execute(CommandID.CloseUI, "CommonMgr", "Loading")
end

-- 锁定屏幕，超时后回调并解锁，返回定时器
---@param waitTime number
---@param callBack function
---@return integer
function LLoadingManager:Lock(waitTime, callBack)
    self:OpenLoadingUI()

    self.num = self.num + 1

    self.timerCounter = self.timerCounter + 1
    local timerId = self.timerCounter
    local timer = 1
    if callBack ~= nil then
        timer = Timer.New(function()
            self:Unlock(timerId)

            if callBack ~= nil then
                callBack()
            end
        end, waitTime)
        timer:Start()
    end
    self.timerSet[timerId] = timer
    return timerId
end

-- 主动解锁 传入定时器
---@param timerId integer
function LLoadingManager:Unlock(timerId)
    if timerId == nil or self.timerSet[timerId] == nil then
        return
    end

    local timer = self.timerSet[timerId]
    if type(timer) == "table" then
        timer:Stop()
    end
    self.num = self.num - 1
    self:CloseLoadingUI()
end

LLoadingMgr = LLoadingManager:new()

return LLoadingMgr