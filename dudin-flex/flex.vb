Dim info As String = "Разработчик: Дудин Дмитрий.
Версия 0.32 (15 июня 2018)
---------------------------
"

Dim c_gabarit As Container
Dim arr_axis As Array[String]
arr_axis.Push(" X ")
arr_axis.Push(" Y ")
Dim arr_gabarit_source As Array[String]
arr_gabarit_source.Push("весь размер")
arr_gabarit_source.Push("брать с первого подобъекта")
Dim arr_justify As Array[String]
arr_justify.Push("start")
arr_justify.Push("end")
arr_justify.Push("center")
arr_justify.Push("space-between")
arr_justify.Push("space-around")
Dim arr_align As Array[String]
arr_align.Push("free")
arr_align.Push("bottom")
arr_align.Push("center")
arr_align.Push("top")
Dim gap_min_param, gap_param, gap_shift As Double
Dim treshhold As Double = 0.001

Dim children As Array[Container]
Dim gabarit, child_gabarit, v1, v2, child_v1, child_v2, item_gabarit As Vertex
Dim count As Integer = 1
Dim mode_axis, mode_gabarit_source, mode_justify, mode_align As Integer
Dim width_step, sum_children_width, sum_children_height, gap, start As Double
Dim arr_width, arr_height, arr_shift_x, arr_shift_y As Array[Double]

sub OnInitParameters()
	RegisterParameterContainer("gabarit", "Габариты")
	RegisterRadioButton("axis", "Ось", 0, arr_axis)
	RegisterRadioButton("gabarit_source", "Размеры элементов", 0, arr_gabarit_source)
	RegisterRadioButton("justify", "Распред.", 0, arr_justify)
	RegisterRadioButton("align", "Выравнивание", 0, arr_align)
	RegisterParameterDouble("gap", "Зазор в % остатка", 0, -1000, 1000)
	RegisterParameterDouble("gap_min", "Мин. зазор", 0, 0, 1000)
end sub

sub OnInit()
	c_gabarit = GetParameterContainer("gabarit")
	mode_axis = GetParameterInt("axis")
	mode_gabarit_source = GetParameterInt("gabarit_source")
	mode_justify = GetParameterInt("justify")
	mode_align = GetParameterInt("align")
	gap_param = GetParameterDouble("gap")
	gap_min_param = GetParameterDouble("gap_min")
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

'#############################################################
'#############################################################
'#############################################################
Function GetChildGabarit(child As Container) As Vertex
	if (mode_gabarit_source == 1) AND child.ChildContainerCount > 0 then
		GetChildGabarit = child.FirstChildContainer.GetTransformedBoundingBoxDimensions()
	else
		GetChildGabarit = child.GetTransformedBoundingBoxDimensions()
	end if
End Function


Sub calc_gap_and_start(gabarit_size As Double, sum_children_size As Double)
	gap_shift = (gabarit_size - sum_children_size)*(gap_param/100)/(count-1)
	if mode_justify = 0 then
		'start
		gap = 0 + gap_shift
	elseif mode_justify = 1 then
		'end
		gap = 0 + gap_shift
	elseif mode_justify = 2 then
		'center
		gap = 0 + gap_shift
	elseif mode_justify = 3 then
		'space-between
		gap = (gabarit_size - sum_children_size)/(count-1)
	elseif mode_justify = 4 then
		'space-around
		
		gap = (gabarit_size - sum_children_size)/(count+1)
		
		gap_shift = (gabarit_size - sum_children_size - gap*(count-1))*(gap_param/100)/(count-1)
		
		gap += gap_shift
		
	end if
	
	if gap < gap_min_param then
		if (gabarit_size - sum_children_size)/(count-1) < gap_min_param then
			gap = 0
		else
			gap = gap_min_param
		end if
	end if
	
	if mode_justify = 0 then
		'start
		start = 0
	elseif mode_justify = 1 then
		'end
		start = gabarit_size - sum_children_size - gap*(count-1)
	elseif mode_justify = 2 then
		'center
		start = (gabarit_size - sum_children_size)/2.0 - gap*(count-1)/2.0
	elseif mode_justify = 3 then
		'space-between
		start = (gabarit_size - sum_children_size)/2.0 - gap*(count-1)/2.0
	elseif mode_justify = 4 then
		'space-around
		start = (gabarit_size - sum_children_size)/2.0 - gap*(count-1)/2.0
	end if
End Sub


Sub update()
	gabarit = c_gabarit.GetTransformedBoundingBoxDimensions()
	c_gabarit.GetTransformedBoundingBox(v1,v2)
	v1 = c_gabarit.WorldPosToLocalPos(v1)
	v2 = c_gabarit.WorldPosToLocalPos(v2)
	
	children.clear
	for i=0 to this.ChildContainerCount-1
		item_gabarit = this.GetChildContainerByIndex(i).GetTransformedBoundingBoxDimensions()
		
		if this.GetChildContainerByIndex(i).active AND item_gabarit.X > treshhold AND item_gabarit.Y > treshhold then
			children.push(this.GetChildContainerByIndex(i))
		end if
	next
	
	
	arr_width.clear
	arr_height.clear
	arr_shift_x.clear
	arr_shift_y.clear
	sum_children_width = 0
	sum_children_height = 0
	for i=0 to children.UBound
		if (mode_gabarit_source == 1) AND children[i].ChildContainerCount > 0 then
			children[i].FirstChildContainer.GetBoundingBox(child_v1,child_v2)
		else
			children[i].GetBoundingBox(child_v1,child_v2)
		end if
		child_gabarit = GetChildGabarit(children[i])
		
		arr_width.push(child_gabarit.X)
		arr_height.push(child_gabarit.Y)
		sum_children_width += child_gabarit.X
		sum_children_height += child_gabarit.Y
				
		arr_shift_x.push( -child_gabarit.X/2.0 -(child_v2.x+child_v1.x)/2.0 )
		arr_shift_y.push( -child_gabarit.Y/2.0 -(child_v2.y+child_v1.y)/2.0 )
		for y=0 to i
			arr_shift_x[i] += arr_width[y]
			arr_shift_y[i] += arr_height[y]
		next
	next
	
	count = children.size
	if mode_axis == 0 then 'X
		calc_gap_and_start(gabarit.X, sum_children_width)
	elseif mode_axis == 1 then 'Y
		calc_gap_and_start(gabarit.Y, sum_children_height)
	end if
End Sub

sub OnExecPerField()
	update
	for i=0 to children.UBound
		child_gabarit = GetChildGabarit(children[i])
		
		if mode_axis == 0 then
			'X
			children[i].Position.X = v1.x + start + i*gap + arr_shift_x[i]
			if mode_align == 1 then
				children[i].Position.Y = v1.y + child_gabarit.Y/2.0
			elseif mode_align == 2 then
				children[i].Position.Y = (v1.y+v2.y)/2.0
			elseif mode_align == 3 then
				children[i].Position.Y = v2.y - child_gabarit.Y/2.0
			end if
		elseif mode_axis == 1 then
			'Y
			children[i].Position.Y = v2.y - start - i*gap - arr_shift_y[i]
			if mode_align == 1 then
				children[i].Position.X = v1.x + child_gabarit.X/2.0
			elseif mode_align == 2 then
				children[i].Position.X = (v1.x+v2.x)/2.0
			elseif mode_align == 3 then
				children[i].Position.X = v2.x - child_gabarit.X/2.0
			end if
		end if
	next
end sub