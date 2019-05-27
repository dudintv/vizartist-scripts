RegisterPluginVersion(1,1,0)
Dim infoText As String = "Arrange children in lines. Author: Dmitry Dudin, dudin.tv"

Dim max_in_row, max_col, count_rows As Integer
Dim row, col, prev_count As Integer
Dim gap_x, gap_y As Double
Dim xx,yy,zz, shift_xx, shift_yy As Double
Dim arr_childs, arr_ctexts As Array[Container]
Dim c As Container

Dim randomize As Boolean
Dim random_x, random_y, random_z As Double

Dim type_sort As Array[String]
type_sort.Push("by tree")
type_sort.Push("by name")
type_sort.Push("by text")
Dim SORT_BY_TREE As Integer = 0
Dim SORT_BY_NAME As Integer = 1
Dim SORT_BY_TEXT As Integer = 2

sub OnInitParameters()
	RegisterInfoText(infoText)
	RegisterParameterInt("max_in_line", "Max count in line", 1, 1, 999)
	RegisterParameterDouble("gap_x", "Distance X", 0, -999999, 999999)
	RegisterParameterDouble("gap_y", "Distance Y", 0, -999999, 999999)
	RegisterParameterBool("randomize", "Randomize", false)
	RegisterParameterDouble("random_x", "Random X", 0, 0, 999999)
	RegisterParameterDouble("random_y", "Random Y", 0, 0, 999999)
	RegisterParameterDouble("random_z", "Random Z", 0, 0, 999999)
	RegisterRadioButton("sort_type", "Sort", 0, type_sort)
	RegisterParameterString("sort_name", "Text container path", "item$container", 40, 999, "")
	RegisterParameterBool("sort_inverse", "Inverse", false)
end sub

sub OnInit()
	max_in_row = GetParameterInt("max_in_line")
	gap_x = GetParameterDouble("gap_x")
	gap_y = GetParameterDouble("gap_y")
	arr_childs.Clear()
	for i=0 to this.ChildContainerCount-1
		if this.GetChildContainerByIndex(i).Active then
			arr_childs.Push(this.GetChildContainerByIndex(i))
		end if
	next
	arr_ctexts.Clear()
	for i=0 to arr_childs.ubound
		c = arr_childs[i].FindSubContainer(GetParameterString("sort_name"))
		if c == null then c = arr_childs[i]
		arr_ctexts.Push(c)
		if GetParameterInt("sort_type")==SORT_BY_TEXT then
			c.Geometry.RegisterTextChangedCallback()
		else
			c.Geometry.UnregisterChangedCallback()
		end if 
	next
	
	randomize = GetParameterBool("randomize")
	random_x = GetParameterDouble("random_x")
	random_y = GetParameterDouble("random_y")
	random_z = GetParameterDouble("random_z")
	
	SendGuiParameterShow("random_x", CInt(randomize))
	SendGuiParameterShow("random_y", CInt(randomize))
	SendGuiParameterShow("random_z", CInt(randomize))
	SendGuiParameterShow("sort_name", CInt( GetParameterInt("sort_type")==SORT_BY_TEXT ))
end sub

sub OnExecPerField()
	arr_childs.Clear()
	for i=0 to this.ChildContainerCount-1
		if this.GetChildContainerByIndex(i).Active then
			arr_childs.Push(this.GetChildContainerByIndex(i))
		end if
	next
	
	if prev_count <> arr_childs.size then
		Update()
		prev_count = arr_childs.size
	end if
end sub

sub OnParameterChanged(parameterName As String)
	Update()
end sub

sub OnGeometryChanged(geom As Geometry)
	Update()
end sub

Sub Update()
	OnInit()
	if GetParameterInt("sort_type")==SORT_BY_NAME then SortByName()
	if GetParameterInt("sort_type")==SORT_BY_TEXT then SortByText()
	if GetParameterBool("sort_inverse") then Inverse()
	ArrangeInLines()
End Sub

Sub ArrangeInLines()
	count_rows = CInt(Ceil(arr_childs.size/CDbl(max_in_row)))
	max_col = CInt(Ceil(arr_childs.size/CDbl(count_rows)))
	shift_yy = gap_y*(count_rows-1)/2.0
	for i=0 to arr_childs.size-1
		col = i mod max_col
		row = CInt(i/max_col)
		xx = gap_x * col
		yy = gap_y * row
		zz = 0
		if row < count_rows-1 OR (arr_childs.size mod max_col) == 0 then
			shift_xx = gap_x * max_col/2.0 - gap_x/2.0
		else
			'last row
			shift_xx = gap_x * (arr_childs.size mod max_col)/2.0 - gap_x/2.0
		end if
		arr_childs[i].Position.xyz = CVertex(xx-shift_xx,yy-shift_yy,zz)
		
		if randomize then
			arr_childs[i].Position.x += random_x * Random() - random_x/2.0
			arr_childs[i].Position.y += random_y * Random() - random_y/2.0
			arr_childs[i].Position.z += random_z * Random() - random_z/2.0
		end if
	next
End Sub

Sub Swap(_c1 As Container, _c2 As Container)
	Dim _c As Container = _c1
	_c1 = _c2
	_c2 = _c
End Sub

Sub SortByText()
	Dim _finish As Boolean
	Dim _s1, _s2 As String
	for j=0 to arr_ctexts.ubound-1
		_finish = true
		for i=0 to arr_ctexts.ubound-j-1
			_s1 = arr_ctexts[i].Geometry.Text
			_s2 = arr_ctexts[i+1].Geometry.Text
			_s1.Substitute(",", ".", true)
			_s2.Substitute(",", ".", true)
			if CDbl(_s1) > CDbl(_s2) then
				Swap(arr_ctexts[i], arr_ctexts[i+1])
				Swap(arr_childs[i], arr_childs[i+1])
				_finish = false
			end if
		next
		if _finish then exit sub
	next
End Sub

Sub SortByName()
	Dim _finish As Boolean
	Dim _s1, _s2 As String
	for j=0 to arr_ctexts.ubound-1
		_finish = true
		for i=0 to arr_ctexts.ubound-j-1
			_s1 = arr_childs[i].Name
			_s2 = arr_childs[i+1].Name
			if _s1 > _s2 then
				Swap(arr_childs[i], arr_childs[i+1])
				_finish = false
			end if
		next
		if _finish then exit sub
	next
End Sub

Sub Inverse()
	for i=0 to CInt(arr_childs.ubound/2)
		Swap(arr_childs[i], arr_childs[arr_childs.ubound-i])
	next
End Sub
