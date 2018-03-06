local UIBase = LuaClass()
local this=UIBase

local ResSubscription = require("Game/ui/base/ResSubscription")
local EventSubscription = require("Game/ui/base/EventSubscription")

--UIManager._UIBase = this

-- 关闭方法 隐藏 或 销毁 {DISABLE="DISABLE", DESTORY = "DESTORY"}
this.DESTROY_METHOD = {DESTORY= 1, DISABLE = 2}

this._CloseMethod = nil

local qualityIconPath =
{
    "bg_icon_green",
    "bg_icon_blue",
    "bg_icon_purple",
    "bg_icon_orange",
    "bg_icon_red",
    "bg_icon_golden",
}

function UIBase:ctor()
	-- self._viewModel = nil
	self.m_GameObject = nil
	self._Disposable = nil
end

function UIBase:BindCreateTemplate(doCreateTemplate)
	doCreateTemplate(self)
end

function UIBase:ResetPosScale()
	self.m_GameObject.transform.localScale = Vector3.New(1,1,1)
	self.m_GameObject.transform.localPosition = Vector3.New(0,0,0)
end

function UIBase:SetSiblingIndex(index)
	self.m_GameObject.transform:SetSiblingIndex(index)
end

function UIBase:Intersect(transformA, transformB)
	local boundA = UnityEngine.RectTransformUtility.CalculateRelativeRectTransformBounds(transformA)
	local boundB = UnityEngine.RectTransformUtility.CalculateRelativeRectTransformBounds(transformB)
	local b = boundA:Intersects(boundB)
	return b
end

function UIBase:GetGameObject()
	return self.m_GameObject
end

function UIBase:SetActive(active)
	self.m_GameObject:SetActive(active)
end

function UIBase:SetParent(parent)
	self.m_GameObject.transform:SetParent(parent.transform)
	self:ResetPosScale()
end

function UIBase:On(func, onfinished, onfaild)
	local wrapFun = function(...)
		local Onfinished = onfinished
		local Onfaild = onfaild
		if(select('#', ...)==0) then
			func(Onfinished, Onfaild)
		else
	 		func(..., Onfinished, Onfaild) 
		end
	 end
	return wrapFun
end

function UIBase:Func(fun)
	local func = function(...)
		fun(self, ...)
	end
	return func
end

function UIBase:LoadTexture(path, func)
	local isImmediate = false
	local subscription = nil
	local callback = function(asset)
		func(asset)
		isImmediate = true
		self:RemoveDisposable(subscription)
	end
	local taskId = g_ResMgr:LoadBundle(path, callback)
	if(not isImmediate) then
		subscription = ResSubscription.New(taskId, callback)
		subscription:Init(taskId, callback)
		self:AddDisposable(subscription)
	else
		print(string.format( "$$ resource is loaded immediate %s",path))
	end
end

function UIBase:LoadSpriteSync(altasName, spriteName)
	local sprite = g_ResMgr:LoadAtlasSprite(altasName, spriteName)
	return sprite
end

function UIBase:PreLoadAtlas(atlasName)
	g_ResMgr:PreLoadAtlas(atlasName)
end

function UIBase:LoadSprite(path, func)
	local isImmediate = false
	local subscription = nil
	local callback = function(asset)
		func(asset)
		isImmediate = true
		self:RemoveDisposable(subscription)
	end
	local taskId = g_ResMgr:LoadBundle(path, callback)
	if(not isImmediate) then
		subscription = ResSubscription.New(taskId, callback)
		subscription:Init(taskId, callback)
		self:AddDisposable(subscription)
	else
		print(string.format( "$$ resource is loaded immediate %s",path))
	end
end

