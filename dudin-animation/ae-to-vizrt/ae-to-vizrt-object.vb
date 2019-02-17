RegisterPluginVersion(1,1,0)
Dim info As String = "Script created by Dmitry Dudin, dudintv@gmail.com
Version 15 February 2019
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
Dim crot As container
Dim input As String
Dim arrInput As Array[String]
Dim filePath As String
Dim key, key2 As Keyframe
Dim curTime As Double
Dim chPos,chRotation,chOrientation,chScale As Channel
Dim prev_position, prev_rotation, prev_orientation, prev_scaling As Vertex
Dim cur_x, cur_y, cur_z As Double
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
	RegisterPushButton("fold", "Create rotation sub-containers", 1)
	RegisterPushButton("delanim", "Delete animation", 2)
	RegisterFileSelector("file", "Text file coordinates", "", "", "*.txt")
	RegisterParameterString("layer","Layer name in AE", this.name, 50, 1000, "")
	RegisterRadioButton("aefps", "fps in AE", 0, buttonNames)
	RegisterPushButton("paste", "Paste animation from file", 3)
end sub

'check out existing helper sub-container
function CheckRotationFolding() as boolean
	crot = this.ChildContainer 
	if crot <> null AND crot.name == this.name & "_rotation" Then
		CheckRotationFolding = true
		exit function
	end if
	CheckRotationFolding = false
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

'remove all container animations
Sub RemoveAnimation()
	this.GetChannelsOfObject(arrChannels)
	for i=0 to arrChannels.UBound
		arrChannels[i].Delete()
	next
	this.FindSubcontainer(this.name & "_rotation").GetChannelsOfObject(arrChannels)
	for i=0 to arrChannels.UBound
		arrChannels[i].Delete()
	next
End Sub

sub OnExecAction(buttonId As Integer)
	if buttonID == 1 AND NOT CheckRotationFolding() then 
		crot = this.AddContainer(TL_NEXT)
		crot.name = this.name & "_rotation"
		crot.MoveTo(this,TL_DOWN)
		Scene.UpdateSceneTree()
		
	elseif buttonId == 2 Then
		RemoveAnimation()
		
	elseif buttonId == 3 Then
		'открываем файл и получаем строки текста в arrInput
		
		if NOT CheckRotationFolding() then
			println("There isn't rotation container! Please push button 'Add containers' for create them.")
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
		
		'get started parameters without creating an animation
		startLine = FindStartLine(arrInput,GetParameterString("layer"))
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
		this.position.x = CDbl(arrLine[0])
		this.position.y = CDbl(arrLine[1])
		this.position.z = CDbl(arrLine[2])
		crot.rotation.x = CDbl(arrLine[3])
		crot.rotation.y = CDbl(arrLine[4])
		crot.rotation.z = CDbl(arrLine[5])
		this.rotation.x = CDbl(arrLine[6])
		this.rotation.y = CDbl(arrLine[7])
		this.rotation.z = CDbl(arrLine[8])
		this.scaling.x  = CDbl(arrLine[9])/100.0
		this.scaling.y  = CDbl(arrLine[10])/100.0
		this.scaling.z  = CDbl(arrLine[11])/100.0
		
		prev_position = this.position.xyz
		prev_rotation = crot.rotation.xyz
		prev_orientation = this.rotation.xyz
		prev_scaling = this.scaling.xyz
		
		RemoveAnimation()
		
		'lets insert the animation
		aefps = GetParameterInt("aefps")
		select case aefps
		case 0
			aefps = 50
		case 1
			aefps = 25
		end select
		
		chPos = this.FindOrCreateChannelOfObject("Position")
		chRotation = crot.FindOrCreateChannelOfObject("Rotation")
		System.SendCommand("#" & crot.VizId & "*ROTATION_ORDER SET XYZ")
		chOrientation = this.FindOrCreateChannelOfObject("Rotation")
		System.SendCommand("#" & this.VizId & "*ROTATION_ORDER SET XYZ")
		chScale = this.FindOrCreateChannelOfObject("Scaling")
		
		'count all strings (count of all keyframes)
		countLines = 0
		for i=startLine to arrInput.UBound
			line = arrInput[i]
			line.Trim()
			if line == "" then exit for
			countLines += 1
		next
		
		'swith on the creating in OnExecPerField()
		stepScript = 0
	end if
