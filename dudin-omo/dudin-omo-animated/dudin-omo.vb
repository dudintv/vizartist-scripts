RegisterPluginVersion(1,3,1)

' pos=(0,0,0);a=100;scale=base
' pos=(base*1.1,base*1.1,base*1.1);a=20;scale=base*0.9

Structure Transformation
	c As Container
	pos, rot, scale As Vertex
	base_pos, base_rot, base_scale As Vertex
	prev_pos, prev_rot, prev_scale As Vertex
	next_pos, next_rot, next_scale As Vertex
	
	base_a, prev_a, next_a, a As Double
	'a is Alpha
	
	playhead As Double
	'playhead = 0..100 (prev..next)
	
	what_animated As Array[Boolean]
	'[0] - Alpha
	'[1] - Position
	'[2] - Rotation
	'[3] - Scaling
	
	selected As Boolean
	
	dir As Director
	dir_dur As Double
End Structure
Dim arr_transformations As Array[Transformation]

sub OnInitParameters()
	RegisterParameterString("transform_selected", "Transform selected", "", 80, 999, "")
	RegisterParameterString("transform_hided", "Transform hided", "", 80, 999, "")
	RegisterParameterBool("trought_base", "Transition throught base(0)", false)
	RegisterParameterInt("selected", "Selected", 0, -1, 999)
	RegisterParameterBool("keep_visible", "Keep visible (like in Omo)", false)
	RegisterPushButton("init", "Init", 1)
	RegisterPushButton("base", "Base", 2)
	RegisterPushButton("prev", "Prev", 3)
	RegisterPushButton("next", "Next", 4)
	
	'advanced settings
	RegisterParameterBool("advanced", "Advance functions", false)
	RegisterParameterString("filter", "Child filter (regexp)", "", 80, 999, "")
	RegisterParameterString("takedir", "Take director", "", 80, 999, "")
end sub

Dim selected, prev_selected As Integer
Dim filter As String
Dim takedir As Director
sub OnInit()
	if GetParameterBool("advanced") then
		filter = GetParameterString("filter")
		filter.Trim()
		takedir = Stage.FindDirector(GetParameterString("takedir"))
	end if
	arr_transformations.Clear()
	for i=0 to this.ChildContainerCount-1
		Dim new_transform As Transformation
		if filter == "" OR this.GetChildContainerByIndex(i).name.Match(filter) then
			new_transform.c = this.GetChildContainerByIndex(i)
			new_transform.base_a     = new_transform.c.alpha.value
			new_transform.base_pos   = new_transform.c.position.xyz
			new_transform.base_rot   = new_transform.c.rotation.xyz
			new_transform.base_scale = new_transform.c.scaling.xyz
			new_transform.a     = new_transform.c.alpha.value
			new_transform.pos   = new_transform.c.position.xyz
			new_transform.rot   = new_transform.c.rotation.xyz
			new_transform.scale = new_transform.c.scaling.xyz
			new_transform.what_animated.Clear()
			
			'set what_animated flags:
			new_transform.what_animated.Push(  GetParameterString("transform_selected").Match("a")     OR GetParameterString("transform_hided").Match("a")      ) 'Alpha
			new_transform.what_animated.Push(  GetParameterString("transform_selected").Match("pos")   OR GetParameterString("transform_hided").Match("pos")    ) 'Position
			new_transform.what_animated.Push(  GetParameterString("transform_selected").Match("rot")   OR GetParameterString("transform_hided").Match("rot")    ) 'Rotation
			new_transform.what_animated.Push(  GetParameterString("transform_selected").Match("scale") OR GetParameterString("transform_hided").Match("scale")  ) 'Scaling
			
			StopAnimationOne(new_transform)
			FindDirector(new_transform)
			new_transform.dir.Show(0)
			arr_transformations.Push(new_transform)
		end if
	next
	prev_selected = 0
	selected = 0
end sub

sub OnParameterChanged(parameterName As String)
	SendGuiParameterShow("filter",  GetParameterInt("advanced"))
	SendGuiParameterShow("takedir", GetParameterInt("advanced"))
	
	if parameterName == "selected" then
		prev_selected = selected
		selected = GetParameterInt("selected")
		ResetPrev()
		if selected == -1 then
			HideAll()
		elseif selected == 0 then
			Deselect()
		else
			DoSelect(selected-1)
		end if
	else
		OnInit()
	end if
end sub


