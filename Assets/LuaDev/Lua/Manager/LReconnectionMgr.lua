-- region *.lua
-- Date
-- 此文件由[BabeLua]插件自动生成

local class = require "30log"
local Timer = require "System.Timer"

local LLoadingMgr = require "Manager.LLoadingMgr"

-- 网络重连管理器
---@class LReconnectingManager @ ---------------------------------------------------------------
LReconnectingManager = LReconnectingManager or class('LReconnectingManager')

function LReconnectingManager:init()
    -- 延迟转圈等待定时器
    self.timerDelayLoading = nil
    -- 重连定时器
    self.timerReconnecting = nil
    -- 锁定中
    self.isLocked = false
end

function LReconnectingManager:IsLocked()
    return self.isLocked
end

---@param delayLoadingTime number 延迟转圈等待时间(毫秒)
---@param reconnectTime number 延迟重连时间(毫秒)
function LReconnectingManager:Lock(delayLoadingTime, reconnectTime)
    self.isLocked = true
    self:RemoveTimer()

    -- 延迟转圈等待
    local fnShowLoading = function()
        LLoadingMgr:Unlock(self.timerDelayLoading)
        self.timerDelayLoading = nil
        LLoadingMgr:OpenLoadingUI()
    end
    if delayLoadingTime > 0 then
        self.timerDelayLoading = LLoadingMgr:Lock(delayLoadingTime, fnShowLoading)
    else
        fnShowLoading()
    end

    -- 太久了断开连接，重新调用登陆
    self.timerReconnecting = LLoadingMgr:Lock(reconnectTime, function()
        LLoadingMgr:Unlock(self.timerReconnecting)
        self.timerReconnecting = nil
        LogWarning("GlobalPart.ChongLianView ReConnect")

        --尝试再次登陆
        CommandManager.Execute(CommandID.DoLogin)
    end)
end

function LReconnectingManager:Unlock()
    if not self.isLocked then
        return
    end

    self.isLocked = false
    self:RemoveTimer()
    LLoadingMgr:CloseLoadingUI()
end

function LReconnectingManager:RemoveTimer()
    LLoadingMgr:Unlock(self.timerDelayLoading)
    LLoadingMgr:Unlock(self.timerReconnecting)
    self.timerDelayLoading = nil
    self.timerReconnecting = nil
end

LReconnectingMgr = LReconnectingManager:new()

return LReconnectingMgr