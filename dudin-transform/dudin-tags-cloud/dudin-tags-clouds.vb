RegisterPluginVersion(1,0,0)

Dim c, c_source, c_template, c_logo, c_target, c_gabarit As Container
Dim s, separator As String
Dim arr_s As Array[String]

Dim arr_c As Array[Container]
Dim border_x, border_y, gabarit_width, gabarit_height As Double
Dim v1, v2, vv1, vv2 As Vertex
Dim gap As Double
Dim rand_treshhold As Double = 20.0 'percent_from_max

Structure Line
    width As Double
    arr_c As Array[Container]
End Structure
Dim arr_lines As Array[Line]
Dim line_shift_x, line_height_step As Double
Dim rand_x, rand_y As Double
Dim is_rand_order As Boolean

sub OnInitParameters()
	RegisterParameterContainer("source", "Text source")
	RegisterParameterString("separator", "Separator", "\\n", 10, 99, "")
	RegisterParameterContainer("template", "Template (optional)")
	RegisterParameterContainer("logo", "Logo (optional)")
	RegisterParameterContainer("target", "Target")
	RegisterParameterContainer("gabarit", "Gabarit")
	
	RegisterPushButton("do", "Split text to containers", 1)
	
	RegisterParameterDouble("gap", "Gap X", 0, 0, 999999)
	RegisterParameterBool("rand_order", "Random order", false)
	RegisterParameterDouble("rand_x", "Random pos X", 0, 0, 999999)
	RegisterParameterDouble("rand_y", "Random pos Y (%)", 0, 0, 100)
	RegisterParameterDouble("scale_by_width", "Scale by width %", 0, 0, 100)
	RegisterParameterDouble("rand_scale", "Random Scale", 0, 0, 999)
	RegisterParameterDouble("rand_alpha", "Random Alpha", 0, 0, 100)
	RegisterParameterBool("spread_animation", "Spread anim to dirs", false)
	RegisterParameterString("root_dir", "Root director", "", 10, 99, "")
end sub

sub OnInit()
	c_source = GetParameterContainer("source")
	c_gabarit = GetParameterContainer("gabarit")
	c_target = GetParameterContainer("target")
	if GetParameterContainer("template") <> null then
		c_template = GetParameterContainer("template")
	else
		c_template = c_source
	end if
	c_logo = GetParameterContainer("logo")
	gap = GetParameterDouble("gap")
	is_rand_order = GetParameterBool("rand_order")
	separator = GetParameterString("separator")
	separator.MakeLower()
	if separator == "vbnewline" OR separator == "\\n" Then
		separator = "\n"
	end if

	if GetParameterBool("spread_animation") then
		SendGuiParameterShow("root_dir",SHOW)
	else
		SendGuiParameterShow("root_dir",HIDE)
	end if
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		c_gabarit.GetTransformedBoundingBox(v1,v2)
		border_x = v1.x
		border_y = v2.y
		gabarit_width = v2.x - v1.x
		gabarit_height = v2.y - v1.y
		
		Populate()
		if is_rand_order then RandomOrder()
		Place()
		if GetParameterBool("spread_animation") then SpreadAnimations()
		Scene.UpdateSceneTree()
	end if
end sub

'-----------------------------------------------------------------

Function GetSubContainerByName(_c As Container, _name As String) As Array[Container]
	Dim _arr As Array[Container]
	_c.GetContainerAndSubContainers(_arr, false)
	_arr.Erase(0)
	for i=0 to _arr.ubound
		if _arr[i].name <> _name then
			_arr.Erase(i)
			i += 1
		end if
	next
	GetSubContainerByName = _arr
End Function

Dim decrease_scale_by_width As Double
Dim max_width As Double = 0
Sub Populate()
	s = c_source.geometry.text
	s.Trim()
	s.Split(separator, arr_s)
	
	c_target.DeleteChildren()
	arr_c.Clear()
		
	for i=arr_s.ubound to 0 step -1
		c = c_template.CopyTo(c_target, TL_DOWN)
		c.name = c_source.name & "_" & CStr(i)
		arr_s[i].Trim()
		if GetParameterContainer("template") <> null then
			Dim _text_containers As Array[Container] = GetSubContainerByName(c, "text")
			for y=0 to _text_containers.ubound
				_text_containers[y].geometry.text = arr_s[i]
			next
		else
			c.geometry.text = arr_s[i]
		end if
		c.CreateAlpha()
		c.Alpha.Value = 100 - Random()*GetParameterDouble("rand_alpha")
		c.RecomputeMatrix()
		if c.GetTransformedBoundingBoxDimensions().x > max_width then max_width = c.GetTransformedBoundingBoxDimensions().x
		
		arr_c.Push(c)
	next
	for i=0 to arr_c.ubound
		decrease_scale_by_width = c.scaling.x * (max_width - arr_c[i].GetTransformedBoundingBoxDimensions().x)/max_width * GetParameterDouble("scale_by_width")/100.0
		c.scaling.xyz = c.scaling.x + (Random()*GetParameterDouble("rand_scale")/100.0 - GetParameterDouble("rand_scale")/100.0/2.0) - decrease_scale_by_width
		c.RecomputeMatrix()
	next
	
