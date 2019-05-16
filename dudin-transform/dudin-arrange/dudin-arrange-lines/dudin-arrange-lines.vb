RegisterPluginVersion(1,0,1)
Dim infoText As String = "Arrange children in lines.
Author: Dmitry Dudin"

Dim max_in_row, max_col, count_rows As Integer
Dim row, col, prev_count As Integer
Dim gap_x, gap_y As Double
Dim xx,yy,zz, shift_xx As Double
Dim arr_childs As Array[Container]

Dim randomize As Boolean
Dim random_x, random_y, random_z As Double

sub OnInitParameters()
	RegisterInfoText(infoText)
	RegisterParameterInt("max_in_line", "Max count in line", 1, 1, 999)
	RegisterParameterDouble("gap_x", "Distance X", 0, -999999, 999999)
	RegisterParameterDouble("gap_y", "Distance Y", 0, -999999, 999999)
	RegisterParameterBool("randomize", "Randomize", false)
	RegisterParameterDouble("random_x", "Random X", 0, 0, 999999)
	RegisterParameterDouble("random_y", "Random Y", 0, 0, 999999)
	RegisterParameterDouble("random_z", "Random Z", 0, 0, 999999)
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
	
	randomize = GetParameterBool("randomize")
	random_x = GetParameterDouble("random_x")
	random_y = GetParameterDouble("random_y")
	random_z = GetParameterDouble("random_z")
	
	SendGuiParameterShow("random_x", CInt(randomize))
	SendGuiParameterShow("random_y", CInt(randomize))
	SendGuiParameterShow("random_z", CInt(randomize))
end sub

sub OnParameterChanged(parameterName As String)
	OnInit()
	ArrangeInLines()
end sub

Sub ArrangeInLines()
	count_rows = CInt(Ceil(arr_childs.size/CDbl(max_in_row)))
	max_col = CInt(Ceil(arr_childs.size/CDbl(count_rows)))
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
		arr_childs[i].Position.xyz = CVertex(xx-shift_xx,yy,zz)
		
		if randomize then
			arr_childs[i].Position.x += random_x * Random() - random_x/2.0
			arr_childs[i].Position.y += random_y * Random() - random_y/2.0
			arr_childs[i].Position.z += random_z * Random() - random_z/2.0
		end if
	next
End Sub

sub OnExecPerField()
	arr_childs.Clear()
	for i=0 to this.ChildContainerCount-1
		if this.GetChildContainerByIndex(i).Active then
			arr_childs.Push(this.GetChildContainerByIndex(i))
		end if
	next
	
	if prev_count <> arr_childs.size then
		OnInit()
		ArrangeInLines()
		prev_count = arr_childs.size
	end if
end sub
