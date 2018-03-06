
local Func = {}

-- function Func.New(object, call)
--     local func = {}
--     func.object = object
--     func.call = call
    
--     setmetatable(func, Func)
--     return func
-- end

function Func:New(object)
    local func = {}
    func.object = object
    func.call = self.call
    
    setmetatable(func, Func)
    return func
end

function Func:OnCall(...)
    return self.call(self.object, ...)
end

Func.__call = Func.OnCall

local FuncProto = {}
FuncProto.__call = nil

function FuncProto.New(func)
    local FuncTemp = {}
    FuncTemp.object = nil
    FuncTemp.call = func

    setmetatable(FuncTemp, {__index = Func, __call = Func.New})
    return FuncTemp
end

return FuncProto