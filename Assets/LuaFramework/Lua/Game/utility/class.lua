
function LuaClass(baseClass)
    -- 一个类模板
    local class_type = { }
    class_type.Base = baseClass
    class_type.ctor = function() end
    class_type.New = function(...)
        -- 对一个新建的表，递归执行构造函数
        local instObj = { }
        instObj.Base = baseClass
        instObj.ctor = function() end
        -- 递归函数
        local CallCtor
        CallCtor = function(curClassType, ...)
            if curClassType.Base ~= nil then
                CallCtor(curClassType.Base, ...)
            end
            if curClassType.ctor ~= nil then
                curClassType.ctor(instObj, ...)
            end

        end

        -- 调用递归
        CallCtor(class_type, ...)

        -- 设置元表
        setmetatable(instObj, { __index = class_type })

        return instObj
    end

    setmetatable(class_type, { __index = baseClass, __call = class_type.New})
    return class_type
end