end sub

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
	
	'POSITION
	if arrLine[0] <> "-" AND arrLine[1] <> "-" AND arrLine[2] <> "-" then
		key = chPos.AddKeyframe(curTime)
		cur_x = CDbl(arrLine[0])
		cur_y = CDbl(arrLine[1])
		cur_z = CDbl(arrLine[2])
		if arrLine[0] == "-" then cur_x = prev_position.x
		if arrLine[1] == "-" then cur_y = prev_position.y
		if arrLine[2] == "-" then cur_z = prev_position.z
		key.XyzValue = CVertex( cur_x, cur_y, cur_z )
		if aefps == 25 then
			key2 = chPos.AddKeyframe(curTime-0.02)
			key2.XyzValue = key.XyzValue
		end if
		prev_position = key.XyzValue
	end if
	
	'ROTATIION
	if arrLine[3] <> "-" OR arrLine[4] <> "-" OR arrLine[5] <> "-" then
		key = chRotation.AddKeyframe(curTime)
		cur_x = CDbl(arrLine[3])
		cur_y = CDbl(arrLine[4])
		cur_z = CDbl(arrLine[5])
		if arrLine[3] == "-" then cur_x = prev_rotation.x
		if arrLine[4] == "-" then cur_y = prev_rotation.y
		if arrLine[5] == "-" then cur_z = prev_rotation.z
		key.XyzValue = CVertex( cur_x, cur_y, cur_z )
		if aefps == 25 then
			key2 = chRotation.AddKeyframe(curTime-0.02)
			key2.XyzValue = key.XyzValue
		end if
		prev_rotation = key.XyzValue
	end if
	
	'ORIENTATION
	if arrLine[6] <> "-" AND arrLine[7] <> "-" AND arrLine[8] <> "-" then
		key = chOrientation.AddKeyframe(curTime)
		cur_x = CDbl(arrLine[6])
		cur_y = CDbl(arrLine[7])
		cur_z = CDbl(arrLine[8])
		if arrLine[6] == "-" then cur_x = prev_orientation.x
		if arrLine[7] == "-" then cur_y = prev_orientation.y
		if arrLine[8] == "-" then cur_z = prev_orientation.z
		key.XyzValue = CVertex( cur_x, cur_y, cur_z )
		if aefps == 25 then
			key2 = chOrientation.AddKeyframe(curTime-0.02)
			key2.XyzValue = key.XyzValue
		end if
		prev_orientation = key.XyzValue
	end if
	
	'SCALING
	if arrLine[9] <> "-" AND arrLine[10] <> "-" AND arrLine[11] <> "-" then
		key = chScale.AddKeyframe(curTime)
		cur_x = CDbl(arrLine[9])/100.0
		cur_y = CDbl(arrLine[10])/100.0
		cur_z = CDbl(arrLine[11])/100.0
		if arrLine[9]  == "-" then cur_x = prev_scaling.x
		if arrLine[10] == "-" then cur_y = prev_scaling.y
		if arrLine[11] == "-" then cur_z = prev_scaling.z
		key.XyzValue = CVertex( cur_x, cur_y, cur_z )
		if aefps == 25 then
			key2 = chScale.AddKeyframe(curTime-0.02)
			key2.XyzValue = key.XyzValue
		end if
		prev_scaling = key.XyzValue
	end if
		
	println("Step " & stepScript & "/" & (countLines-1) & " is done.")
end sub

sub OnExecPerField()
	if stepScript < countLines then
		MakeStep()
		stepScript += 1
	end if
end sub
