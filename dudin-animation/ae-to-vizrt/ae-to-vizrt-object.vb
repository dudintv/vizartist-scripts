Dim buttonNames As Array[String]
	buttonNames.Push("50 fps")
	buttonNames.Push("25 fps")
sub OnInitParameters()
	RegisterPushButton("fold", "Add containers for Rotation", 1)
	RegisterPushButton("delanim", "Delete all animation of ThisContainer", 2)
	RegisterFileSelector("file", "Text file coordinates", "", "", "*.txt")
	RegisterParameterString("layer","Layer name in AE", this.name, 50, 1000, "")
	RegisterRadioButton("aefps", "fps in AE", 0, buttonNames)
	RegisterPushButton("paste", "Paste animation from file", 3)
end sub
Dim cx, cy, cz As container
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

'исполнение скрипта
Dim stepScript As Integer = 0
Dim countSteps As Integer = 0
Dim countLines As Integer = 0
Dim stepStartLine As Integer
Dim stepCapability As Integer = 10


function CheckRotationFolding() as boolean
	cx = this.ChildContainer 
	if cx <> null AND cx.name == this.name&"_RotationX" Then
		cy = cx.ChildContainer 
		if cy <> null AND cy.name == this.name&"_RotationY" Then
			cz = cy.ChildContainer 
			if cz <> null AND cz.name == this.name&"_RotationZ" Then
				CheckRotationFolding = true
				exit function
			end if
		end if
	end if
	CheckRotationFolding = false
end function

function FindStartLine(arr As Array[String],layerName as String) as Integer
	for i=0 to arr.UBound
		if arr[i].Find(layerName) > -1 then
			FindStartLine = i
			exit function
		end if
	next
	FindStartLine = -1
end function

sub OnExecAction(buttonId As Integer)
	if buttonID == 1 AND NOT CheckRotationFolding() then 
		cx = this.AddContainer(TL_NEXT)
		cy = this.AddContainer(TL_NEXT)
		cz = this.AddContainer(TL_NEXT)
		cx.name = this.name & "_RotationX"
		cy.name = this.name & "_RotationY"
		cz.name = this.name & "_RotationZ"
		cx.MoveTo(this,TL_DOWN)
		cy.MoveTo(cx,TL_DOWN)
		cz.MoveTo(cy,TL_DOWN)
		
		Scene.UpdateSceneTree()
	elseif buttonId == 2 Then
		'удаляем ВСЮ анимацию
		this.GetChannelsOfObject(arrChannels)
		for i=0 to arrChannels.UBound
			arrChannels[i].Delete()
		next
		this.FindSubcontainer(this.name & "_RotationX").GetChannelsOfObject(arrChannels)
		for i=0 to arrChannels.UBound
			arrChannels[i].Delete()
		next
		this.FindSubcontainer(this.name & "_RotationY").GetChannelsOfObject(arrChannels)
		for i=0 to arrChannels.UBound
			arrChannels[i].Delete()
		next
		this.FindSubcontainer(this.name & "_RotationZ").GetChannelsOfObject(arrChannels)
		for i=0 to arrChannels.UBound
			arrChannels[i].Delete()
		next
	elseif buttonId == 3 Then
		'открываем файл и получаем строки текста в arrInput
		
		if NOT CheckRotationFolding() then
			println("There isn't rotation container! Please push button for create them.")
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
		
		'устанавливаем базовые парамтеры (без анимации)
		startLine = FindStartLine(arrInput,GetParameterString("layer"))
		line = arrInput[startLine]
		line.Split(":",arrLine)
		line = arrLine[1]
		line.Trim()
		line.Split(" ",arrLine)
		'теперь в arrBase хранятся все базовые параметры:
		'0,1,2 - позиция xyz
		'3,4,5 - поворот xyz
		'6,7,8 - ориентация xyz
		'9,10,11 - масштаб xyz
		
		'расставляем базовые параметры
		this.position.x = CDbl(arrLine[0])
		this.position.y = CDbl(arrLine[1])
		this.position.z = CDbl(arrLine[2])
		cx.rotation.x   = CDbl(arrLine[3])
		cy.rotation.y   = CDbl(arrLine[4])
		cz.rotation.z   = CDbl(arrLine[5])
		this.rotation.x = CDbl(arrLine[6])
		this.rotation.y = CDbl(arrLine[7])
		this.rotation.z = CDbl(arrLine[8])
		this.scaling.x  = CDbl(arrLine[9])/100.0
		this.scaling.y  = CDbl(arrLine[10])/100.0
		this.scaling.z  = CDbl(arrLine[11])/100.0
		
		'удалить уже имеющуюся анимацию
		'....TODO....
		
		'вставляем всю анимацию
		aefps = GetParameterInt("aefps")
		select case aefps
		case 0
			aefps = 50
		case 1
			aefps = 25
		end select
		
		chPos = this.FindOrCreateChannelOfObject("Position")
		chRot = this.FindOrCreateChannelOfObject("Rotation")
		chRotX = cx.FindOrCreateChannelOfObject("Rotation")
		chRotY = cy.FindOrCreateChannelOfObject("Rotation")
		chRotZ = cz.FindOrCreateChannelOfObject("Rotation")
		chScale = this.FindOrCreateChannelOfObject("Scaling")
		
		'подсчитаем кол-во строк и посчитаем за сколько шагов их обработаем
		countLines = 0
		for i=startLine to arrInput.UBound
			line = arrInput[i]
			line.Trim()
			if line == "" then exit for
			countLines += 1
		next
		stepScript = 0
	end if
