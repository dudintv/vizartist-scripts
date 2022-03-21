RegisterPluginVersion(1,0,0)

Dim c_objects, c_from, c_to As Container
Dim arr_c_objects, arr_c_from, arr_c_to As Array[Container]
Dim v_object, v_from, v_to As Vertex
Dim one_progress, progress, shift, begin, finish, a, b As Double
Dim reverse As Boolean

sub OnInitParameters()
	RegisterParameterContainer("objects", "Objects")
	RegisterParameterContainer("targets_from", "Targets from")
	RegisterParameterContainer("targets_to", "Targets to")
	RegisterParameterDouble("progress", "Progress", 0.0, 0.0, 100.0)
	RegisterParameterDouble("shift", "Shift", 0.0, 0.0, 100.0)
	RegisterParameterBool("reverse", "Reverse", false)
	RegisterPushButton("set_vertices", "Ser vertices", 1)
end sub

Dim i_start As Integer
Dim i_end As Integer 
Dim i_step As Integer
sub OnInit()
	reverse = GetParameterBool("reverse")
	arr_c_objects.clear()
	arr_c_from.clear()
	arr_c_to.clear()
	
	i_start = 0
	i_end = c_objects.ChildContainerCount - 1
	c_objects = GetParameterContainer("objects")
	i_step = 1
	if reverse then
		i_start = i_end
		i_end = 0
		i_step = -1
	end if
	for i=i_start to i_end Step i_step
		arr_c_objects.push(c_objects.GetChildContainerByIndex(i))
	next
	
	c_from = GetParameterContainer("targets_from")
	i_start = 0
	i_end = c_from.ChildContainerCount - 1
	i_step = 1
	if reverse then
		i_start = i_end
		i_end = 0
		i_step = -1
	end if
	for i=i_start to i_end Step i_step
		arr_c_from.push(c_from.GetChildContainerByIndex(i))
	next
	
	c_to = GetParameterContainer("targets_to")
	i_start = 0
	i_end = c_to.ChildContainerCount - 1
	i_step = 1
	if reverse then
		i_start = i_end
		i_end = 0
		i_step = -1
	end if
	for i=i_start to i_end Step i_step
		arr_c_to.push(c_to.GetChildContainerByIndex(i))
	next
	SetVertexes()
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecAction(buttonId As Integer)
	if buttonId == 1 then
		SetVertexes()
	end if
end sub


Dim arr_v_from, arr_v_to As Array[Vertex]
Sub SetVertexes()
	arr_v_from.Clear()
	arr_v_to.Clear()
	for i=0 to arr_c_objects.ubound
		arr_v_from.Push( arr_c_objects[i].WorldPosToLocalPos( arr_c_from[i].LocalPosToWorldPos(arr_c_from[i].position.xyz) ) )
		arr_v_to.Push( arr_c_objects[i].WorldPosToLocalPos( arr_c_to[i].LocalPosToWorldPos(arr_c_to[i].position.xyz) ) )
	next
End Sub

sub OnExecPerField()
	progress = GetParameterDouble("progress")
	shift = GetParameterDouble("shift")
	for i=0 to arr_c_objects.ubound
		begin = i*shift/arr_c_objects.size
		finish = 100 - (arr_c_objects.ubound - i)*(shift/arr_c_objects.size)
		a = 100.0/(finish - begin)
		b = -begin*a
		one_progress = a*progress + b
		'v_from = arr_c_objects[i].WorldPosToLocalPos( arr_c_from[i].LocalPosToWorldPos(arr_c_from[i].position.xyz) )
		'v_to = arr_c_objects[i].WorldPosToLocalPos( arr_c_to[i].LocalPosToWorldPos(arr_c_to[i].position.xyz) )
		v_from = arr_v_from[i]
		v_to = arr_v_to[i]
		
		if one_progress <= 0 then
			arr_c_objects[i].position.xyz = v_from
		elseif one_progress >= 100 then
			arr_c_objects[i].position.xyz = v_to
		else
			v_object = VertexBesier(one_progress, v_from, v_to)
			arr_c_objects[i].position.xyz = v_object
		end if
	next
end sub

'''''''''

Function ClampDbl(_value as double, _min  as double, _max as double) as Double
	if _value < _min then _value = _min
	if _value > _max then _value = _max
	ClampDbl = _value
End Function

Dim _begin_weight = 0.3
Dim _end_weight   = 0.8
Dim _a = 3*_begin_weight - 3*(1.0 - _end_weight) + 1
Dim _b = - 6*_begin_weight + 3*(1.0 - _end_weight)
Dim _c = 3*_begin_weight
Dim _d, _t_besier_value As Double
Function Besizer(ByVal _procent as double, ByVal _begin_value as double, ByVal _end_value as double) as Double
	_d = -ClampDbl(_procent,       0, 100)/100.0
	_t_besier_value = (sqrt((-27*_a^2*_d + 9*_a*_b*_c - 2*_b^3)^2 + 4*(3*_a*_c - _b^2)^3) - 27*_a^2*_d + 9*_a*_b*_c - 2*_b^3)^(1.0/3)/(3*2^(1.0/3)*_a) - (2^(1.0/3)*(3*_a*_c - _b^2))/(3*_a*(sqrt((-27*_a^2*_d + 9*_a*_b*_c - 2*_b^3)^2 + 4*(3*_a*_c - _b^2)^3) - 27*_a^2*_d + 9*_a*_b*_c - 2*_b^3)^(1.0/3)) - _b/(3*_a)
	Besizer = _begin_value + (_end_value - _begin_value)*( 3*(1-_t_besier_value)*_t_besier_value^2 + _t_besier_value^3 ) 
End Function

Function VertexBesier(ByVal _procent as double, ByVal _begin_value as vertex, ByVal _end_value as vertex) as Vertex
	Dim _v As Vertex
	_v.x = Besizer(_procent, _begin_value.x, _end_value.x)
	_v.y = Besizer(_procent, _begin_value.y, _end_value.y)
	_v.z = Besizer(_procent, _begin_value.z, _end_value.z)
	VertexBesier = _v
End Function