End Sub

Dim arr_order As Array[Integer]
Dim rand_num, rand_order As Integer
Sub RandomOrder()
	arr_order.clear()
	for i=0 to arr_c.ubound
		arr_order.Push(i)
	next
	for i=0 to arr_c.ubound
		rand_order = CInt(Random()*(arr_order.size-1))
		rand_num = arr_order[rand_order]
		arr_order.Erase(rand_order)
		
		arr_c.Push(arr_c[rand_num])
		arr_c.Erase(rand_num)
		arr_c[rand_num].MoveTo(c_target, TL_DOWN)
	next
End Sub

Dim cur_line As Line
Dim cur_width, next_width As Double
Sub Place()
	'collect lines
	arr_lines.Clear()
	cur_line.width = 0
	for i=0 to arr_c.ubound
		cur_width = arr_c[i].GetTransformedBoundingBoxDimensions().x
		if i == arr_c.ubound then
			next_width = 0
		else
			next_width = arr_c[i+1].GetTransformedBoundingBoxDimensions().x
		end if
		cur_line.width += cur_width + gap
		cur_line.arr_c.Push(arr_c[i])
		if i >= arr_c.ubound OR cur_line.width + gap + next_width > gabarit_width then
			cur_line.width -= gap
			arr_lines.Push(cur_line)
			cur_line.width = 0
			cur_line.arr_c.Clear()
		end if
	next
	
	line_height_step = gabarit_height / (arr_lines.size+1)
	border_y = v2.y - line_height_step
	for i=0 to arr_lines.ubound
		'place a line
		border_x = v1.x
		line_shift_x = (gabarit_width - arr_lines[i].width)/2
		if arr_lines[i].arr_c.size <= 1 then
			'first separator
			PlaceLogo(border_x + line_shift_x, border_y + GenRandY())
		end if
		for y=0 to arr_lines[i].arr_c.ubound
			'place an element
			arr_lines[i].arr_c[y].GetTransformedBoundingBox(vv1, vv2)
			rand_x = Random()*GetParameterDouble("rand_x") - GetParameterDouble("rand_x")/2.0
			rand_y = GenRandY()
			arr_lines[i].arr_c[y].position.x = border_x + line_shift_x + (arr_lines[i].arr_c[y].position.x - vv1.x) + rand_x
			arr_lines[i].arr_c[y].position.y = border_y + rand_y
			border_x += arr_lines[i].arr_c[y].GetTransformedBoundingBoxDimensions().x + gap
			if y < arr_lines[i].arr_c.ubound then
				'add separator after all elements exept the last one
				PlaceLogo(border_x + line_shift_x, border_y + GenRandY())
			end if
		next
		if arr_lines[i].arr_c.size <= 1 then
			'last separator
			PlaceLogo(border_x + line_shift_x, border_y + GenRandY())
		end if
		border_y -= line_height_step
	next
End Sub

Sub PlaceLogo(_x As Double, _y As Double)
	if c_logo <> null then
		c = c_logo.CopyTo(c_target, TL_DOWN)
		'c.GetTransformedBoundingBox(vv1, vv2)
		c.position.x = _x
		c.position.y = _y
	end if
End Sub

Dim rootdir As Director
Sub SpreadAnimations()
	rootdir = Stage.FindDirector(GetParameterString("root_dir"))
	if rootdir == null then
		if Stage.RootDirector == null then
			rootdir = Stage.CreateRootDirector()
			rootdir.name = GetParameterString("root_dir")
		else
			rootdir = Stage.RootDirector.AddDirector(TL_NEXT)
			rootdir.name = GetParameterString("root_dir")
		end if
	end if
	for i=0 to arr_c.ubound
		PlaceAnimationToDir(arr_c[i], rootdir, arr_c[i].name)
	next
End Sub

Sub PlaceAnimationToDir(_c As Container, _rootdir As Director, _dirname As String)
	Dim _arr_subs As Array[Container]
	_c.GetContainerAndSubContainers(_arr_subs, false)
	_arr_subs.Erase(0)
	
	for i=0 to _arr_subs.ubound
		Dim _d As Director = _rootdir.FindSubDirector(_dirname)
		if _d == null then
			_d = _rootdir.AddDirector(TL_DOWN)
			_d.name = _dirname
		end if
		_arr_subs[i].MoveAllChannelsToDirector(_d)
	next
End Sub

'-----------------------------------------------------------------

Function GenRandY() As Double
	Dim _new_rand As Double
	do
		_new_rand = line_height_step*(  Random()*GetParameterDouble("rand_y") - GetParameterDouble("rand_y")/2.0  )/100.0
	loop while _new_rand < rand_y + line_height_step*GetParameterDouble("rand_y")/100.0*rand_treshhold/100.0 AND _new_rand > rand_y - line_height_step*GetParameterDouble("rand_y")/100.0*rand_treshhold/100.0
	GenRandY = _new_rand
End Function
