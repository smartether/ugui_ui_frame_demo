require('Game/utility/class')
--require('utility/rx')
--require('utility/rx_d')
--require('utility/RxTable')
local LoadEmit = require("Game/UI/base/LoadPrefabEmit")
--unit test lib
-- require('ui/test/TestInclude')

local util = {}
util.pack = table.pack or function(...) return { n = select('#', ...), ... } end
util.unpack = table.unpack or unpack

-- UIDefine Expect
UIDefine = {}
-- UI层级优先级 暂未用
UIDefine.UIPriority = {
    UI_Login = 1,
    UI_Tips = 100
}

--静态配置的层级
UIDefine.Sort = {
    DlgConsole_DlgConsole = 10000
}

UIManager = { }
local this  = UIManager

--[[ Global UI Container ]]
UIManager._Root = nil
-- ViewBase类型
UIManager._UIBase = require('Game/ui/base/UIBase')
--ViewModel类型
UIManager._ViewModel = require('Game/ui/base/ViewModel_d')
--所有View的类型
UIManager._ViewTypes = {}
--所有ViewModel的类型
UIManager._ViewModelTypes = {}
--打开的View实例 背包界面等
UIManager._ViewInstances = {}
--创建的View模板实例  背包里面的物品Icon等
UIManager._ViewTemplateInstances = {}
--UIManager._SortedView = {}

UIManager._ViewModels = {}

UIManager._Logics = {}

UIManager._MainCanvasGo=nil

function UIManager.AddLogic(key,logic)
    UIManager._Logics[key] = logic
end

function UIManager.GetLogic(key)
    return UIManager._Logics[key]
end

function UIManager.Add(key,value)
    UIManager._ViewModels[key] = value
end

UIManager._Capacity = 5
UIManager._ZoomFact = 20
UIManager._UICamera = nil

function UIManager.Test()
	UIManager.OpenPanel('DlgBag', 'dlgbag',function() end)
end

function UIManager.Init()
    UIManager._Root = UnityEngine.GameObject.Find('/GameManager/UIManager/RootCanvas').gameObject
    UIManager._MainCanvasGo=UnityEngine.GameObject.Find('/GameManager/UIManager/MainCanvas').gameObject 
 
    UIManager.OpenPanel("UI_Test",'ui_test', function() end)
end

function UIManager.GetViewModel(module)
    local vm = this._ViewModels[type] 
    if(vm ~= nil) then
        return vm
    else
        vm = this.GetModelType(module .. "ViewModel",module).New()
        return vm
    end
end

--添加ViewModel类型 并且注册到UIManager中 private
function UIManager.AddViewModelType(handle, module, type)
    local fullTypeName = handle
    if(module ~= nil) then
        fullTypeName = module .. '::' .. handle
    end
    if(this._ViewModelTypes[fullTypeName] ~= nil) then
        print('$$ ViewModelType '.. fullTypeName.. ' is exist.')
    end
    this._ViewModelTypes[fullTypeName] = type
end
--创建一个ViewModel类型  并且注册到UIManager中 public
function UIManager.GenViewModelType(type, module, baseType)
    local t = nil
    if baseType ~= nil and this._ViewModelTypes[baseType] ~= nil  then
        t = LuaClass(this._ViewModelTypes[baseType])
    else if UIManager._ViewModel ~= nil then
            t = UIManager._ViewModel.CreateType(type, module) -- type,module for debug purpose
        else
            t = LuaClass()
        end
    end
    this.AddViewModelType(type, module, t)
    return t
end
--获取一个ViewModel类型 public
--GenViewModelType时候没有指定module getType的时候可以不指定module
function UIManager.GetModelType(type, module)
    local fullTypeName = type
    if(module ~= nil) then
        fullTypeName = module .. '::' .. type
    end
    local vType = this._ViewModelTypes[fullTypeName] or this._ViewModelTypes[type] or  pcall(require,"ui/manual/" .. module.. "/" .. type) or pcall(require,"Game/UI/" .. string.lower(module) .. "/" .. type) or 0
    if(vType ~= 0) then
        vType = this._ViewModelTypes[fullTypeName] or this._ViewModelTypes[type];
    end
    return vType;
end

--添加一个View类型 并且注册到UIManager中 private
function UIManager.AddUIType(handle,viewType, module)
    local fullTypeName = module .. '::' .. handle
    if(this._ViewTypes[fullTypeName] ~= nil) then
        print('$$ UIType '.. handle.. ' is exist.')
    end
    this._ViewTypes[fullTypeName] = viewType
    viewType.ClassName = fullTypeName
