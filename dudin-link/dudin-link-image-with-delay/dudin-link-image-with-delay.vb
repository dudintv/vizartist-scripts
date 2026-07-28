RegisterPluginVersion(1,0,0)

'''''''''''''''''''''''
' Change it as you need:
Dim TARGETS_AMOUNT = 3
'''''''''''''''''''''''

Dim cSource As Container
Dim arrcTargets As Array[Container]

dim tick = 0
dim hasImageChanged = false
dim previousImage As Image
dim imgName = "None"
dim prevName = "None"

sub OnInitParameters()
	RegisterParameterContainer("source", "Source (or this)")
	RegisterParameterInt("delay", "Sync delay (in frames)", 0, 0, 999999)
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
end sub

sub OnExecPerField()
	if cSource == null OR cSource.Texture == null then
		exit sub
	end if
		
	hasImageChanged = cSource.Texture.Image <> previousImage 
	

	if cSource.Texture.Image <> null then imgName = cSource.Texture.Image.Name
	if previousImage <> null then prevName = previousImage.Name
	
	if hasImageChanged then
		tick += 1
		
		if tick >= GetParameterInt("delay") then
			tick = 0
			SyncImage()
		end if
	end if
end sub

SyncImage()
sub SyncImage()
	if cSource == null OR cSource.Texture == null then
		exit sub
	end if
	
	previousImage = cSource.Texture.Image
	
	for i=0 to arrcTargets.ubound
		arrcTargets[i].Texture = cSource.Texture
	next
end sub
