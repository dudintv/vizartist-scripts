RegisterPluginVersion(1,1,0)
Dim info As String = "Script created by Dmitry Dudin, dudintv@gmail.com
Version 17 February 2019
Import animation from a text file. Where each line is one keyframe except the first line.
The first line have to contain common information abour layer — name and beginning position.
Text file can contain animation of several layers but it import only one selected by name.
"

'for running the script:
Dim stepScript As Integer = 0
Dim countSteps As Integer = 0
Dim countLines As Integer = 0
Dim stepStartLine As Integer
Dim stepCapability As Integer = 10

'stuff
Dim cTarget, cCamera As container
Dim chThisPos, chThisRot, chPosTarget, chPosCamera As Channel
Dim input As String
Dim arrInput As Array[String]
Dim filePath As String
Dim key, key2 As Keyframe
Dim curTime As Double
Dim chPos,chRot,chRotX,chRotY,chRotZ,chScale As Channel
Dim line As String
Dim arrLine As Array[String]
Dim startLine As Integer
Dim aefps, i As Integer
Dim arrChannels As Array[Channel]
Dim layer_name As String

'interface
Dim buttonNames As Array[String]
	buttonNames.Push("50 fps")
	buttonNames.Push("25 fps")
Dim arr_cam_types As Array[String]
	arr_cam_types.Push("One-Node camera")
	arr_cam_types.Push("Two-Node camera")
sub OnInitParameters()
	RegisterPushButton("fold", "Add containers for Camera", 1)
	RegisterPushButton("delanim", "Delete animation", 2)
	RegisterFileSelector("file", "Text file coordinates", "", "", "*.txt")
	RegisterParameterBool("isnamecontainer", "Layer name from contaner name", true)
	RegisterParameterString("layer","Layer name (in AE)", this.name, 50, 1000, "")
	RegisterRadioButton("type", "Type (in AE)", 0, arr_cam_types)
	RegisterRadioButton("aefps", "FPS (in AE)", 0, buttonNames)
	RegisterPushButton("paste", "Import animation", 3)
end sub
sub OnParameterChanged(parameterName As String)
	SendGuiParameterShow("layer", 1 - (Integer)GetParameterBool("isnamecontainer"))
end sub

'checking of existing helper containers
function CheckContainer() as boolean
	if GetParameterInt("type") == 0 then
		CheckContainer = true
		exit function
	elseif GetParameterInt("type") == 1 then
		cTarget = this.FindSubcontainer(this.name & "_camTarget")
		cCamera = this.FindSubcontainer(this.name & "_camPosition")
		if cTarget <> null AND cCamera <> null Then
			CheckContainer = true
			exit function
		end if
	end if
	CheckContainer = false
end function

'finding the line where is start selected layer
function FindStartLine(arr As Array[String],layerName as String) as Integer
	for i=0 to arr.UBound
		if arr[i].Find(" | ") > 1 then
			if layer_name == arr[i].Left(arr[i].Find(" | ")) then
				FindStartLine = i
				exit function
			end if
		end if
	next
	FindStartLine = -1
end function

'remove all camera animation
Sub RemoveAnimation()
	this.GetChannelsOfObject(arrChannels)
	for i=0 to arrChannels.UBound
		arrChannels[i].Delete()
	next
	this.FindSubcontainer(this.name & "_camTarget").GetChannelsOfObject(arrChannels)
	for i=0 to arrChannels.UBound
		arrChannels[i].Delete()
	next
	this.FindSubcontainer(this.name & "_camPosition").GetChannelsOfObject(arrChannels)
	for i=0 to arrChannels.UBound
		arrChannels[i].Delete()
	next
End Sub