function UIBase:LoadQualitySprite(quality)
    --print("LoadQualitySprite: " .. tostring(quality) .. " " .. #qualityIconPath)
    if quality >= 0 and quality < #qualityIconPath then
        local spriteName = qualityIconPath[quality + 1]
        return self:LoadSpriteSync("ui/atlasbag.ab", spriteName)
    end

    return nil
end

function UIBase:LoadAsset(path, func)
	local isImmediate = false
	local subscription = nil
	local callback = function(asset)
		func(asset)
		isImmediate = true
		self:RemoveDisposable(subscription)
	end
	local taskId = g_ResMgr:LoadBundle(path, callback)
	
	if(not isImmediate) then
		subscription = ResSubscription.New(taskId, callback)
		subscription:Init(taskId, callback)		
		self:AddDisposable(subscription)
	else
		print(string.format( "$$ resource is loaded immediate %s",path))
	end
end

function UIBase:OnLoaded(go)
	
end

function UIBase:AddDisposable(subscription)
	if(self._Disposable == nil) then
		self._Disposable = {}
	end
	table.insert(self._Disposable, subscription)
end

function UIBase:RemoveDisposable(subscription)
	if(self._Disposable ~= nil and subscription ~= nil) then
		for i=#self._Disposable, 1 do
			local dispose =  self._Disposable[i]
			if(dispose ~= nil and dispose == subscription) then
				table.remove(self._Disposable, i)
			end
		end
		
	end
end

function UIBase:PushSub(subscription)
	subscription.Dispose = function() subscription:unsubscribe() end
	-- table.insert(self._Disposable, subscription)
	self:AddDisposable(subscription)
end

-- EventMgr的语法糖 注册事件到当前实例
-- 返回订阅实例 local sub = UIBase:AddListener(XXX,func) 使用sub()取消退订
-- func = function(self, ...) end  或者  UIBase:Func(...) end
-- 或者等到UIBase关闭的时候自动退订
function UIBase:AddListener(event, handler)
	 EventMgr.AddListener(event, handler, self)
	 local subscription = EventSubscription.New(event, handler, self)
	 self:AddDisposable(subscription)
	 return subscription
end

function UIBase:Dispose()
	if(self._Disposable ~= nil) then
		for i,v in ipairs(self._Disposable) do
			v.Dispose()
		end
		self._Disposable = nil
	end
end

--创建 当前module中类型为type的View 此方法会管理创建出来的节点生命周期 当当前节点被关闭时 子节点也会同时受到OnDestroy的调用
-- onfinished(instanceOfType) 
function UIBase:CreateTemplate(type,onfinished, ...)
    return UIManager.Create(type, self:GetModule(), self,onfinished,...)
end

--创建 指定module中的类型为type的View  此方法会管理创建出来的节点生命周期 当当前节点被关闭时 子节点也会同时受到OnDestroy的调用
-- onfinished(instanceOfType) 
function UIBase:CreateTemplateFromModule(type, module,onfinished, ...)
	UIManager.Create(type, module, self,onfinished,...)
end

-- 克隆当前节点 自己再外部 初始化 此方法克隆出来的节点需要自己管理其生命周期
-- 新框架需求 把prevab实例中的节点作为prefab再创建副本
function UIBase:Clone(...)
	local go = UnityEngine.GameObject.Instantiate(self.m_GameObject)
	local newInstance = self.New(...)
	newInstance.m_GameObject = go
	return newInstance
end

-- 克隆当前节点 自己再外部 初始化 此方法克隆出来的节点需要自己管理其生命周期
-- 新框架需求 把prevab实例中的节点作为prefab再创建副本
function UIBase:SafeClone(...)
	local go = UnityEngine.GameObject.Instantiate(self.m_GameObject)
	local newInstance = self.New(...)
	newInstance.m_GameObject = go
	return newInstance
end

--  /** function DestroyUI */
function UIBase:DestroyUI(forceDestroy)
    UIManager.DestroyViewInstance(self, forceDestroy)
end

function UIBase:SetSortOrder(order)
 
end

function UIBase:BindView()

end

function UIBase:UnBindView()

end

function UIBase:Init(...)

end

--  /** function OnDestroy */
function UIBase:OnDestroy()
	
end

--  /** function OnOpen */
-- 处理打开ui后的动画或者音效
function UIBase:OnOpen()
	
end

--  /** function OnClose */
-- 处理关闭ui后的动画或者音效 销毁了ui实例
function UIBase:OnClose()
	
end

--  /** function OnShow */
-- UI从关闭状态再次唤醒
function UIBase:OnShow()
	
end

--  /** function OnHide */
-- 如果ui关闭是通过setActive(false)时会调用这个回调
function UIBase:OnHide()
	
end

--  /** function OnShowUIAnimation */
function UIBase:OnShowUIAnimation()
	
end

--  /** function OnHideUIAnimation */
function UIBase:OnHideUIAnimation(func,obj)
	
end

--  /** function OnUIAnimationEnd */
function UIBase:OnUIAnimationEnd(uibase,args)
	
end

return this