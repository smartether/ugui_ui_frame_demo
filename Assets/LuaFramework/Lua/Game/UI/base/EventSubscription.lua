local EventSubscription = LuaClass()


function EventSubscription:ctor(event, handler, obj)
    self.Dispose = nil
    self.Dispose = slot(EventSubscription.unSubscribe, self)

    self._event = event
    self._handler = handler
    self._obj = obj
end

function EventSubscription:Init(event, handler, obj)
    self._event = event
    self._handler = handler
    self._obj = obj
end

function EventSubscription:unSubscribe()
    EventMgr.RemoveListener(self._event, self._handler, self._obj)
end

return EventSubscription