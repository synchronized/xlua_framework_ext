local pb = require "pb"

local class = require "30log"

LProtoManager = LProtoManager or class("LProtoManager")

function LProtoManager:OnInit()
    local protoBytes = ResManager.LLoadBinaryAssetSyn("Proto/Protobuf/Protocol")
    assert(pb.load(protoBytes))
end

LProtoMgr = LProtoManager:new()

return LProtoMgr
