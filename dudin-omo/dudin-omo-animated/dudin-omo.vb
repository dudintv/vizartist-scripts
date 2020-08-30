RegisterPluginVersion(2,4,1)

Structure Properties
	a As Double 'it is Alpha
	pos As Vertex
	rot As Vertex
	scale As Vertex
End Structure

Structure Transformation
	c As Container
	cur_props, base_props, prev_props, next_props As Properties
	playhead As Double
	'playhead = 0..100 (prev..next)
	target_state As Integer
	'target_state = -1 (hide), 0 (base), 1 (selected)
	
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
	RegisterParameterContainer("root", "Root container (or this)")
	RegisterParameterString("transform_base", "Transform base", "", 80, 999, "")
	RegisterParameterString("transform_selected", "Transform selected", "", 80, 999, "")
	RegisterParameterString("transform_hided", "Transform hided", "", 80, 999, "")
	RegisterParameterBool("through_base", "Transition throught base(0)", false)
	RegisterParameterSliderInt("middle_transition", "Middle transition, %", 50, 0, 100, 300)
	RegisterParameterBool("keep_visible", "Keep visible (like in Omo)", false)
	RegisterParameterInt("selected", "Selected", 0, -1, 999)
	RegisterPushButton("init", "Init", 10)
	RegisterPushButton("to_base", "All to Base!", 11)
	RegisterPushButton("to_hide", "All to Hide!", 12)
	RegisterPushButton("to_show", "All to Selected!", 13)
	RegisterPushButton("base", "Base", 20)
	RegisterPushButton("prev", "Prev", 30)
	RegisterPushButton("next", "Next", 40)
	RegisterParameterDouble("transition_duration_show", "Show duration (sec)", 1, 0, 999)
	RegisterParameterDouble("transition_duration_hide", "Hide duration (sec)", 1, 0, 999)
	
	'advanced settings
	RegisterParameterBool("advanced", "Advance functions", false)
	RegisterParameterBool("anim_items", "Animate item dirs", false)
	RegisterParameterString("filter", "Child filter (regexp)", "", 80, 999, "")
	RegisterParameterString("common_dir", "Common director", "", 80, 999, "")
	
	RegisterParameterBool("manual_show_anim", "Manual show-anim", false)
	RegisterParameterDouble("show_anim_value", "Show-anim value (uniq dir)", 0, 0, 100.0)
end sub

Dim c_root As Container
Dim selected, prev_selected, new_selected As Integer
Dim filter As String
Dim common_dir, show_anim_dir As Director
Dim transition_duration_show, transition_duration_hide As Integer
Dim middle_transition As Double

sub OnParameterChanged(parameterName As String)
	SendGuiParameterShow("anim_items", GetParameterInt("advanced"))
	SendGuiParameterShow("filter",  GetParameterInt("advanced"))
	SendGuiParameterShow("common_dir", GetParameterInt("advanced"))
	SendGuiParameterShow("middle_transition", CInt(GetParameterBool("through_base")))
	SendGuiParameterShow("show_anim_value", CInt(GetParameterBool("manual_show_anim")))
	
	select case parameterName
	case "selected"
		prev_selected = selected
		selected = GetParameterInt("selected")
		
		for i=0 to arr_transformations.ubound
			arr_transformations[i].prev_props = arr_transformations[i].cur_props
		next

		if selected == -1 then
			DeselectAll()
		elseif selected == 0 then
			BaseAll()
		else 'selected > 1
			SelectOne(selected-1)
		end if
	case "middle_transition", "transition_duration_show", "transition_duration_hide"
		middle_transition = GetParameterInt("middle_transition")/100.0 * GetParameterDouble("transition_duration_show")/System.CurrentRefreshRate
		transition_duration_show = CInt(GetParameterDouble("transition_duration_show") / System.CurrentRefreshRate)
		transition_duration_hide = CInt(GetParameterDouble("transition_duration_hide") / System.CurrentRefreshRate)
		for i=0 to arr_transformations.ubound
			arr_transformations[i].playhead = -1
		next
	case "transform_base", "transform_selected", "transform_hided"
		ToBaseAllNow()
		OnInit()
	end select
end sub

