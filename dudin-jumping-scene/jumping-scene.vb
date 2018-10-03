Dim info As String = "Скрипт для загрузки прыгающих сцен.
Смотрит что загружено в среднем и верхнем слое.
При необходимости создает копию сцены и загружает её в верхний слой.
При первой загрузке сразу делается копия.
Разработчик: Дудин Дмитрий. Версия 0.1 (3 октября 2018)
"

Dim loadedSceneInMiddle, loadedSceneInFront, pathForLoad, sceneUUID, sceneInfo, uuid, sceneRef, sceneID As String
Dim zeroRefCount, less, more, ref, countScenesInPool As integer
Dim arr_scenes As Array[String]
Dim needCheckCountSceneInPool As Boolean

Sub Load(scenePath As String)
	println("")
	println("")
	println(3,"scenePath = " & scenePath)
    loadedSceneInMiddle = System.SendCommand("RENDERER*LOCATION_PATH GET")
	loadedSceneInFront  = ""
	if CInt(System.FrontScene) <> 0 then
		loadedSceneInFront  = System.SendCommand("RENDERER*FRONT_LAYER*LOCATION_PATH GET")
	end if
	println("loadedSceneInMiddle = " & loadedSceneInMiddle)
	println("loadedSceneInFront  = " & loadedSceneInFront)
	sceneUUID = System.SendCommand("RENDERER*UUID GET")
	println("sceneUUID = " & sceneUUID)
	sceneInfo = System.SendCommand("SCENE INFO")
	println("sceneInfo = " & sceneInfo)
	sceneInfo.Split("\n", arr_scenes)
	needCheckCountSceneInPool = false
	if loadedSceneInFront == "SCENE*" & scenePath then
		'если сцена уже есть во FRONT слое — не успела спрынуть — перехватываем её, все равно cut-склейка неминуема
		println(2,"CATCH SCENE FROM FRONT LAYER")
        pathForLoad = "SCENE*" & System.SendCommand("RENDERER*FRONT_LAYER*OBJECT_ID GET")
    elseif loadedSceneInMiddle <> "SCENE*" & scenePath then
		'если в среднем слое другая сцена то берем сцену просто как обычно
        pathForLoad = "SCENE*" & ScenePath
		needCheckCountSceneInPool = true
	else
		'итак, ситуация ради которой все затевалось:
		'в среднем слое такая же сцена и надо 
		'найти или создать копию этой сцены для независимой выдачи
	    zeroRefCount = 0
	
	    for each scene in arr_scenes
	        scene.Trim()
	        if scene <> "" then
				uuid = GetUUID(scene)
				println("current uuid = " & uuid)
	
	            if uuid == sceneUUID then
					countScenesInPool += 1
					ref = scene.Find("#ref=")
	                sceneRef = scene.GetSubstring(ref+5,1)
					println("current sceneRef = " & sceneRef)
	
	                if sceneRef == "0" then
	                    pathForLoad = "SCENE*" & scene.Left(scene.Find(","))
	
	                    if zeroRefCount > 0 then
	                        sceneID = scene.Left(scene.Find(","))
	                        System.SendCommand("SCENE CLOSE " & sceneID)
							countScenesInPool -= 1
	                    end if
	                    zeroRefCount = zeroRefCount + 1
	                end if
	            end if
	        end if
	    next
		
		println("zeroRefCount = " & zeroRefCount)
	    if zeroRefCount <= 0 then
			'дубликат уже нужен! создаем и используем
			pathForLoad = System.SendCommand(sceneUUID & " DUPLICATE")
	    end If
    end If

    
	println("pathForLoad = " & pathForLoad)
	System.SendCommand("RENDERER*FRONT_LAYER SET_OBJECT " & pathForLoad)
	
	if needCheckCountSceneInPool then
		'предусмотрительно смотрим - эта сцена уже имеет копии?
		sceneUUID = System.SendCommand("RENDERER*FRONT_LAYER*UUID GET")
		countScenesInPool = 0
		for each scene in arr_scenes
	        scene.Trim()
			println("| cur scene = " & scene)
	        if scene <> "" AND GetUUID(scene) == sceneUUID then
				countScenesInPool += 1
	        end if
	    next
		println("countScenesInPool = " & countScenesInPool)
		if countScenesInPool <= 1 then
			'предусмотрительно делаем копию на будущее
			System.SendCommand(sceneUUID & " DUPLICATE")
	    end If
	end if
	
	println("")
	println("")
End Sub

Function GetUUID(s As String) As String
	less = s.find("<")
	more = s.find(">")
	GetUUID = s.GetSubstring(less,more-less+1)
End Function
