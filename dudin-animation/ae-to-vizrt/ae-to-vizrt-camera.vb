RegisterPluginVersion(1,0,1)
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
Dim chPosTarget, chPosCamera As Channel
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

'interface
Dim buttonNames As Array[String]
	buttonNames.Push("50 fps")
	buttonNames.Push("25 fps")
sub OnInitParameters()
	RegisterPushButton("fold", "Add containers for Camera", 1)
	RegisterPushButton("delanim", "Delete animation", 2)
	RegisterFileSelector("file", "Text file coordinates", "", "", "*.txt")
	RegisterParameterString("layer","Layer name in AE", this.name, 50, 1000, "")
	RegisterRadioButton("aefps", "FPS", 0, buttonNames)
	RegisterPushButton("paste", "Import animation", 3)
end sub

'checking of existing helper containers
function CheckContainer() as boolean
	cTarget = this.FindSubcontainer(this.name & "_camTarget")
	cCamera = this.FindSubcontainer(this.name & "_camPosition")
	if cTarget <> null AND cCamera <> null Then
		CheckContainer = true
		exit function
	end if
	CheckContainer = false
end function

'finding the line where is start selected layer
function FindStartLine(arr As Array[String],layerName as String) as Integer
	for i=0 to arr.UBound
		if arr[i].Find(layerName) > -1 then
			FindStartLine = i
			exit function
		end if
	next
	FindStartLine = -1
end function

'remove all camera animation
Sub RemoveAnimation()
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
		startLine = FindStartLine(arrInput,GetParameterString("layer"))
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
		cTarget.position.x = CDbl(arrLine[0])
		cTarget.position.y = CDbl(arrLine[1])
		cTarget.position.z = CDbl(arrLine[2])
		cCamera.position.x = CDbl(arrLine[3])
		cCamera.position.y = CDbl(arrLine[4])
		cCamera.position.z = CDbl(arrLine[5])
		
		RemoveAnimation()
		
		'lets insert the animation
		aefps = GetParameterInt("aefps")
		select case aefps
		case 0
			aefps = 50
		case 1
			aefps = 25
		end select
		
		chPosTarget = cTarget.FindOrCreateChannelOfObject("Position")
		chPosCamera = cCamera.FindOrCreateChannelOfObject("Position")
		
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
	
	if arrLine[0] <> "-" AND arrLine[1] <> "-" AND arrLine[2] <> "-" then
		key = chPosTarget.AddKeyframe(curTime)
		key.XyzValue  = CVertex(   CDbl(arrLine[0]),CDbl(arrLine[1]),CDbl(arrLine[2])   )
		if aefps == 25 then
			key2 = chPosTarget.AddKeyframe(curTime-0.02)
			key2.XyzValue = key.XyzValue
		end if
	end if
	if arrLine[3] <> "-" AND arrLine[4] <> "-" AND arrLine[5] <> "-" then
		key = chPosCamera.AddKeyframe(curTime)
		key.XyzValue  = CVertex(   CDbl(arrLine[3]),CDbl(arrLine[4]),CDbl(arrLine[5])   )
		if aefps == 25 then
			key2 = chPosCamera.AddKeyframe(curTime-0.02)
			key2.XyzValue = key.XyzValue
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