sub OnExecAction(buttonId As Integer)
	select case buttonId
	case 10
		OnInit()
	case 11
		ToBaseAllNow()
	case 12
		ToHideAllNow()
	case 13
		ToShowAllNow()
	case 20
		'Base
		this.ScriptPluginInstance.SetParameterInt("selected", 0)
	case 30
		'Prev
		new_selected = GetParameterInt("selected") - 1
		if new_selected <= 0 then new_selected = arr_transformations.size
		this.ScriptPluginInstance.SetParameterInt("selected", new_selected)
	case 40
		'Next
		new_selected = GetParameterInt("selected") + 1
		if new_selected > arr_transformations.size then new_selected = 1
		this.ScriptPluginInstance.SetParameterInt("selected", new_selected)
	end select
end sub

Dim max_duration As Double
sub OnExecPerField()
	for i=0 to arr_transformations.ubound
		if arr_transformations[i].target_state == 1 then
			max_duration = transition_duration_show
		else
			max_duration = transition_duration_hide
		end if

		if arr_transformations[i].playhead >= 0 AND arr_transformations[i].playhead < max_duration then
			if GetParameterBool("through_base") AND (prev_selected == 0 OR selected == 0) then
				' two time faster
				arr_transformations[i].playhead += 2
			else
				arr_transformations[i].playhead += 1
			end if
			CalcCurTransform(arr_transformations[i])
			ApplyTransform(arr_transformations[i])
			PlayItemAnimation(arr_transformations[i])
		else
			arr_transformations[i].playhead = -1 'stop animation
		end if
	next
end sub

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

' INIT

sub OnInit()
	c_root = GetParameterContainer("root")
	if c_root == null then c_root = this
	
	transition_duration_show = CInt(GetParameterDouble("transition_duration_show") / System.CurrentRefreshRate)
	if GetParameterBool("advanced") then
		'fill these variables only if "advanced" is ON
		filter = GetParameterString("filter")
		filter.Trim()
		common_dir = Stage.FindDirector(GetParameterString("common_dir"))
		common_dir.Show(0)
	end if
	
	if GetParameterBool("manual_show_anim") then
		show_anim_dir = this.GetDirector()
	end if
	
	Dim _transform_selected, _transform_hided As String
	arr_transformations.Clear()
	for i=0 to c_root.ChildContainerCount-1
		Dim new_transform As Transformation
		if filter == "" OR c_root.GetChildContainerByIndex(i).name.Match(filter) then
			new_transform.c = c_root.GetChildContainerByIndex(i)
			new_transform.base_props.a     = new_transform.c.alpha.value
			new_transform.base_props.pos   = new_transform.c.position.xyz
			new_transform.base_props.rot   = new_transform.c.rotation.xyz
			new_transform.base_props.scale = new_transform.c.scaling.xyz
			new_transform.base_props = ParseProps(new_transform, GetParameterString("transform_base"))

			new_transform.cur_props  = new_transform.base_props
			new_transform.prev_props = new_transform.base_props
			
			'set what_animated flags:
			new_transform.what_animated.Clear()
			_transform_selected = GetParameterString("transform_selected")
			_transform_selected.Substitute("\\s", "", true) 'remove all spaces
			_transform_hided = GetParameterString("transform_hided")
			_transform_hided.Substitute("\\s", "", true) 'remove all spaces
			new_transform.what_animated.Push(  _transform_selected.Match("a=")     OR _transform_hided.Match("a=") OR _transform_selected.Match("alpha=")     OR _transform_hided.Match("alpha=")      ) 'Alpha
			new_transform.what_animated.Push(  _transform_selected.Match("pos.*=")   OR _transform_hided.Match("pos.*=")    ) 'Position
			new_transform.what_animated.Push(  _transform_selected.Match("rot.*=")   OR _transform_hided.Match("rot.*=")    ) 'Rotation
			new_transform.what_animated.Push(  _transform_selected.Match("scale=") OR _transform_hided.Match("scale=") OR  _transform_selected.Match("scalng=") OR _transform_hided.Match("scaling=")  ) 'Scaling
			
			new_transform.playhead = transition_duration_show 'to stop scripted animation in ExecPerField
			InitDirector(new_transform)
			new_transform.dir.Show(new_transform.dir_dur)
			new_transform.target_state = 0

			arr_transformations.Push(new_transform)
		end if
	next
	prev_selected = 0
	selected = 0
	c_root.ScriptPluginInstance.SetParameterInt("selected", 0)
