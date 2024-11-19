local protobuf = require "pb"
local cjsonutil = require "cjson.util"
local crypt = require "crypt"

local class = require "30log"

LNetManager = LNetManager or class("LNetManager")

function LNetManager:init()
    self.NetChannel = nil
    self.SessionId = 0;
    self.SessionMap = {};
end

function LNetManager:OnInit()
    self.NetChannel = NetManager.GetNetworkChannel("GameServer")
    if IsNil(self.NetChannel) then
        local helper = GameClient.Network.GameServerNetworkChannelHelper()
        local serviceType = GameFramework.Network.ServiceType.Tcp
        self.NetChannel = NetManager.CreateNetworkChannel("GameServer", serviceType, helper)

        CommandManager.Add(CommandID.NetworkConnected, function ()
            Log("CommandID.NetworkConnected")
        end)

        CommandManager.Add(CommandID.NetworkClose, function ()
            Log("CommandID.NetworkClose")
            self:Reset()

            local messageBox = Modules.Common:OpenUI("MessageBox")
            messageBox:SetContent("链接已经断开")
            messageBox:OnOk(function ()
                Modules.Login:EnterLogin()
            end)
        end)

        CommandManager.Add(CommandID.NetworkError, function(errorCode, socketErrorCode, errorMessage)
            LogError(string.format("CommandID.NetworkError %s, %s, %s", tostring(errorCode), tostring(socketErrorCode), errorMessage))
            self:Reset()

            local messageBox = Modules.Common:OpenUI("MessageBox")
            messageBox:SetContent("链接已经断开 errorMessage:"..errorMessage)
            messageBox:OnOk(function ()
                Modules.Login:EnterLogin()
            end)
        end)
    end
end

function LNetManager:Reset()
    self.SessionMap = {}
    self:StopPing()
end

function LNetManager:StartPing()
    self:StopPing()
    self.PingTimer = Timer.New(function()
        self:SendMessage ("common.ping", nil)
    end, 8, -1)
    self.PingTimer:Start()
end

function LNetManager:StopPing()
    if self.PingTimer then 
        self.PingTimer:Stop()
        self.PingTimer = nil
    end
end

function LNetManager:CheckConnect()
    Log(string.format("LNetManager.CheckConnect(%s, %d)", ServerConfig.Address, ServerConfig.Port))
    self.NetChannel:Connect(ServerConfig.Address, math.tointeger(ServerConfig.Port))
end

function LNetManager:CloseConnect()
    self.NetChannel:Close()
    self:Reset()
end

function LNetManager:SendMessage(msgName, args, cb)
    Log(string.format("=============> sendmsg msgname:%s data: %s", msgName, cjsonutil.serialise_value(args)))

    local client_session_id = 0;
    if cb ~= nil then
        self.SessionId = self.SessionId+1;
        client_session_id = self.SessionId;
        self.SessionMap[client_session_id] = { 
            name = msgName, 
            req = args, 
            callback = cb,
        };
    end

	local bytesBody = ""
	if args then
		bytesBody = assert(protobuf.encode('proto.'..msgName, args))
	end
	local bytemsg = string.pack(">s2>I4>s2", msgName, client_session_id, bytesBody)

    Log(string.format("=============> sendmsg msgname:%s bytes_body: %s|", msgName, crypt.base64encode(bytesBody)))
    Log(string.format("=============> sendmsg msgname:%s bytemsg: %s|", msgName, crypt.base64encode(bytemsg)))

    local packet = GameClient.Network.GameServerPacketReq(msgName, bytemsg)
    self.NetChannel:Send(packet)
    return client_session_id
end

function LNetManager:SendMessageWithLock(msgName, args, cb)
    local client_session_id = self:SendMessage(msgName, args, cb)
    if client_session_id > 0 then
        -- 加载loading
        local timerId = LLoadingMgr:Lock(0.5)
        self.SessionMap[client_session_id].timerId = timerId
    end
    return client_session_id
end

function LNetManager:OnResMsgresult(msgResult)
    local client_session_id = msgResult.session
    local session = self.SessionMap[client_session_id]
    if session ~= nil then
        -- 取消loading
        LLoadingMgr:Unlock(session.timerId)

        local ok, err_msg = pcall(session.callback, session.req, msgResult.result, msgResult.error_code)
        if not ok then
            LogError(string.format("    session %s[%d] for msgresult error : %s", session.name, client_session_id, tostring(err_msg)))
        end
    end
end

function LNetManager.OnReceveServerData(msgName, bytesBody)
    Log(string.format("=============> receivemsg msgname:%s bytes_body: %s|", msgName, crypt.base64encode(bytesBody)))
    local resp = nil
    if bytesBody then
        resp = assert(protobuf.decode('proto.'..msgName, bytesBody))
    end
    Log(string.format("=============> receivemsg msgname:%s data: %s", msgName, cjsonutil.serialise_value(resp)))

    if "res_msgresult" == msgName then
        LNetMgr:OnResMsgresult(resp)
        return
    end

    local nFunc = Proto[msgName]
    if nFunc == nil then
        LogError(string.format("函数找不到 %s", msgName))
    else
        nFunc(resp)
    end
end

function LNetManager.OnNetworkConnected(ud)
    CommandManager.Execute(CommandID.NetworkConnected)
end

function LNetManager.OnNetworkClose()
    CommandManager.Execute(CommandID.NetworkClose)
end

function LNetManager.OnNetworkError(errorCode, socketErrorCode, errorMessage)
    CommandManager.Execute(CommandID.NetworkError, errorCode, socketErrorCode, errorMessage)
end

LNetMgr = LNetManager:new()

return LNetMgr