end

--创建一个View类型  并且注册到UIManager中 public
function UIManager.GenUIType(type, module,baseType)
    local t = nil
    local fullTypeName = module .. '::' .. type
    if(baseType ~= nil and this._ViewTypes[module .. "::" .. baseType] ~= nil) then
        t = LuaClass(this._ViewTypes[module .. "::" .. baseType])
    else
        t = LuaClass(UIManager._UIBase)
    end
    t.GetModule = function() return module end
    this.AddUIType(type, t, module)
    -- 注入自动生成的代码
    if(module ~= nil) then
        local loadSuccess, res = pcall(require, 'ui/gen/' .. module .. '/' .. type ..'_gen')
        if(not loadSuccess) then
            xpcall(function()  require('Game/UI/' .. string.lower(module) .. '/' .. type ..'_gen') end, 
            function(err) print("## uiType " .. fullTypeName .. " generated file load failed .. \n" .. tostring(err)) end)
            -- loadSuccess, res = pcall(require, 'ui/' .. string.lower(module) .. '/' .. type ..'_gen')
        end
    end
    return t
end


--获取一个View类型 public
function UIManager.GetUIType(viewType, module)
    local fullTypeName = module .. '::' .. viewType
    local vType = this._ViewTypes[fullTypeName];
    local loadSuccess = false
    if(vType == nil) then
        loadSuccess, vType = pcall(require, 'ui/manual/' .. module.. '/' .. viewType)
        if(not loadSuccess) then
            xpcall(function() require('Game/UI/' .. string.lower(module) .. '/' .. viewType) end, 
            function(err) print("## uiType " .. fullTypeName .. "  file load failed .. \n" .. tostring(err)) end)
            -- loadSuccess, vType = pcall(require, 'ui/' .. string.lower(module) .. '/' .. viewType)
        end

        if(vType ~= nil) then
            if(this._ViewTypes[fullTypeName] == nil) then
                this.AddUIType(viewType, vType, module)
            end
        end
        
        vType = this._ViewTypes[fullTypeName]
    end
    return vType;
end

-- public  /** 可以创建所有ui节点 e:背包,背包里面的Icon module大模块类型(背包等)  type小模块类型(背包物品) */
function UIManager.Create(type,module,parent,onfinished, ...)
    local args = util.pack(...)
    local viewType = this.GetUIType(type, module or type)    --默认module名称和type名称一样
    local view = viewType.New(...)
    local emit = LoadEmit.New(type, module, view, parent, args, onfinished)
    local fullTypeName = (module or type) .. '::' .. type

    if(viewType.Pnl ~= nil or parent == nil) then
        if(this._ViewInstances[fullTypeName] == nil) then
            this._ViewInstances[fullTypeName] = {}
        end
        table.insert(this._ViewInstances[fullTypeName], view)        
    end

    if(parent ~=nil or (parent~= nil and viewType.Pnl == nil)) then
        if(this._ViewTemplateInstances[parent] == nil) then
            this._ViewTemplateInstances[parent] = {}
        end
        table.insert(this._ViewTemplateInstances[parent], view)
    end

	-- LuaComponent.Create(viewType.PATH(), onLoaded)
    LuaComponent.CreateWithEmit(viewType.PATH(), emit)

        -- this._ViewInstances[type].last = view
    return view
end


