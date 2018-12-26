Dim info As String = "Script for smart loading 'jumping' scenes.
It looks what loaded in front and middle layers.
Create scene duplicate and load in front layer if it needed.
Make two duplicates at first loading.
Developer: Dmitry Dudin. Version 0.2 (26 dec 2018)
"

Dim loadedSceneInMiddle, loadedSceneInFront, pathForLoad, sceneUUID, sceneInfo, uuid, sceneRef, sceneID As String
Dim zeroRefCount, less, more, ref, countScenesInPool As integer
Dim arr_scenes As Array[String]
Dim needCheckCountSceneInPool As Boolean

Sub Load(scenePath As String)
    loadedSceneInMiddle = System.SendCommand("RENDERER*LOCATION_PATH GET")
	loadedSceneInFront  = ""
	if CInt(System.FrontScene) <> 0 then
		loadedSceneInFront  = System.SendCommand("RENDERER*FRONT_LAYER*LOCATION_PATH GET")
	end if
	sceneUUID = System.SendCommand("RENDERER*UUID GET")
	sceneInfo = System.SendCommand("SCENE INFO")
	sceneInfo.Split("\n", arr_scenes)
	needCheckCountSceneInPool = false
	
	if loadedSceneInFront == "SCENE*" & scenePath then
		'if the front layer have the scene (the scene did not have time to jump) we just catch it and use. We can't avoid cut of frames.
        pathForLoad = "SCENE*" & System.SendCommand("RENDERER*FRONT_LAYER*OBJECT_ID GET")
    elseif loadedSceneInMiddle <> "SCENE*" & scenePath then
		'if the middle layer have another scene we get the scene as usual
        pathForLoad = "SCENE*" & ScenePath
		needCheckCountSceneInPool = true
	else
		'there is the same scene in middle layer
		'we have to find or create copy of the scene for independent using
	    zeroRefCount = 0
	
	    for each scene in arr_scenes
	        scene.Trim()
	        if scene <> "" then
				uuid = GetUUID(scene)
	
	            if uuid == sceneUUID then
					countScenesInPool += 1
					ref = scene.Find("#ref=")
	                sceneRef = scene.GetSubstring(ref+5,1)
	
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
		
	    if zeroRefCount <= 0 then
			'we sure duplicate must be created
			pathForLoad = System.SendCommand(sceneUUID & " DUPLICATE")
	    end If
    end If

	System.SendCommand("RENDERER*FRONT_LAYER SET_OBJECT " & pathForLoad)
	
	if needCheckCountSceneInPool then
		'Looking providently if the scene already have copies...
		sceneUUID = System.SendCommand("RENDERER*FRONT_LAYER*UUID GET")
		countScenesInPool = 0
		for each scene in arr_scenes
	        scene.Trim()
	        if scene <> "" AND GetUUID(scene) == sceneUUID then
				countScenesInPool += 1
	        end if
	    next
		if countScenesInPool <= 1 then
			'Making additional copy of scene for the future
			System.SendCommand(sceneUUID & " DUPLICATE")
	    end If
	end if
End Sub

Function GetUUID(s As String) As String
	less = s.find("<")
	more = s.find(">")
	GetUUID = s.GetSubstring(less,more-less+1)
End Function
