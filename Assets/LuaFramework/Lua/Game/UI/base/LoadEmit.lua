local LoadEmit = LuaClass()

local util = {}
util.pack = table.pack or function(...) return { n = select('#', ...), ... } end
util.unpack = table.unpack or unpack

function LoadEmit:ctor(type, module, view, parent, args, onfinished)
    self._module = module
    self._type = type
    self._view = view
    self._parent = parent
    self._args = args
    self._onfinished = onfinished
end

function LoadEmit:Init(type, module, view, parent, args, onfinished)
    self._module = module
    self._type = type
    self._view = view
    self._parent = parent
    self._args = args
    self._onfinished = onfinished
end

function LoadEmit:Emit(go)
    self._view.m_GameObject = go
    self._view:OnLoaded(go)
    if(self._parent == nil and UIManager._Root ~= nil) then
        self._view:SetParent(UIManager._Root)
    end
    UIManager.SetSortOrder(self._module,self._type, self._view)

    local unpackedArgs = util.unpack(self._args)
    self._view:Init(unpackedArgs) 


    if(self._onfinished ~= nil) then
        self._onfinished(self._view)
    end
    self._view:OnOpen()
end


return LoadEmit