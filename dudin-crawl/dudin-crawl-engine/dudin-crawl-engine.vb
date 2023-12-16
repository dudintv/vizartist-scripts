RegisterPluginVersion(1,1,4)
Dim info As String = "Crawl
Developer: Dmitry Dudin
http://dudin.tv"

Dim cTemplates, cProduction, cArea, cInput As Container
Dim animationStatus, intentionStatus, delimiter, console As String

Dim startX, endX As Double
Dim v1, v2 As Vertex

Dim sourceText As String
Dim arrSourteText As Array[String]
Structure CrawlItem
	sourceText As String
	type As String
	text As String
	cTemplate As Container
End Structure
Dim arrCrawlItems As Array[CrawlItem]
Dim currentItemIndex As Integer
Dim arrActiveItems As Array[Container]

Dim sourceMode As Integer
Dim sourceModes As Array[String]
sourceModes.Push("text")
sourceModes.Push("container")
sourceModes.Push("SHM")
Dim SOURCE_MODE_TEXT = 0
Dim SOURCE_MODE_CONTAINER = 1
Dim SOURCE_MODE_SHM = 2

Dim BUTTON_INIT = 1
Dim BUTTON_START = 2
Dim BUTTON_PAUSE = 3
Dim BUTTON_CONTINUE = 4
Dim BUTTON_FINISH = 5

sub OnInitParameters()
	RegisterParameterContainer("templates", "Templates")
	RegisterParameterContainer("production", "Production")
	RegisterParameterContainer("area", "Bounding box area")
	RegisterParameterDouble("speed", "Speed", 100, 0, 9999)
	RegisterParameterDouble("gap", "Gap", 10, 0, 9999)
	RegisterParameterString("delimiter", "Source delimiter", "\\n", 100, 999, "")
	RegisterRadioButton("source_mode", "Source mode", 0, sourceModes)
	RegisterParameterText("source_text", "Source text", 100, 100)
	RegisterParameterContainer("source_container", "Source text container")
	RegisterParameterString("source_shm", "Source SHM variable", "", 100, 999, "")
	RegisterPushButton("init", "Initialize", BUTTON_INIT)
	RegisterPushButton("start", "Start", BUTTON_START)
	RegisterPushButton("pause", "Pause", BUTTON_PAUSE)
	RegisterPushButton("continue", "Continue", BUTTON_CONTINUE)
	RegisterPushButton("finish", "Finish gracefully", BUTTON_FINISH)
	RegisterParameterText("console", "", 100, 100)
end sub

sub OnParameterChanged(parameterName As String)
	OnInit()
	SendGuiParameterShow("source_text", CInt(sourceMode == SOURCE_MODE_TEXT))
	SendGuiParameterShow("source_container", CInt(sourceMode == SOURCE_MODE_CONTAINER))
	SendGuiParameterShow("source_shm", CInt(sourceMode == SOURCE_MODE_SHM))
	if(parameterName == "source_text") then PrepareItems()

	if sourceMode == SOURCE_MODE_CONTAINER then
		System.Map.UnregisterChangedCallback(GetParameterString("source_shm"))

		cInput = GetParameterContainer("source_container")
		if cInput <> null then cInput.geometry.RegisterTextChangedCallback()
	elseif sourceMode == SOURCE_MODE_SHM then
		if cInput <> null then cInput.geometry.UnregisterChangedCallback()

		System.Map.RegisterChangedCallback(GetParameterString("source_shm"))
	end if
end sub
sub OnInit()
	sourceMode = GetParameterInt("source_mode")
	cTemplates = GetParameterContainer("templates")
	cProduction = GetParameterContainer("production")
	cArea = GetParameterContainer("area")
	delimiter = GetParameterString("delimiter")
	if delimiter == "\\n" then delimiter = "\n"
	if cTemplates == null OR cProduction == null OR cArea == null Then
		exit sub
	end if

	cArea.GetTransformedBoundingBox(v1, v2)
	startX = v2.x
	endX = v1.x

	Dim arrEventNames As Array[String]
	arrEventNames.Push("NEW ITEM")
	Eventpool.registerEvents("Crawl", arrEventNames)
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId = BUTTON_INIT then
		OnInit()
		CleanupProduction()
		PrepareItems()
		animationStatus = "stop"
	elseif buttonId = BUTTON_START then
		Start()
	elseif buttonId = BUTTON_PAUSE then
		animationStatus = "pause"
	elseif buttonId = BUTTON_CONTINUE then
		animationStatus = "play"
	elseif buttonId = BUTTON_FINISH then
		intentionStatus = "stop"
	end if