Dim new_selected As Integer
sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		'init
		OnInit()
	elseif buttonId == 2 then
		'Base
		this.ScriptPluginInstance.SetParameterInt("selected", 0)
	elseif buttonId == 3 then
		'Prev
		new_selected = GetParameterInt("selected") - 1
		if new_selected <= 0 then new_selected = arr_transformations.size
		this.ScriptPluginInstance.SetParameterInt("selected", new_selected)
	elseif buttonId == 4 then
		'Next
		new_selected = GetParameterInt("selected") + 1
		if new_selected > arr_transformations.size then new_selected = 1
		this.ScriptPluginInstance.SetParameterInt("selected", new_selected)
	end if
end sub

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

Sub FindDirector(_transform As Transformation)
	Dim _arr As Array[Container]
	_transform.c.GetContainerAndSubContainers(_arr, false)
	if _arr.size <=1 then exit sub
	_arr.Erase(0)
	Dim _d As Director
	for i=0 to _arr.ubound
		if _arr[i].IsAnimated() then
			_d = _arr[i].GetDirector()
			exit for
		end if
	next
	_d.Reverse = false
	_transform.dir = _d
	
	'calc duration
	_d.StartAnimationReverse()
	_d.StopAnimation()
	_transform.dir_dur = _d.Time
End Sub

Sub HideAll()
	for i=0 to arr_transformations.ubound
		'as hided
		SetNextTransformByText(arr_transformations[i], GetParameterString("transform_hided"))
		arr_transformations[i].selected = false
		arr_transformations[i].playhead = 0 'start animation
	next
	takedir.ContinueAnimation()
End Sub

Sub Deselect()
	for i=0 to arr_transformations.ubound
		'set next transforms as based
		if arr_transformations[i].what_animated[0] then arr_transformations[i].next_a = arr_transformations[i].base_a
		if arr_transformations[i].what_animated[1] then arr_transformations[i].next_pos = arr_transformations[i].base_pos
		if arr_transformations[i].what_animated[2] then arr_transformations[i].next_rot = arr_transformations[i].base_rot
		if arr_transformations[i].what_animated[3] then arr_transformations[i].next_scale = arr_transformations[i].base_scale
		arr_transformations[i].playhead = 0 'start animation
		arr_transformations[i].selected = true
	next
	takedir.ContinueAnimationReverse()
End Sub

Dim is_proper As Boolean
Sub DoSelect(index As Integer)
	for i=0 to arr_transformations.ubound
		'set next transforms
		is_proper = false
		if GetParameterBool("keep_visible") then
			is_proper = i <= index
		else
			is_proper = i == index
		end if
		
		if is_proper then
			'as selected
			SetNextTransformByText(arr_transformations[i], GetParameterString("transform_selected"))
			arr_transformations[i].selected = true
		else
			'as hided
			SetNextTransformByText(arr_transformations[i], GetParameterString("transform_hided"))
			arr_transformations[i].selected = false
		end if
		arr_transformations[i].playhead = 0 'start animation
	next
	takedir.ContinueAnimation()
End Sub

Sub SetNextTransformByText(transform As Transformation, s As String)
	'pos=(0,0,0);a=100;scale=0
	'pos=(base,base,base);a=base;scale=base
	Dim arr_params, arr_s As Array[String]
	s.Trim()
	s.Split(";", arr_params)
	for i=0 to arr_params.ubound
		arr_params[i].Split("=", arr_s)
		if arr_s.size == 2 then
			'name
			arr_s[0].Trim()
			arr_s[0].MakeLower()
			'value
			arr_s[1].Trim()
			arr_s[1].MakeLower()
			
			Select Case arr_s[0]
			Case "a", "alpha"
				if arr_s[1] == "base" then
					transform.next_a = transform.base_a
				else
					arr_s[1].Substitute(",", ".", true)
					transform.next_a = CDbl(arr_s[1])
				end if
			Case "pos", "position"
				ParseVertexValue(arr_s[1], transform.base_pos, transform.next_pos)
			Case "rot", "rotation"
				ParseVertexValue(arr_s[1], transform.base_rot, transform.next_rot)
			Case "scale", "scaling"
				ParseVertexValue(arr_s[1], transform.base_scale, transform.next_scale)
			Case Else
				println("Din't find param " & arr_s[0])
			End Select
		end if
	next
End Sub