end sub

Sub InitDirector(_transform As Transformation)
	'try to find a Director among child containers of _transformation.c
	'the first result found will be stored
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

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

' MAIN ACTIONS with animations

Sub BaseAll()
	for i=0 to arr_transformations.ubound
		'set next transforms as based
		arr_transformations[i].next_props = arr_transformations[i].base_props
		arr_transformations[i].selected = true
		arr_transformations[i].target_state = 0
		arr_transformations[i].playhead = 0 'start animation
	next
	common_dir.ContinueAnimationReverse()
End Sub

Sub SelectOne(_index As Integer)
	Dim _should_be_selected As Boolean
	for i=0 to arr_transformations.ubound
		_should_be_selected = false
		if GetParameterBool("keep_visible") then
			'select all items before and equal index
			_should_be_selected = i <= _index
		else
			'select only index item
			_should_be_selected = i == _index
		end if
		
		arr_transformations[i].selected = _should_be_selected

		if _should_be_selected then
			arr_transformations[i].next_props = ParseProps(arr_transformations[i], GetParameterString("transform_selected"))
			arr_transformations[i].target_state = 1
		else
			arr_transformations[i].next_props = ParseProps(arr_transformations[i], GetParameterString("transform_hided"))
			arr_transformations[i].target_state = -1
		end if
		arr_transformations[i].playhead = 0 'start animation
	next
	common_dir.ContinueAnimation()
	if GetParameterBool("manual_show_anim") then
		show_anim_dir.StartAnimation()
	end if
End Sub

Sub DeselectAll()
	for i=0 to arr_transformations.ubound
		arr_transformations[i].next_props = ParseProps(arr_transformations[i], GetParameterString("transform_hided"))
		arr_transformations[i].selected = false
		arr_transformations[i].target_state = -1
		arr_transformations[i].playhead = 0 'start animation
	next
	common_dir.ContinueAnimation()
End Sub

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

' MOMENTUM ACTIONS without any animations

Sub ToBaseAllNow()
	for i=0 to arr_transformations.ubound
		arr_transformations[i].cur_props = arr_transformations[i].base_props
		arr_transformations[i].target_state = 0
		ApplyTransform(arr_transformations[i])
		if GetParameterBool("anim_items") then
			arr_transformations[i].dir.Show(arr_transformations[i].dir_dur)
			arr_transformations[i].playhead = -1 'stop animation
		end if
	next
	common_dir.Show(0)
End Sub

Sub ToHideAllNow()
	for i=0 to arr_transformations.ubound
		arr_transformations[i].cur_props = ParseProps(arr_transformations[i], GetParameterString("transform_hided"))
		arr_transformations[i].target_state = -1
		ApplyTransform(arr_transformations[i])
		if GetParameterBool("anim_items") then
			arr_transformations[i].dir.Show(0)
			arr_transformations[i].playhead = -1 'stop animation
		end if
	next
	common_dir.Show(0)
End Sub

Sub ToShowAllNow()
	for i=0 to arr_transformations.ubound
		arr_transformations[i].cur_props = ParseProps(arr_transformations[i], GetParameterString("transform_selected"))
		arr_transformations[i].target_state = 1
		ApplyTransform(arr_transformations[i])
		if GetParameterBool("anim_items") then
			arr_transformations[i].dir.Show(arr_transformations[i].dir_dur)
			arr_transformations[i].playhead = -1 'stop animation
		end if
	next
	common_dir.StartAnimationReverse()
	common_dir.StopAnimation()
End Sub

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

' PARSING

