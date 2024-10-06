local UIModuleMgr = require "Framework.Core.UIModuleMgr"
local LoginMgr = Class("LoginMgr", UIModuleMgr)

require "Modules.Login.LoginPart"

function LoginMgr:Ctor()
    self.super.Ctor(self)
    self:AddUI("Login", require "Modules.Login.LoginWnd")

    --TODO:注册服务器广播事件：onReceveServerData()

    CommandManager.Add(CommandID.TryLogin, LoginPart.NeedTryLogin)
    CommandManager.Add(CommandID.DoLogin, LoginPart.DoLogin)
end

--由服务器广播触发
function LoginMgr:OnReceveServerData(msg)
    self.msg = msg
    --在模块管理器里获取UI的方法
    local loginUI = self:GetUI(1)
    if loginUI and loginUI.gameObject.activeInHierarchy then
        loginUI:RefrshUI()
    end
end

function LoginMgr:getServerData()
    return self.msg
end

return LoginMgr
