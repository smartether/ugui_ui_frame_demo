local UI_Test = UIManager.GenUIType('UI_Test','ui_test')
local this=UI_Test
--  /** function ctor */
function UI_Test:ctor(modelOrVmodel)
	self._model = modelOrVmodel
end

--  /** function Init */
function UI_Test:Init()
	-- TODO  INIT DATA
	
	self:BindView()
	self:PostBind()
	self:BindEvent()
	-- TODO
end

--  /** function PostBind */
function UI_Test:PostBind()
	
end

--  /** function BindEvent */
function UI_Test:BindEvent()
	
end

--  /** function OnDestroy */
function UI_Test:OnDestroy()
	
end

--  /** function OnOpen */
function UI_Test:OnOpen()
	
end

--  /** function OnClose */
function UI_Test:OnClose()
	
end

--  /** function OnShow */
function UI_Test:OnShow()
	
end

--  /** function OnHide */
function UI_Test:OnHide()
	
end

--  /** function OnShowUIAnimation */
function UI_Test:OnShowUIAnimation()
	
end

--  /** function OnHideUIAnimation */
function UI_Test:OnHideUIAnimation(func,obj)
	
end

--  /** function OnUIAnimationEnd */
function UI_Test:OnUIAnimationEnd(uibase,args)
	
end

