RegisterPluginVersion(1,0,0)
Dim infoText As String = "Sort children by inner value. Author: Dmitry Dudin, dudin.tv"

Dim arrChilds, arrcTexts As Array[Container]
Dim arrGeoms As Array[Geometry]
Dim prevCount As Integer
Dim c, cRoot As Container

Dim searchButtonNames As Array[String]
searchButtonNames.push("viz path")
searchButtonNames.push("start with")

Dim emptyButtonNames As Array[String]
emptyButtonNames.push("Top")
emptyButtonNames.push("Sorted")
emptyButtonNames.push("Bottom")

sub OnInitParameters()
	RegisterInfoText(infoText)
	RegisterParameterContainer("root", "Root container (or this)")
	RegisterRadioButton("search_type", "Sub-child with value", 0, searchButtonNames)
	RegisterParameterString("sort_name", "Text container path", "child$subchild", 40, 999, "")
	RegisterParameterBool("sort_inverse", "Inverse", false)
	RegisterRadioButton("empty_order", "Place empty childs to", 1, emptyButtonNames)
end sub

sub OnInit()
	cRoot = GetParameterContainer("root")
	if (cRoot == null) then cRoot = this
	CollectChildren()
	CollectValueContainers()
end sub

sub OnExecPerField()
	if prevCount <> CountActiveChildren() then
		Update()
		prevCount = arrChilds.size
	end if
end sub

sub OnParameterChanged(parameterName As String)
	OnInit()
	Update()
end sub

sub OnGeometryChanged(geom As Geometry)
	Update()
end sub

Sub Update()
	SortByText()
	if GetParameterBool("sort_inverse") then Inverse()
	HandleEmpty()
	if NeedReorder() then ReorderChildren()
End Sub

Sub Swap(_c1 As Container, _c2 As Container)
	Dim _c As Container = _c1
	_c1 = _c2
	_c2 = _c
End Sub

Function CountActiveChildren() As Integer	
	Dim _count = 0
	for i=0 to cRoot.ChildContainerCount-1
		if cRoot.GetChildContainerByIndex(i).Active then
			_count += 1
		end if
	next
	CountActiveChildren = _count
End Function

Sub CollectChildren()
	arrChilds.Clear()
	for i=0 to cRoot.ChildContainerCount-1
		if cRoot.GetChildContainerByIndex(i).Active then
			arrChilds.Push(cRoot.GetChildContainerByIndex(i))
		end if
	next
End Sub

Function RegisterGeom(_geom As Geometry) As Boolean
	for i=0 to arrGeoms.ubound
		if arrGeoms[i].Uuid == _geom.Uuid then
			exit function
		end if
	next
	c.Geometry.RegisterTextChangedCallback()
	arrGeoms.push(_geom)
End Function

Sub CollectValueContainers()
	arrcTexts.Clear()
	for i=0 to arrChilds.ubound
		c = FindValueContainer(arrChilds[i])
		if c == null then c = arrChilds[i]
		arrcTexts.Push(c)
		if c.geometry <> null then RegisterGeom(c.geometry)
	next
End Sub

Sub SortByText()
	Dim _finish As Boolean
	Dim _s1, _s2 As String
	for j=0 to arrcTexts.ubound-1
		_finish = true
		for i=0 to arrcTexts.ubound-j-1
			_s1 = arrcTexts[i].Geometry.Text
			_s2 = arrcTexts[i+1].Geometry.Text
			if arrcTexts[i].Geometry == null then _s1 = ""		
			if arrcTexts[i+1].Geometry == null then _s2 = ""
			_s1.Substitute(",", ".", true)
			_s2.Substitute(",", ".", true)
			if CDbl(_s1) > CDbl(_s2) then
				Swap(arrcTexts[i], arrcTexts[i+1])
				Swap(arrChilds[i], arrChilds[i+1])
				_finish = false
			end if
		next
		if _finish then
			exit for
		end if
	next
End Sub

Function IsGeometryEmpty(_c As Container) As Boolean
	IsGeometryEmpty = _c.geometry == null OR _c.geometry.text == ""
End Function

Sub HandleEmpty()
	if GetParameterInt("empty_order") == 1 then exit sub	
	Dim hasEmptyStart As Boolean = IsGeometryEmpty(arrcTexts[0])
	Dim hasEmptyEnd As Boolean = IsGeometryEmpty(arrcTexts[arrcTexts.ubound])
	Dim hasEmpty As Boolean = hasEmptyStart OR hasEmptyEnd
	Dim hasAllEmpty As Boolean = hasEmptyStart AND hasEmptyEnd
	if (NOT hasEmpty) OR hasAllEmpty then exit sub
	
	if hasEmptyStart AND GetParameterInt("empty_order") == 0 then
		do while IsGeometryEmpty(arrcTexts[0])
			arrcTexts.push(arrcTexts[0])
			arrcTexts.erase(0)
			arrChilds.push(arrChilds[0])
			arrChilds.erase(0)
		loop
	end if
	
	if hasEmptyEnd AND GetParameterInt("empty_order") == 2 then
		do while IsGeometryEmpty(arrcTexts[arrcTexts.ubound])
			arrcTexts.insert(0, arrcTexts[arrcTexts.ubound])
			arrcTexts.pop()
			arrChilds.insert(0, arrChilds[arrChilds.ubound])
			arrChilds.pop()
		loop
	end if
End Sub

Sub PrintContainerArray(_arr As Array[Container])
	for i=0 to _arr.ubound
		println("C" & i & " = " & _arr[i].name)
	next
End Sub

Sub PrintCTextArray(_arr As Array[Container])
	for i=0 to _arr.ubound
		println("T:" & i & " = " & _arr[i].geometry.text)
	next
End Sub

Sub Inverse()
	for i=0 to CInt(arrChilds.ubound/2)
		Swap(arrcTexts[i], arrcTexts[arrChilds.ubound-i])
		Swap(arrChilds[i], arrChilds[arrChilds.ubound-i])
	next
End Sub

Function NeedReorder() As Boolean
	for i=0 to arrChilds.ubound
		if arrChilds[i].GetLocalIndex() <> i then
			NeedReorder = true
			exit function
		end if
	next
	NeedReorder = false
End Function

Sub ReorderChildren()
	for i=0 to arrChilds.ubound
		arrChilds[i].MoveTo(cRoot, TL_DOWN)
	next
	Scene.UpdateSceneTree()
End Sub

Function FindValueContainer(cChild as Container) as Container
	if GetParameterInt("search_type") == 0 then
		FindValueContainer = cChild.FindSubContainer(GetParameterString("sort_name"))
	elseif GetParameterInt("search_type") == 1 then
		Dim arrSubContainers As Array[Container]
		cChild.GetContainerAndSubContainers(arrSubContainers, false)
		arrSubContainers.Erase(0)
		for i=0 to arrSubContainers.ubound
			if arrSubContainers[i].name.StartsWith(GetParameterString("sort_name")) Then
				FindValueContainer = arrSubContainers[i]
				exit function
			end if
		next
		FindValueContainer = null
	end if
End Function