end sub

sub OnExecPerField()
	if animationStatus == "play" then
		for i=0 to arrActiveItems.ubound
			arrActiveItems[i].position.x -= GetParameterDouble("speed")/50.0
			arrActiveItems[i].RecomputeMatrix()
		next
		DetectAndDeleteOutsiders() 'manage deleting side
		DetectAndCreateNextItem() 'manage creating side
	end if
end sub

sub OnGeometryChanged(geom As Geometry)
	PrepareItems()
end sub

sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	PrepareItems()
end sub

'-----------------------------------------

Sub CleanupProduction()
	cProduction.DeleteChildren()
	cProduction.position.x = 0
End Sub

Sub PrepareItems()
	if sourceMode == SOURCE_MODE_TEXT then
		sourceText = GetParameterString("source_text")
	elseif sourceMode == SOURCE_MODE_CONTAINER then
		sourceText = GetParameterContainer("source_container").geometry.text
	elseif sourceMode == SOURCE_MODE_SHM then
		sourceText = CStr(System.Map[GetParameterString("source_shm")])
	end if
	sourceText.Trim()
	sourceText.Split(delimiter, arrSourteText)

	arrCrawlItems.Clear()
	if sourceText == "" then exit sub

	for i=0 to arrSourteText.ubound
		Dim item As CrawlItem
		item.sourceText = arrSourteText[i]

		item.text = item.sourceText
		item.text.trim()
		Dim containerTagName = "container"
		if item.text.StartsWith("<" & containerTagName) then
			item.type = "container"
			Dim openTagIndex = 2 + containerTagName.length
			Dim closeTagIndex = item.text.Find(">")
			Dim sElementName = item.text.GetSubstring(openTagIndex, closeTagIndex - openTagIndex)
			item.cTemplate = scene.findContainer(sElementName)
		elseif item.text <> "" Then
			item.type = "text"
			item.cTemplate = cTemplates.FirstChildContainer
		end if
		arrCrawlItems.Push(item)
		PrintItem(item)
	next
End Sub

Sub InsertNextItem()
	println("InsertNextItem| intentionStatus = " & intentionStatus)
	if intentionStatus == "stop" Then
		'do not insert a new item
		exit sub
	end if

	currentItemIndex += 1
	currentItemIndex = currentItemIndex mod arrCrawlItems.size

	Dim cLast = cProduction.FirstChildContainer
	Dim newItemPositionX = startX
	if cLast <> null then
		newItemPositionX = cLast.position.x + cLast.GetTransformedBoundingBoxDimensions().x + GetParameterDouble("gap")
	end if
	
	if currentItemIndex > arrCrawlItems.ubound then exit sub

	Dim cNew As Container = arrCrawlItems[currentItemIndex].cTemplate.CopyTo(cProduction, TL_DOWN)
	cNew.position.xyz = CVertex(newItemPositionX, 0, 0)
	if arrCrawlItems[currentItemIndex].type == "container" Then
		'nothing to change
	else
		cNew.geometry.text = arrCrawlItems[currentItemIndex].text
	end if
	cNew.RecomputeMatrix()
	arrActiveItems.push(cNew)
End Sub

Sub Start()
	intentionStatus = ""
	currentItemIndex = -1 'to start from "0"
	InsertNextItem()
	animationStatus = "play"
End Sub

Sub DetectAndDeleteOutsiders()
	for i=0 to arrActiveItems.ubound
		arrActiveItems[i].GetTransformedBoundingBox(v1, v2)
		if v2.x < endX then
			arrActiveItems[i].Delete()
			arrActiveItems.Erase(i)
			i -= 1
		end if
	next

	if arrActiveItems.size <= 0 Then
		animationStatus = "stop"
	end if
End Sub

Sub DetectAndCreateNextItem()
	cProduction.GetTransformedBoundingBox(v1, v2)
	if v2.x < startX - GetParameterDouble("gap") then
		InsertNextItem()
	end if
End Sub

Sub PrintItem(item As CrawlItem)
	println("type=" & item.type & "| text=" & item.text & "|c=" & item.cTemplate.name)
End Sub

Sub Report()
	console = ""
	console &= "Status: " & animationStatus
	this.ScriptPluginInstance.SetParameterString("console",console)
End Sub