Sub ParseVertexValue(s_value As String, v_base As Vertex, v_next As Vertex)
	s_value.Substitute("\\s", "", true)
	if s_value == "base" then
		v_next = v_base
	elseif s_value.Match("\\([\\d\\.\\w\\-\\+\\*\\/]+\\,[\\d\\.\\w\\-\\+\\*\\/]+\\,[\\d\\.\\w\\-\\+\\*\\/]+\\)") OR s_value.Match("\\([\\d\\.\\w\\-\\+\\*\\/]+\\)") then
		'\([\d\w\-\+]+\,[\d\w\-\+]+\,[\d\w\-\+]+\)
		'\([\d\w\-\+]+\)
		'(num, num, num) or (num)
		Dim open_par, close_par As Integer
		Dim arr_values As Array[String]
		open_par = s_value.Find("(")
		close_par = s_value.FindLastOf(")")
		s_value = s_value.GetSubstring( open_par+1 , close_par-open_par-1 )
		s_value.Trim()
		s_value.Split(",", arr_values)
		for i=0 to arr_values.ubound
			arr_values[i].Trim()
		next
		if arr_values.size == 3 then
			v_next.x = ParseOneValue(arr_values[0], v_base.x)
			v_next.y = ParseOneValue(arr_values[1], v_base.y)
			v_next.z = ParseOneValue(arr_values[2], v_base.z)
		else
			'xyz
			if s_value == "base" then
				v_next = v_base
			else
				v_next = CDbl(s_value)
			end if
		end if
	else
		'one number?
'		s_value.Substitute(",", ".", true)
'		v_next = CDbl(s_value)
		v_next.x = ParseOneValue(s_value, v_base.x)
		v_next.y = ParseOneValue(s_value, v_base.y)
		v_next.z = ParseOneValue(s_value, v_base.z)
	end if
End Sub

Function ParseOneValue(s As String, base As Double) As Double
	if s.Match("^-?\\d+$") then
		ParseOneValue = CDbl(s)
	elseif s == "base" then
		ParseOneValue = base
	elseif s.Match("base[\\+\\-\\*\\/]+[\\d]+") then
		if s.GetChar(4) == "+" then
			ParseOneValue = base + CDbl(s.GetSubstring(5,s.length-5))
		elseif s.GetChar(4) == "-" then
			ParseOneValue = base - CDbl(s.GetSubstring(5,s.length-5))
		elseif s.GetChar(4) == "*" then
			ParseOneValue = base * CDbl(s.GetSubstring(5,s.length-5))
		elseif s.GetChar(4) == "/" then
			ParseOneValue = base / CDbl(s.GetSubstring(5,s.length-5))
		end if
	else '"base"
		ParseOneValue = base
	end if
End Function

Sub StopAnimationOne(transform As Transformation)
	transform.prev_a = transform.a
	transform.prev_pos = transform.pos
	transform.prev_rot = transform.rot
	transform.prev_scale = transform.scale
	transform.playhead = 100 'stop animation
End Sub

Sub ResetPrev()
	for i=0 to arr_transformations.ubound
		if arr_transformations[i].what_animated[0] then arr_transformations[i].prev_a = arr_transformations[i].a
		if arr_transformations[i].what_animated[1] then arr_transformations[i].prev_pos = arr_transformations[i].pos
		if arr_transformations[i].what_animated[2] then arr_transformations[i].prev_rot = arr_transformations[i].rot
		if arr_transformations[i].what_animated[3] then arr_transformations[i].prev_scale = arr_transformations[i].scale
	next
End Sub

Sub CalcTransform(transform As Transformation)
	if transform.what_animated[0] then
		transform.a = CalcCurrentValue(transform.playhead, transform.prev_a, transform.base_a, transform.next_a)
	end if
	if transform.what_animated[1] then
		transform.pos.x = CalcCurrentValue(transform.playhead, transform.prev_pos.x, transform.base_pos.x, transform.next_pos.x)
		transform.pos.y = CalcCurrentValue(transform.playhead, transform.prev_pos.y, transform.base_pos.y, transform.next_pos.y)
		transform.pos.z = CalcCurrentValue(transform.playhead, transform.prev_pos.z, transform.base_pos.z, transform.next_pos.z)
	end if
	if transform.what_animated[2] then
		transform.rot.x = CalcCurrentValue(transform.playhead, transform.prev_rot.x, transform.base_rot.x, transform.next_rot.x)
		transform.rot.y = CalcCurrentValue(transform.playhead, transform.prev_rot.y, transform.base_rot.y, transform.next_rot.y)
		transform.rot.z = CalcCurrentValue(transform.playhead, transform.prev_rot.z, transform.base_rot.z, transform.next_rot.z)
	end if
	if transform.what_animated[3] then
		transform.scale.x = CalcCurrentValue(transform.playhead, transform.prev_scale.x, transform.base_scale.x, transform.next_scale.x)
		transform.scale.y = CalcCurrentValue(transform.playhead, transform.prev_scale.y, transform.base_scale.y, transform.next_scale.y)
		transform.scale.z = CalcCurrentValue(transform.playhead, transform.prev_scale.z, transform.base_scale.z, transform.next_scale.z)
	end if
