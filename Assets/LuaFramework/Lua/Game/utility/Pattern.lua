local Pattern = {}


--解析任务配置x_x_x
function Pattern.Convert(config, ...)
	 local paramCount = select("#", ...)

	 local configsTable = {}
	 local configs = string.gmatch(config,"%d+[_%d]+[;]*")
	 for k,v in configs do
		 --print(k,v)
		 local configDic = {}
		 local configItems = string.gmatch(k, "%d+")
		 local idx1 = 1 
		 for k1,v1 in configItems do
			 --print(k1,v1)
			 local key = select(idx1, ...)
			 configDic[key] = k1
			 idx1 = idx1 + 1
		 end
		 table.insert( configsTable, configDic)
	 end
	 return configsTable
end

return Pattern