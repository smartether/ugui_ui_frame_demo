--lua Event消息通知机制 (暂时只支持同步执行)
--异步执行的方式 就跟ulua提供的eventlib内一样，使用协程以及args参数需要pack保存，然后unpack取出通知

--全局方法
function GetHandlerObjByAdd(handler, obj, event)
	--handler交给luaObj自己保存
	if not obj.handlers then
		obj.handlers = {}
	end

	if not obj.handlers[handler] then		
		obj.handlers[handler] = {callback = handler, luaObj = obj, eventStr = event}
	end

	return obj.handlers[handler]
end

function GetHandlerObjByDel(handler, obj)
	--handler交给luaObj自己保存
	if not obj or not obj.handlers or not handler then
		return
	end

	if not obj.handlers[handler] then		
		return
	end

	return obj.handlers[handler]
end

--自动清理luaObj自身的handler
function AutoClearHandlerObj(luaObj)
	if luaObj and luaObj.handlers then
		for k,v in pairs(luaObj.handlers) do
			EventMgr.RemoveListener(v.eventStr, v.callback, v.luaObj)
		end

		luaObj.handlers = nil
	end
end

------------------------------------------------------------------------------------------
--事件对象
EventObj = LuaClass()

function EventObj:Add(handler, luaObj, event)
	if not self.handlers then 
		self.handlers = {}
	end

	local handlerObj = GetHandlerObjByAdd(handler, luaObj, event)
	if not handlerObj then
		--error("error handlerObj , handlerObj is nil")
	 	return 
	end

	self.handlers[handlerObj] = handlerObj
end

function EventObj:Remove(handler, luaObj)
	if not self.handlers then return end

	local handlerObj = GetHandlerObjByDel(handler, luaObj)
	if not handlerObj then
		--error("error handlerObj , handlerObj is nil")
	 	return 
	end

	self.handlers[handlerObj] = nil
end

function EventObj:Fire(...)
	if not self.handlers then return end

	for k,v in pairs(self.handlers) do
		v.callback(v.luaObj, ...)
	end
end

------------------------------------------------------------------------------------------
--事件管理器
EventMgr = {}

local events = {}

function EventMgr.AddListener(event, handler, luaObj)
	if not event or not handler then
		error("error parameter , event is nil or handler is nil")
		return
	end

	if not events[event] then
		events[event] = EventObj:New(event)
	end

	events[event]:Add(handler, luaObj, event)
end

function EventMgr.RemoveListener(event, handler, luaObj)
	local eventObj = events[event]
	if not eventObj then
		--error("RemoveListener " .. event .. " has no event.")
	else
		eventObj:Remove(handler, luaObj)
	end
end

function EventMgr.BroadCast(event, ...)
	local eventObj = events[event]
	if not eventObj then
		--warn("BroadCast " .. event .. " has no event.")
	else
		--log("BroadCast " .. event .. " Success.")
		eventObj:Fire(...)
	end
end