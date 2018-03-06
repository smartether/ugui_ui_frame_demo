
local ViewModel = {}


ViewModel.LastDefinedField = nil

function ViewModel.NewIndexDebug(t, key, value)
    setmetatable(t, {__index=t.__index})
    ViewModel.LastDefinedField = key
    if(string.match(tostring(value), "Subject") or string.match(tostring(value), "Observable")) then
        -- print('$$ define rx type:','name:',ViewModel.LastDefinedField)
        value.NodeName = (t.TypeName or 'global') .. '::' .. key
    end

    t[key] = value
    setmetatable(t, {__index=t.__index, __newindex=ViewModel.NewIndexDebug})
    
    return value
end

function ViewModel.NewIndexRxb(t, key, value)
    local field = rx.BehaviorSubject.create(value)
    t._tablerx[key] = field
    return field
end 

function ViewModel.NewIndexRxs(t, key, value)
    local field = rx.Subject.create()
    t._tablerx[key] = field
    return field
end 

function ViewModel.NewIndex(t, key, value)
    --不允许在在ctor以外的地方对ViewModel添加成员
    print('## define New key from outsize is not permit.')
end

--创建一个类型继承ViewModel
function ViewModel.CreateType(typeName, module)
    local self = {}
    self.Module = module
    --通用构造函数
    self.ctor = function() end
    --自动包装rx类型的构造函数 在此函数内声明的所有类型 都会转化成BehaviourSubject
    self.ctor_rxb = function() end
    --自动包装rx类型的构造函数 在此函数内声明的所有类型 都会转化成Subject
    self.ctor_rxs = function() end
    self._tablerx = nil
    
    --重定向索引self表中找不到 就去找rx类型存储的表
    self.Index= function(t, key) 
        if(self[key] == nil) then
            return t._tablerx[key]   
        else
            return self[key]
        end
    end
    self.New = function(...) 
        local child = {TypeName = typeName}
        --执行ctor自动生成rx实例
        if(self.ctor ~= nil) then
            setmetatable(child, {__index=self, __newindex=ViewModel.NewIndexDebug})
            self.ctor(child,...)
        end
        
        if(self.ctor_rxb) then
            self._tablerx = self._tablerx or {}
            setmetatable(child, {__index=self, __newindex=ViewModel.NewIndexRxb})
            self.ctor_rxb(child, ...)
        end
        
        if(self.ctor_rxs) then
            self._tablerx = self._tablerx or {}
            setmetatable(child, {__index=self, __newindex=ViewModel.NewIndexRxs})
            self.ctor_rxs(child, ...)
        end
        --不允许在在ctor以外的地方对ViewModel添加成员
        setmetatable(child, {__index=self.Index, __newindex=ViewModel.NewIndex})
        
        --把ViewModel放到UIManager下来ViewModel容器里 typeName: XXXViewModel DlgBag DlgBagViewModel
        -- if(module == nil or (typeName ==  module .. "ViewModel") or (typeName == module .. "Model")) then
        --     local fullName = (module or "") .. "::" .. typeName
        --     if(UIManager._ViewModels[fullName] == nil) then
        --         UIManager._ViewModels[fullName] = child
        --     else
        --         print("$$ this ViewModel is Exist, could not be create duplicate..")
        --         return UIManager._ViewModels[fullName]
        --     end
        -- end

        return child
    end
    setmetatable(self, {__index = ViewModel})
    return self
end



return ViewModel