'create keyframes by OnExecAction to aviod loong freezing the interface ;)
sub OnExecAction(buttonId As Integer)
	if buttonID == 1 AND NOT CheckContainer() then 
		cTarget = this.AddContainer(TL_DOWN)
		cCamera = this.AddContainer(TL_DOWN)
		cTarget.name = this.name & "_camTarget"
		cCamera.name = this.name & "_camPosition"
		Scene.UpdateSceneTree()
	elseif buttonId == 2 Then
		RemoveAnimation()
		
	elseif buttonId == 3 Then
		'open txt file and get strings to the arrInput
		
		if NOT CheckContainer() then
			println("There isn't camera container! Please push button 'Add containers' for create them.")
			exit sub
		end if
		filePath = GetParameterString("file")
		if NOT System.FileExists(filePath) then
			println("Can't FIND file " & filePath)
			exit sub
		end if
		if NOT System.LoadTextFile(filePath, input) then
			println("Can't READ file " & filePath)
			exit sub
		end if
		input.Split("\n",arrInput)
		'-----------------------------------------
		
		cTarget = this.FindSubContainer( this.name & "_camTarget" )
		cCamera = this.FindSubContainer( this.name & "_camPosition" )
		
		'get started parameters without animation
		if GetParameterBool("isnamecontainer") then
			layer_name = this.name
		else
			layer_name = GetParameterString("layer")
		end if
		startLine = FindStartLine(arrInput,layer_name)
		println("startLine = " & startLine)
		line = arrInput[startLine]
		line.Split(":",arrLine)
		line = arrLine[1]
		line.Trim()
		line.Split(" ",arrLine)
		'now, arrBase have all parameters:
		'0,1,2 - position xyz
		'3,4,5 - rotation xyz
		'6,7,8 - orientation xyz
		'9,10,11 - scaling xyz
		'spread it out:
		
		if GetParameterInt("type") == 0 then
			this.position.x = CDbl(arrLine[0])
			this.position.y = CDbl(arrLine[1])
			this.position.z = CDbl(arrLine[2])
			this.rotation.x = CDbl(arrLine[3])
			this.rotation.y = CDbl(arrLine[4])
			this.rotation.z = CDbl(arrLine[5])
		elseif GetParameterInt("type") == 1 then
			this.position.xyz = 0
			this.rotation.xyz = 0
			cCamera.position.x = CDbl(arrLine[0])
			cCamera.position.y = CDbl(arrLine[1])
			cCamera.position.z = CDbl(arrLine[2])
			cTarget.position.x = CDbl(arrLine[3])
			cTarget.position.y = CDbl(arrLine[4])
			cTarget.position.z = CDbl(arrLine[5])
			println("You have to twist camrera by 180 degree before set \"Direction tracking\"")
		end if
		
		RemoveAnimation()
		
		'lets insert the animation
		aefps = GetParameterInt("aefps")
		select case aefps
		case 0
			aefps = 50
		case 1
			aefps = 25
		end select
		
		if GetParameterInt("type") == 0 then
			chThisPos = this.FindOrCreateChannelOfObject("Position")
			chThisRot = this.FindOrCreateChannelOfObject("Rotation")
		elseif GetParameterInt("type") == 1 then
			chPosTarget = cTarget.FindOrCreateChannelOfObject("Position")
			chPosCamera = cCamera.FindOrCreateChannelOfObject("Position")
		end if
		
		'count all keyframe strings (count of all keyframes)
		countLines = 0
		stepScript = 0
		for i=startLine to arrInput.UBound
			line = arrInput[i]
			line.Trim()
			if line == "" then exit for
			countLines += 1
		next
		println("countLines = " & countLines)
	end if
end sub

'insert keyframes for one timestamp (one string in txt)
sub MakeStep()
	'tick for creating by OnExecAction()
	i = startLine + stepScript
	
	line = arrInput[i]
	line.Trim()
	if line == "" then exit sub
	line.Split(":",arrLine)
	curTime = Cint(arrLine[0])
	line = arrLine[1]
	line.Trim()
	line.Split(" ",arrLine)
	
	curTime = curTime/aefps
	
	if GetParameterInt("type") == 0 then
		'ONE-NODE CAMERA
		if arrLine[0] <> "-" AND arrLine[1] <> "-" AND arrLine[2] <> "-" then
			key = chThisPos.AddKeyframe(curTime)
			key.XyzValue  = CVertex(   CDbl(arrLine[0]),CDbl(arrLine[1]),CDbl(arrLine[2])   )
			if aefps == 25 then
				key2 = chThisPos.AddKeyframe(curTime-0.02)
				key2.XyzValue = key.XyzValue
			end if
		end if
		if arrLine[0] <> "-" AND arrLine[1] <> "-" AND arrLine[2] <> "-" then
			key = chThisRot.AddKeyframe(curTime)
			key.XyzValue  = CVertex(   CDbl(arrLine[3]),CDbl(arrLine[4])+180.0,180.0-CDbl(arrLine[5])   )
			if aefps == 25 then
				key2 = chThisRot.AddKeyframe(curTime-0.02)
				key2.XyzValue = key.XyzValue
			end if
		end if
	elseif GetParameterInt("type") == 1 then
		'TWO-NODE CAMERA
		if arrLine[0] <> "-" AND arrLine[1] <> "-" AND arrLine[2] <> "-" then
			key = chPosCamera.AddKeyframe(curTime)
			key.XyzValue  = CVertex(   CDbl(arrLine[0]),CDbl(arrLine[1]),CDbl(arrLine[2])   )
			if aefps == 25 then
				key2 = chPosCamera.AddKeyframe(curTime-0.02)
				key2.XyzValue = key.XyzValue
			end if
		end if
		if arrLine[12] <> "-" AND arrLine[13] <> "-" AND arrLine[14] <> "-" then
			key = chPosTarget.AddKeyframe(curTime)
			key.XyzValue  = CVertex(   CDbl(arrLine[12]),CDbl(arrLine[13]),CDbl(arrLine[14])   )
			if aefps == 25 then
				key2 = chPosTarget.AddKeyframe(curTime-0.02)
				key2.XyzValue = key.XyzValue
			end if
		end if
	end if
	
	'feedback information
	println("Step " & stepScript & "/" & (countLines-1) & " is done.")
end sub

sub OnExecPerField()
	if stepScript < countLines then
		MakeStep()
		stepScript += 1
	end if
end sub