Function ParseProps(_transform As Transformation, _s As String) As Properties
	'pos=(0,0,0);a=100;scale=0
	'pos=(base,base,base);a=base;scale=base

	Dim _props As Properties = _transform.base_props
	Dim _arr_params, _arr_s As Array[String]
	_s.Trim()
	_s.Split(";", _arr_params)
	for i=0 to _arr_params.ubound
		_arr_params[i].MakeLower()
		_arr_params[i].Split("=", _arr_s)
		if _arr_s.size == 2 then
			'name
			_arr_s[0].Trim()
			'value
			_arr_s[1].Trim()
			
			Select Case _arr_s[0]
			Case "a", "alpha"
				_props.a = ParseOneValue(_transform.base_props.a, _arr_s[1])
			Case "pos", "position"
				_props.pos = ParseVertexValue(_transform.base_props.pos, _arr_s[1])
			Case "posx", "positionx"
				_props.pos = CVertex(ParseOneValue(_transform.base_props.pos.x, _arr_s[1]), _transform.base_props.pos.y, _transform.base_props.pos.z)
			Case "posy", "positiony"
				_props.pos = CVertex(_transform.base_props.pos.x, ParseOneValue(_transform.base_props.pos.y, _arr_s[1]), _transform.base_props.pos.z)
			Case "posz", "positionz"
				_props.pos = CVertex(_transform.base_props.pos.x, _transform.base_props.pos.y, ParseOneValue(_transform.base_props.pos.z, _arr_s[1]))
			Case "rot", "rotation"
				_props.rot = ParseVertexValue(_transform.base_props.rot, _arr_s[1])
			Case "rotx", "rotationx"
				_props.rot = CVertex(ParseOneValue(_transform.base_props.rot.x, _arr_s[1]), _transform.base_props.rot.y, _transform.base_props.rot.z)
			Case "roty", "rotationy"
				_props.rot = CVertex(_transform.base_props.rot.x, ParseOneValue(_transform.base_props.rot.y, _arr_s[1]), _transform.base_props.rot.z)
			Case "rotz", "rotationz"
				_props.rot = CVertex(_transform.base_props.rot.x, _transform.base_props.rot.y, ParseOneValue(_transform.base_props.rot.z, _arr_s[1]))
			Case "scale", "scaling"
				_props.scale = ParseVertexValue(_transform.base_props.scale, _arr_s[1])
			Case "scalex", "scalingx"
				_props.scale = CVertex(ParseOneValue(_transform.base_props.scale.x, _arr_s[1]), _transform.base_props.scale.y, _transform.base_props.scale.z)
			Case "scaley", "scalingy"
				_props.scale = CVertex(_transform.base_props.scale.x, ParseOneValue(_transform.base_props.scale.y, _arr_s[1]), _transform.base_props.scale.z)
			Case "scalez", "scalingz"
				_props.scale = CVertex(_transform.base_props.scale.x, _transform.base_props.scale.y, ParseOneValue(_transform.base_props.scale.z, _arr_s[1]))
			Case Else
				println("Din't find param " & _arr_s[0])
			End Select
		end if
	next
	ParseProps = _props
End Function

Function ParseVertexValue(_base As Vertex, _s As String) As Vertex
	Dim _v As Vertex
	_s.Substitute("\\s", "", true) 'remove all spaces
	if _s == "base" then
		_v = _base
	elseif _s.Match("\\([\\d\\.\\w\\-\\+\\*\\/]+\\,[\\d\\.\\w\\-\\+\\*\\/]+\\,[\\d\\.\\w\\-\\+\\*\\/]+\\)") OR _s.Match("\\([\\d\\.\\w\\-\\+\\*\\/]+\\)") then
		'\([\d\w\-\+]+\,[\d\w\-\+]+\,[\d\w\-\+]+\)
		'\([\d\w\-\+]+\)
		'(num, num, num) or (num)
		Dim _open_par, _close_par As Integer
		Dim _arr_values As Array[String]
		_open_par = _s.Find("(")
		_close_par = _s.FindLastOf(")")
		_s = _s.GetSubstring(_open_par + 1, _close_par - _open_par - 1)
		_s.Trim()
		_s.Split(",", _arr_values)
		for i=0 to _arr_values.ubound
			_arr_values[i].Trim()
		next
		if _arr_values.size == 3 then
			_v.x = ParseOneValue(_base.x, _arr_values[0])
			_v.y = ParseOneValue(_base.y, _arr_values[1])
			_v.z = ParseOneValue(_base.z, _arr_values[2])
		else
			'xyz
			if _s == "base" then
				_v = _base
			else
				_v = CDbl(_s)
			end if
		end if
	else
		'one number
		_v.x = ParseOneValue(_base.x, _s)
		_v.y = ParseOneValue(_base.y, _s)
		_v.z = ParseOneValue(_base.z, _s)
	end if
	ParseVertexValue = _v
