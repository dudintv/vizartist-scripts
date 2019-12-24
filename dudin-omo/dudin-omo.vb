RegisterPluginVersion(1,0,0)

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
End Structure
Dim arr_transformations As Array[Transformation]

sub OnInitParameters()
	RegisterParameterString("transform_selected", "Transform selected", "", 80, 999, "")
	RegisterParameterString("transform_hided", "Transform hided", "", 80, 999, "")
	RegisterParameterInt("selected", "Selected", 0, 0, 999)
	RegisterPushButton("init", "Init", 1)
end sub

sub OnInit()
	arr_transformations.Clear()
	for i=0 to this.ChildContainerCount
		Dim new_transform As Transformation
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
		new_transform.what_animated.Push(true) 'Alpha
		new_transform.what_animated.Push(true) 'Position
		new_transform.what_animated.Push(true) 'Rotation
		new_transform.what_animated.Push(true) 'Scaling
		
		StopAnimationOne(new_transform)
		arr_transformations.Push(new_transform)
	next
end sub

sub OnParameterChanged(parameterName As String)
	if parameterName == "selected" then
		ResetPrev()
		if GetParameterInt("selected") == 0 then
			Deselect()
		else
			SelectOne(GetParameterInt("selected")-1)
		end if
	else
		OnInit()
	end if
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		'init
		OnInit()
	end if
end sub

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

Sub SelectOne(index As Integer)
	for i=0 to arr_transformations.ubound
		'set next transforms
		if i == index then
			'as selected
			SetNextTransformByText(arr_transformations[i], GetParameterString("transform_selected"))
		else
			'as hided
			SetNextTransformByText(arr_transformations[i], GetParameterString("transform_hided"))
		end if
		arr_transformations[i].playhead = 0 'start animation
	next
End Sub

Sub Deselect()
	for i=0 to arr_transformations.ubound
		'set next transforms as based
		if arr_transformations[i].what_animated[0] then arr_transformations[i].next_a = arr_transformations[i].base_a
		if arr_transformations[i].what_animated[1] then arr_transformations[i].next_pos = arr_transformations[i].base_pos
		if arr_transformations[i].what_animated[2] then arr_transformations[i].next_rot = arr_transformations[i].base_rot
		if arr_transformations[i].what_animated[3] then arr_transformations[i].next_scale = arr_transformations[i].base_scale
		arr_transformations[i].playhead = 0 'start animation
	next
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
'				println("transform.base_scale = " & CStr(transform.base_scale))
'				println("transform.next_scale = " & CStr(transform.next_scale))
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
		println("THREE OR ONE NUMBER")
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
			println("arr_values.size == 3 AND s_value = " & s_value)
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
		println("ONE NUMBER")
		v_next.x = ParseOneValue(s_value, v_base.x)
		v_next.y = ParseOneValue(s_value, v_base.y)
		v_next.z = ParseOneValue(s_value, v_base.z)
	end if
End Sub

Function ParseOneValue(s As String, base As Double) As Double
	println("!!!!s = " & s)
	if s.Match("^\\d+$") then
		ParseOneValue = CDbl(s)
	elseif s == "base" then
		ParseOneValue = base
	elseif s.Match("base[\\+\\-\\*\\/]+[\\d]+") then
		println("s.GetChar(5) = " & s.GetChar(5))
		println("s.GetSubstring(5,s.length-5) = " & s.GetSubstring(5,s.length-5))
		
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

Dim easy_in As Double = 30
Dim easy_out As Double = 90
Sub CalcTransform(transform As Transformation)
	if transform.what_animated[0] then
		transform.a = Besizer(transform.playhead, transform.prev_a, transform.next_a, easy_in, easy_out)
	end if
	if transform.what_animated[1] then
		transform.pos.x = Besizer(transform.playhead, transform.prev_pos.x, transform.next_pos.x, easy_in, easy_out)
		transform.pos.y = Besizer(transform.playhead, transform.prev_pos.y, transform.next_pos.y, easy_in, easy_out)
		transform.pos.z = Besizer(transform.playhead, transform.prev_pos.z, transform.next_pos.z, easy_in, easy_out)
	end if
	if transform.what_animated[2] then
		transform.rot.x = Besizer(transform.playhead, transform.prev_rot.x, transform.next_rot.x, easy_in, easy_out)
		transform.rot.y = Besizer(transform.playhead, transform.prev_rot.y, transform.next_rot.y, easy_in, easy_out)
		transform.rot.z = Besizer(transform.playhead, transform.prev_rot.z, transform.next_rot.z, easy_in, easy_out)
	end if
	if transform.what_animated[3] then
		transform.scale.x = Besizer(transform.playhead, transform.prev_scale.x, transform.next_scale.x, easy_in, easy_out)
		transform.scale.y = Besizer(transform.playhead, transform.prev_scale.y, transform.next_scale.y, easy_in, easy_out)
		transform.scale.z = Besizer(transform.playhead, transform.prev_scale.z, transform.next_scale.z, easy_in, easy_out)
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
			arr_transformations[i].playhead += 2
			if arr_transformations[i].playhead >= 100 then StopAnimationOne(arr_transformations[i])
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
