RegisterPluginVersion(1,0,1)
Dim info As String = "Sync Window Masks from source tree to multiple targets with the same structure. Developer: Dmitry Dudin, dudin.tv"

Dim cRoot, cRef As Container
Dim arrcItems, arrcSource, arrcTarget As Array[Container]
Dim iSourceId, iTargetId As Integer
Dim sContainerInfoSource, sContainerIdSource, sContainerIdTarget As String
Dim arrS As Array[String]
Dim hasMask As Boolean

Dim arrSourceTypes As Array[String]
arrSourceTypes.Push("First child is source, the rest are targets")
Dim SOURCE_TYPE_FIRST_CHILD = 0

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterContainer("root", "Root container (or this)")
	RegisterRadioButton("type", "Source type", SOURCE_TYPE_FIRST_CHILD, arrSourceTypes)
	RegisterPushButton("arrange", "Sync Traking Objects in Window Masks", 1)
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
	OnInit()
end sub

sub OnExecAction(buttonId As Integer)
	OnInit()
	if buttonId == 1 then
		SyncMasks()
	end if
end sub

sub SyncMasks()
	if GetParameterInt("type") == SOURCE_TYPE_FIRST_CHILD then
		arrcItems[0].GetContainerAndSubContainers(arrcSource, false)
		
		for i=1 to arrcItems.ubound
			arrcItems[i].GetContainerAndSubContainers(arrcTarget, false)
			for y=0 to arrcSource.ubound
				iSourceId = arrcSource[y].vizid
				hasMask = System.SendCommand("#" & iSourceId & "*WINDOW_MASK GET") <> ""
				if hasMask then
					println("arrcSource[y] = " & arrcSource[y].name)
					sContainerInfoSource = System.SendCommand("#" & iSourceId & "*WINDOW_MASK*TRACK*OBJECT GET")
					println("sContainerInfoSource = " & sContainerInfoSource)
					if sContainerInfoSource <> "" then
						sContainerInfoSource.Split(" ", arrS)
						sContainerIdSource = arrS[0]
						System.SendCommand("#" & arrcTarget[y].vizid & "*WINDOW_MASK*TRACK*OBJECT SET " & sContainerIdSource)
					end if
				end if
			next
		next
	end if
end sub