End Function

Function ParseOneValue(_base As Double, _s As String) As Double
	if _s.Match("^-?[\\d,\\.]+$") then
		ParseOneValue = CDbl(_s)
	elseif _s == "base" then
		ParseOneValue = _base
	elseif _s.Match("base[\\+\\-\\*\\/]+[\\d,\\.]+") then
		if _s.GetChar(4) == "+" then
			ParseOneValue = _base + CDbl(_s.GetSubstring(5,_s.length-5))
		elseif _s.GetChar(4) == "-" then
			ParseOneValue = _base - CDbl(_s.GetSubstring(5,_s.length-5))
		elseif _s.GetChar(4) == "*" then
			ParseOneValue = _base * CDbl(_s.GetSubstring(5,_s.length-5))
		elseif _s.GetChar(4) == "/" then
			ParseOneValue = _base / CDbl(_s.GetSubstring(5,_s.length-5))
		end if
	else '"base"
		ParseOneValue = _base
	end if
End Function

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

' ANIMATION

Sub PlayItemAnimation(_transform as Transformation)
	if _transform.dir == null OR NOT GetParameterBool("advanced") OR NOT GetParameterBool("anim_items") then exit sub
	
	if GetParameterBool("through_base") AND prev_selected <> 0 AND selected <> 0 then
		if _transform.playhead < middle_transition then
			if NOT _transform.selected then
				_transform.dir.ContinueAnimation()
			else
				_transform.dir.ContinueAnimationReverse()
			end if
		else
			if _transform.selected then
				_transform.dir.ContinueAnimation()
			else
				_transform.dir.ContinueAnimationReverse()
			end if
		end if
	else
		if _transform.selected then
			_transform.dir.ContinueAnimation()
		else
			_transform.dir.ContinueAnimationReverse()
		end if
	end if
End Sub

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

' TRANSFORM

Sub CalcCurTransform(_transform As transformation)
	if _transform.what_animated[0] then
		_transform.cur_props.a = CalcCurrentValue(_transform.target_state, _transform.playhead, _transform.prev_props.a, _transform.base_props.a, _transform.next_props.a)
	end if
	if _transform.what_animated[1] then
		_transform.cur_props.pos.x = CalcCurrentValue(_transform.target_state, _transform.playhead, _transform.prev_props.pos.x, _transform.base_props.pos.x, _transform.next_props.pos.x)
		_transform.cur_props.pos.y = CalcCurrentValue(_transform.target_state, _transform.playhead, _transform.prev_props.pos.y, _transform.base_props.pos.y, _transform.next_props.pos.y)
		_transform.cur_props.pos.z = CalcCurrentValue(_transform.target_state, _transform.playhead, _transform.prev_props.pos.z, _transform.base_props.pos.z, _transform.next_props.pos.z)
	end if
	if _transform.what_animated[2] then
		_transform.cur_props.rot.x = CalcCurrentValue(_transform.target_state, _transform.playhead, _transform.prev_props.rot.x, _transform.base_props.rot.x, _transform.next_props.rot.x)
		_transform.cur_props.rot.y = CalcCurrentValue(_transform.target_state, _transform.playhead, _transform.prev_props.rot.y, _transform.base_props.rot.y, _transform.next_props.rot.y)
		_transform.cur_props.rot.z = CalcCurrentValue(_transform.target_state, _transform.playhead, _transform.prev_props.rot.z, _transform.base_props.rot.z, _transform.next_props.rot.z)
	end if
	if _transform.what_animated[3] then
		_transform.cur_props.scale.x = CalcCurrentValue(_transform.target_state, _transform.playhead, _transform.prev_props.scale.x, _transform.base_props.scale.x, _transform.next_props.scale.x)
		_transform.cur_props.scale.y = CalcCurrentValue(_transform.target_state, _transform.playhead, _transform.prev_props.scale.y, _transform.base_props.scale.y, _transform.next_props.scale.y)
		_transform.cur_props.scale.z = CalcCurrentValue(_transform.target_state, _transform.playhead, _transform.prev_props.scale.z, _transform.base_props.scale.z, _transform.next_props.scale.z)
	end if
