--主入口函数。从这里开始lua逻辑
function Main()					
	print("logic start")	 	
	UIManager = require('Game/UI/base/UIManager')
	UIManager.Init()
end

--场景切换通知
function OnLevelWasLoaded(level)
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
end

function OnApplicationQuit()
end