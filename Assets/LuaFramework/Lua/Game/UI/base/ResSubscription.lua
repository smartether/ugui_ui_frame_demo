local ResSubscription = LuaClass()


function ResSubscription:ctor(taskId, callback)
    self.Dispose = nil
    self.Dispose = slot(ResSubscription.unSubscribe, self)

    self._taskId = taskId
    self._callback = callback
end

function ResSubscription:Init(taskId, callback)
    self._taskId = taskId
    self._callback = callback
end

function ResSubscription:unSubscribe()
    g_ResMgr:RemoveTask(self._taskId, self._callback)
end

return ResSubscription