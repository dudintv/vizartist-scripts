RegisterPluginVersion(1,1,0)
Dim info As String = "Manual Sync Window Masks from the first sub-tree to the rest sub-trees with the same structure. Developer: Dmitry Dudin, dudin.tv"

Dim cRoot, cRef As Container
Dim arrcItems, arrcSource, arrcTarget As Array[Container]
Dim iSourceId, iTargetId, iRelativeIndex, iContainerIdSource As Integer
Dim sContainerInfoSource, sContainerIdSource, sContainerIdTarget, console As String
Dim arrS As Array[String]
Dim hasMask As Boolean

Dim arrSourceTypes As Array[String]
arrSourceTypes.Push("Absolute (common, the same)")
arrSourceTypes.Push("Relative (within local sub-tree)")
Dim SOURCE_TYPE_ABSOLUTE = 0
Dim SOURCE_TYPE_RELATIVE = 1

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterContainer("root", "Root container (or this)")
	RegisterRadioButton("type", "Source type", SOURCE_TYPE_ABSOLUTE, arrSourceTypes)
	RegisterPushButton("arrange", "Sync Traking Objects in Window Masks", 1)
	RegisterParameterText("console", "", 900, 200)
end sub

sub OnInit()
	cRoot = GetParameterContainer("root")
	if cRoot == null then cRoot = this
	
	arrcItems.Clear()
	for i=0 to cRoot.ChildContainerCount-1
		arrcItems.Push(cRoot.GetChildContainerByIndex(i))
		println("arrcItems = " & arrcItems[i].name)
	next
end sub

sub OnParameterChanged(parameterName As String)
	if parameterName == "console" then exit sub
	OnInit()
end sub

sub OnExecAction(buttonId As Integer)
	OnInit()
	if buttonId == 1 then
		SyncMasks()
	end if
end sub

sub SyncMasks()
	console = ""
	
	arrcItems[0].GetContainerAndSubContainers(arrcSource, false)
	
	for i=1 to arrcItems.ubound
		arrcItems[i].GetContainerAndSubContainers(arrcTarget, false)
		for y=0 to arrcSource.ubound
			iSourceId = arrcSource[y].vizid
			hasMask = System.SendCommand("#" & iSourceId & "*WINDOW_MASK GET") <> ""
			if hasMask then
				console &= arrcItems[0].name & "/../" & arrcSource[y].name & "(#" & arrcSource[y].VizId & ") -> " & arrcItems[i].name & "/../" & arrcTarget[y].name & "(#" & arrcTarget[y].vizid & ")"
				sContainerInfoSource = System.SendCommand("#" & iSourceId & "*WINDOW_MASK*TRACK*OBJECT GET")
				if sContainerInfoSource <> "" then
					sContainerInfoSource.Split(" ", arrS)
					sContainerIdSource = arrS[0]
					
					
					if GetParameterInt("type") == SOURCE_TYPE_ABSOLUTE then
						' keep the same
						console &= " -- Tracking Object: " & arrcItems[0].name & "/../" & sContainerIdSource
					elseif GetParameterInt("type") == SOURCE_TYPE_RELATIVE then
						' find relative
						iContainerIdSource = CInt( sContainerIdSource.GetSubstring(1, sContainerIdSource.Length-1) )
						iRelativeIndex = FindContainerIndexByVizId(arrcSource, iContainerIdSource)
						cRef = arrcTarget[ iRelativeIndex ]
						sContainerIdSource = "#" & CStr(cRef.VizId)
						
						console &= " -- Tracking Object: " & arrcItems[i].name & "/../" & cRef.name & "(#" & cRef.VizId & ")"
					end if
					
					
					
					System.SendCommand("#" & arrcTarget[y].vizid & "*WINDOW_MASK*TRACK*OBJECT SET " & sContainerIdSource)
				else
					console &= " -- NO TRACKING OBJECT"
				end if
				console &= "\n"
			end if
		next
		console &= "\n"
	next
	
	Report()
end sub

function FindContainerIndexByVizId(_arrC As Array[Container], _vizId As Integer) As integer
	for _i=0 to _arrC.ubound
		if _arrC[_i].VizId = _vizId then
			FindContainerIndexByVizId = _i
			exit function
		end if
	next
	FindContainerIndexByVizId = 0
end function

sub Report()
	if console == "" then
		console = "Din't find containers with WinMask."
	else
		console = "Found containers with WindowMask (source -> target): \n\n" & console
	end if


	this.ScriptPluginInstance.SetParameterString("console",console)
end sub


