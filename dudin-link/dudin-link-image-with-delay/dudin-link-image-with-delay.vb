RegisterPluginVersion(1,1,0)

'''''''''''''''''''''''
' Change it as you need:
dim TARGETS_AMOUNT = 1
'''''''''''''''''''''''

dim cSource As Container
dim arrcTargets As Array[Container]

dim tick = 0
dim hasImageChanged = false
dim previousImage As Image
dim imgName = "None"
dim prevName = "None"

dim buttonNames As Array[String]
buttonNames.Push("Immediate")
buttonNames.Push("Delay")
buttonNames.Push("By button")
dim MODE_IMMEDIATE = 0
dim MODE_DELAY = 1
dim MODE_BUTTON = 2
dim currentMode As Integer

sub OnInitParameters()
	RegisterParameterContainer("source", "Source (or this)")
	RegisterRadioButton("mode", "Sync mode", MODE_DELAY, buttonNames)
	RegisterParameterInt("delay", "Sync delay (in frames)", 0, 0, 999999)
	RegisterPushButton("sync_now", "Sync now", 1)
	RegisterParameterBool("sync_init", "Sync immediate when init", true)
	for i=1 to TARGETS_AMOUNT
		RegisterParameterContainer("target" & i, "Target " & i)
	next
end sub

sub OnInit()
	cSource = GetParameterContainer("source")
	if cSource == null then cSource = this
	
	dim c As Container
	arrcTargets.Clear()
	for i=1 to TARGETS_AMOUNT
		c = GetParameterContainer("target" & CStr(i))
		if c <> null then
			arrcTargets.Push(c)
		end if
	next
	
	if GetParameterBool("sync_init") then
		SyncImage()
	end if
end sub

sub OnParameterChanged(parameterName As String)
	if parameterName == "sync_now" then exit sub
	OnInit()
	SendGuiParameterShow("delay", CInt(GetParameterInt("mode") == MODE_DELAY))
	SendGuiParameterShow("sync_now", CInt(GetParameterInt("mode") == MODE_BUTTON))
	SendGuiParameterShow("sync_init", CInt(GetParameterInt("mode") <> MODE_IMMEDIATE))
end sub

sub OnExecPerField()
	if cSource == null OR cSource.Texture == null then
		exit sub
	end if
		
	hasImageChanged = cSource.Texture.Image <> previousImage 

	if hasImageChanged then
		currentMode = GetParameterInt("mode")
		
		if currentMode == MODE_IMMEDIATE then
			' Mode 0: Sync right away
			SyncImage()
			
		elseif currentMode == MODE_DELAY then
			' Mode 1: Wait for X frames before syncing
			tick += 1
			if tick >= GetParameterInt("delay") then
				SyncImage()
			end if
			
		elseif currentMode == MODE_BUTTON then
			' Mode 2: Do nothing. Wait for the user to press the button.
		end if
	else
		' If the image hasn't changed (or was already synced), reset the tick counter
		tick = 0
	end if
end sub

sub SyncImage()
	if cSource == null OR cSource.Texture == null then
		exit sub
	end if
	
	previousImage = cSource.Texture.Image
	
	for i=0 to arrcTargets.ubound
		arrcTargets[i].Texture = cSource.Texture
	next
end sub


sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		SyncImage()
	end if
end sub

