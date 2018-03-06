local SubscriptionBase = LuaClass()


function SubscriptionBase:ctor()
    self.Dispose = nil
    self.Dispose = slot(Subscription.unSubscribe, self)
end

function ResSubscription:Init()
   
end

function SubscriptionBase:unSubscribe()

end

return SubscriptionBase