end sub

sub MakeStep()
	'для логики покадрового производства
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
		key = chPos.AddKeyframe(curTime)
		key.XyzValue  = CVertex(   CDbl(arrLine[0]),CDbl(arrLine[1]),CDbl(arrLine[2])   )
		if aefps == 25 then
			key2 = chPos.AddKeyframe(curTime-0.02)
			key2.XyzValue = key.XyzValue
		end if
	end if
	if arrLine[3] <> "-" then
		key = chRotX.AddKeyframe(curTime)
		key.XyzValue  = CVertex(   CDbl(arrLine[3]),0,0   )
		if aefps == 25 then
			key2 = chRotX.AddKeyframe(curTime-0.02)
			key2.XyzValue = key.XyzValue
		end if
	end if
	if arrLine[4] <> "-" then
		key = chRotY.AddKeyframe(curTime)
		key.XyzValue  = CVertex(   0,CDbl(arrLine[4]),0   )
		if aefps == 25 then
			key2 = chRotY.AddKeyframe(curTime-0.02)
			key2.XyzValue = key.XyzValue
		end if
	end if
	if arrLine[5] <> "-" then
		key = chRotZ.AddKeyframe(curTime)
		key.XyzValue  = CVertex(   0,0,CDbl(arrLine[5])   )
		if aefps == 25 then
			key2 = chRotZ.AddKeyframe(curTime-0.02)
			key2.XyzValue = key.XyzValue
		end if
	end if
	if arrLine[6] <> "-" AND arrLine[7] <> "-" AND arrLine[8] <> "-" then
		key = chRot.AddKeyframe(curTime)
		key.XyzValue  = CVertex(   CDbl(arrLine[6]),CDbl(arrLine[7]),CDbl(arrLine[8])   )
		if aefps == 25 then
			key2 = chRot.AddKeyframe(curTime-0.02)
			key2.XyzValue = key.XyzValue
		end if
	end if
	if arrLine[9] <> "-" AND arrLine[10] <> "-" AND arrLine[11] <> "-" then
		key = chScale.AddKeyframe(curTime)
		key.XyzValue  = CVertex(   CDbl(arrLine[9])/100.0,CDbl(arrLine[10])/100.0,CDbl(arrLine[11])/100.0   )
		if aefps == 25 then
			key2 = chScale.AddKeyframe(curTime-0.02)
			key2.XyzValue = key.XyzValue
		end if
	end if
		
	println("Step " & stepScript & "/" & (countLines-1) & " is done.")
end sub

sub OnExecPerField()
	if stepScript < countLines then
		MakeStep()
		stepScript += 1
	end if
end sub