-- public /** 用来创建逻辑上的UI节点 e:登录界面,背包 */
function UIManager.OpenPanel(type,module, onFinish, ...)
    local view = nil
    local fullTypeName = (module or type) .. '::' .. type
    if(this._ViewInstances[fullTypeName] ~= nil and #this._ViewInstances[fullTypeName] > 0) then
        view = this._ViewInstances[fullTypeName][1]
    end
    if(view ~= nil) then
        view:SetActive(true)
        view:OnShow(...)
        if(onFinish ~= nil) then
            onFinish(view)
        end
    else
        if(select('#', ...) < 1) then
            local model = this._ViewModels[(module or "") .. "::" .. type .. 'ViewModel'] or this._ViewModels["::" .. type .. 'ViewModel'] 
            if(model ~= nil) then
                view = UIManager.Create(type, module, nil, onFinish, model)
            else
                local modelType = UIManager.GetModelType(type .. 'ViewModel', module or type)
                if(modelType ~= 0) then
                    local model = modelType.New()
                    view = UIManager.Create(type, module, nil, onFinish, model)
                else
                    view = UIManager.Create(type, module, nil, onFinish)
                end
            end
        else
            view = UIManager.Create(type,module, nil, onFinish, ...)
        end
    end
    return view
end

-- 隐藏ui  (临时的)
function UIManager.HidePanel(type,module, hide)
    local fullTypeName = module .. '::' .. type
    if(this._ViewInstances[fullTypeName] ~= nil) then
        local views = this._ViewInstances[fullTypeName]
        for i,view in ipairs(views) do
            view:SetActive(not hide)
        end
    end
end

-- 排序 public 
function UIManager.SetSortOrder(module, type, view)
    local order = 0
    local maxDepth = 0
    local miniDepth = 0
    for k,v in pairs(this._ViewInstances) do
        for i,item in ipairs(v) do
            if(item._sort ~= nil) then
                maxDepth = math.max(item._sort, maxDepth)
                miniDepth = math.min(item._sort, miniDepth)
            end
        end
    end

    local fullName = (module or type) .. '_' .. type
    if(UIDefine.Sort[fullName] ~= nil) then
        local constOrder = UIDefine.Sort[fullName]
        local hasCanvas = LuaComponent.SetSortOrder(view.m_GameObject, constOrder)
    else
        order = maxDepth + this._Capacity
        -- /** 需要区分Canvas和没有的来优化 没有canvas的但是有大量动画和顶点重建的需要动态分配Canvas */
        local hasCanvas = LuaComponent.SetSortOrder(view.m_GameObject, order)
        if(hasCanvas) then
            view._sort = order
        end
    end


    if(UIDefine.UIPriority[type] ~= nil) then
        
    end
    

    -- /** 当最小层级过大的时候 平移层级到基准层级 后续要添加相邻层级间隔优化 */
    if(miniDepth > this._Capacity * this._ZoomFact) then
    
    for k,v in pairs(this._ViewInstances) do
        for i,item in ipairs(v) do
            if(item._sort ~= nil) then
                item._sort = item._sort - this._Capacity * this._ZoomFact
                LuaComponent.SetSortOrder(module, item.m_GameObject, order)
            end
        end
    end
    end
end

--关闭某一个UI public 
function UIManager.ClosePanel(type, module)
    local fullTypeName = module .. '::' .. type
    if(this._ViewInstances[fullTypeName] ~= nil) then
        for i,v in ipairs(this._ViewInstances[fullTypeName]) do
            if(v._CloseMethod == nil) then
                v:OnClose()
                UIManager.DestroyViewInstance(v)
            elseif(v._CloseMethod == "DISABLE" or v._CloseMethod == this._UIBase.DESTROY_METHOD.DISABLE) then
                    v:OnHide()
                    v:SetActive(false) 
            end
        end
    end
end

-- 只有在UIBase中调用 friend UIBase
function UIManager.DestroyViewInstance(viewInsance, forceDestroy)
    if(viewInsance == nil) then
        print("## Pnl is nil")
        return
    end

    if(viewInsance ~= nil) then
        if(forceDestroy or viewInsance._CloseMethod == nil) then
            local findLast = -1
            if(this._ViewInstances[viewInsance.ClassName] ~= nil) then
                for i,v in ipairs(this._ViewInstances[viewInsance.ClassName]) do 
                    if v == viewInsance then
                        findLast = i
                   end
                end
            end
        
            if(findLast ~= -1) then
                local child = this._ViewTemplateInstances[viewInsance]
                if(child ~= nil) then
                    for i,v in ipairs(child) do
                        v:OnDestroy()
                        v:Dispose()
                        v:UnBindView()
                    end
                    for i=1,#this._ViewTemplateInstances[viewInsance] do
                         table.remove(this._ViewTemplateInstances,i)
                    end 
                end

                table.remove(this._ViewInstances[viewInsance.ClassName], findLast);
            end
        
            viewInsance:OnDestroy()
            viewInsance:Dispose()
            UnityEngine.GameObject.Destroy(viewInsance.m_GameObject)
            viewInsance:UnBindView()
        elseif(viewInsance._CloseMethod == "DISABLE" or viewInsance._CloseMethod == this._UIBase.DESTROY_METHOD.DISABLE) then
            viewInsance:OnHide()
            viewInsance:SetActive(false) 
        end
    end
end

return this