End Sub

Sub ApplyTransform(_transform As Transformation)
	if _transform.what_animated[0] then _transform.c.alpha.value  = _transform.cur_props.a
	if _transform.what_animated[1] then _transform.c.position.xyz = _transform.cur_props.pos
	if _transform.what_animated[2] then _transform.c.rotation.xyz = _transform.cur_props.rot
	if _transform.what_animated[3] then _transform.c.scaling.xyz  = _transform.cur_props.scale
End Sub

Dim easy_in As Double = 30
Dim easy_out As Double = 90

Function CalcCurrentValue(_target_state As Integer, _playhead As Double, _prev_value As Double, _base_value As Double, _next_value As Double) As Double
	if GetParameterBool("through_base") AND _prev_value <> _base_value AND _next_value <> _base_value then
		if _playhead < middle_transition then
			' CalcCurrentValue = Besizer(100.0*_playhead/middle_transition, _prev_value, _base_value, easy_in, 30)
			CalcCurrentValue = AnimateOut(100.0*_playhead/middle_transition, _prev_value, _next_value)
			common_dir.ContinueAnimationReverse()
		else
			' CalcCurrentValue = Besizer(100.0*(_playhead-middle_transition)/(transition_duration_show-middle_transition), _base_value, _next_value, 30, easy_out)
			CalcCurrentValue = AnimateIn(100.0*(_playhead-middle_transition)/(transition_duration_show-middle_transition), _prev_value, _next_value)
			common_dir.ContinueAnimation()
		end if
	else
		'CalcCurrentValue = Besizer(100.0*_playhead/transition_duration_show, _prev_value, _next_value, easy_in, easy_out)
		if _target_state == 1 then
			CalcCurrentValue = AnimateIn(100.0*_playhead/transition_duration_show, _prev_value, _next_value)
		else
			CalcCurrentValue = AnimateOut(100.0*_playhead/transition_duration_hide, _prev_value, _next_value)
		end if
	end if
End Function

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

Dim finish_main_anim As Double = 15
Dim value_at_main_finish_anim As Double
Dim result_anim As Double

Function AnimateIn(_playhead As Double, _start As Double, _end As Double) As Double
	if GetParameterBool("manual_show_anim") then
		AnimateIn = _start + (_end - _start)* GetParameterDouble("show_anim_value")/100.0
	else
		AnimateIn = Besizer(_playhead, _start, _end, 30, 80)
	end if
End Function

Function AnimateOut(_playhead As Double, _start As Double, _end As Double) As Double
	AnimateOut = Besizer(_playhead, _start, _end, 40, 80)
End Function

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

Function ClampDbl(ByVal _value as double, ByVal _min as double, ByVal _max as double) as Double
	if _value < _min then _value = _min
	if _value > _max then _value = _max
	ClampDbl = _value
End Function

Function Besizer(ByVal procent as double, ByVal begin_value as double, ByVal end_value as double, ByVal begin_weight as double, ByVal end_weight as double) as Double
	Dim a, b, c, d, t_besier_value As Double
	procent      = ClampDbl(procent,       0, 100)/100.0
	begin_weight = ClampDbl(begin_weight, 35, 100)/100.0
	end_weight   = ClampDbl(end_weight,   35, 100)/100.0
	
	a = 3*begin_weight - 3*(1.0 - end_weight) + 1
	b = - 6*begin_weight + 3*(1.0 - end_weight)
	c = 3*begin_weight
	d = - procent
	
	t_besier_value = (sqrt((-27*a^2*d + 9*a*b*c - 2*b^3)^2 + 4*(3*a*c - b^2)^3) - 27*a^2*d + 9*a*b*c - 2*b^3)^(1.0/3)/(3*2^(1.0/3)*a) - (2^(1.0/3)*(3*a*c - b^2))/(3*a*(sqrt((-27*a^2*d + 9*a*b*c - 2*b^3)^2 + 4*(3*a*c - b^2)^3) - 27*a^2*d + 9*a*b*c - 2*b^3)^(1.0/3)) - b/(3*a)
	Besizer = begin_value + (end_value - begin_value)*( 3*(1-t_besier_value)*t_besier_value^2 + t_besier_value^3 ) 
End Function
