local UIView = require "Framework.Core.UIView"
local LoginWnd = UIView:extend("LoginWnd")

local playerInfo = require "Entity.PlayerInfo"

function LoginWnd:PrefabPath()
    return "Prefabs/Login/LoginWnd"
end

function LoginWnd:Awake()
    self.super.Awake(self)

    --设置保存的用户名和密码
    playerInfo.LoginInfo.username = PlayerPrefs.GetString("PLAYERINFO.USERNAME")
    playerInfo.LoginInfo.password = PlayerPrefs.GetString("PLAYERINFO.PASSWORD")
end

function LoginWnd:GetViewModel()
    return {
        data = {
            loginInfo = {
                username = playerInfo.LoginInfo.username,
                password = playerInfo.LoginInfo.password,
            },
        },
        commands = {
            onLogin = function (data)
                local username = data.loginInfo.username
                local password = data.loginInfo.password
                playerInfo.LoginInfo.username = username
                playerInfo.LoginInfo.password = password

                --保存用户名和密码
                PlayerPrefs.SetString("PLAYERINFO.USERNAME", username)
                PlayerPrefs.SetString("PLAYERINFO.PASSWORD", password)
                Log(string.format("username: %s, password: %s", username, password))
                --CommandManager.Execute(CommandID.DoLogin)
                --TODO 临时直接进入主界面
                CommandManager.Execute(CommandID.OpenUI, "LobbyMainMgr")
            end
        },
    }
end

return LoginWnd