End Sub
Dim easy_in As Double = 30
Dim easy_out As Double = 90
Dim middle_transition As Double = 30
Function CalcCurrentValue(playhead As Double, prev_value As Double, base_value As Double, next_value As Double) As Double
	if GetParameterBool("trought_base") AND prev_value <> base_value AND next_value <> base_value then
		if playhead < middle_transition then
			CalcCurrentValue = Besizer(100*playhead/middle_transition, prev_value, base_value, easy_in, 30)
			takedir.ContinueAnimationReverse()
		else
			CalcCurrentValue = Besizer(100*(playhead-middle_transition)/(100-middle_transition), base_value, next_value, 30, easy_out)
			takedir.ContinueAnimation()
		end if
	else
		CalcCurrentValue = Besizer(playhead, prev_value, next_value, easy_in, easy_out)
	end if
End Function

Sub PlayDir(_transform as Transformation)
	if _transform.dir == null then exit sub
	
	if GetParameterBool("trought_base") AND prev_selected <> 0 AND selected <> 0 then
		if _transform.playhead < middle_transition then
			if NOT _transform.selected then
				_transform.dir.ContinueAnimationReverse()
			else
				_transform.dir.ContinueAnimation()
			end if
		else
			if _transform.selected then
				_transform.dir.ContinueAnimationReverse()
			else
				_transform.dir.ContinueAnimation()
			end if
		end if
	else
		if _transform.selected then
			_transform.dir.ContinueAnimationReverse()
		else
			_transform.dir.ContinueAnimation()
		end if
	end if
End Sub

Sub SetTransform(transform as Transformation)
	if transform.what_animated[0] then
		transform.c.alpha.value = transform.a
	end if
	if transform.what_animated[1] then
		transform.c.position.xyz = transform.pos
	end if
	if transform.what_animated[2] then
		transform.c.rotation.xyz = transform.rot
	end if
	if transform.what_animated[3] then
		transform.c.scaling.xyz = transform.scale
	end if
End Sub

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

sub OnExecPerField()
	for i=0 to arr_transformations.ubound
		if arr_transformations[i].playhead < 100 then
			if prev_selected == 0 OR selected == 0 then
				arr_transformations[i].playhead += 2
			else
				arr_transformations[i].playhead += 1
			end if
			if arr_transformations[i].playhead >= 100 then StopAnimationOne(arr_transformations[i])
			
			PlayDir(arr_transformations[i])
			CalcTransform(arr_transformations[i])
			SetTransform(arr_transformations[i])
		end if
	next
end sub

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

Function ClampDbl(value as double, min  as double, max as double) as Double
	if value < min then value = min
	if value > max then value = max
	ClampDbl = value
End Function

Function Besizer(ByVal procent as double, ByVal begin_value as double, ByVal end_value as double, ByVal begin_weight as double, ByVal end_weight as double) as Double
	Dim a, b, c, d, t_besier_value As Double
	procent      = ClampDbl(procent,       0, 100)/100.0
	begin_weight = ClampDbl(begin_weight, 35, 100)/100.0
	end_weight   = ClampDbl(end_weight,   35, 100)/100.0
	
	a = 3*begin_weight - 3*(1.0 - end_weight) + 1
	b = - 6*begin_weight + 3*(1.0 - end_weight)
	c = 3*begin_weight
	d = -procent
	
	t_besier_value = (sqrt((-27*a^2*d + 9*a*b*c - 2*b^3)^2 + 4*(3*a*c - b^2)^3) - 27*a^2*d + 9*a*b*c - 2*b^3)^(1.0/3)/(3*2^(1.0/3)*a) - (2^(1.0/3)*(3*a*c - b^2))/(3*a*(sqrt((-27*a^2*d + 9*a*b*c - 2*b^3)^2 + 4*(3*a*c - b^2)^3) - 27*a^2*d + 9*a*b*c - 2*b^3)^(1.0/3)) - b/(3*a)
	Besizer = begin_value + (end_value - begin_value)*( 3*(1-t_besier_value)*t_besier_value^2 + t_besier_value^3 ) 
End Function
