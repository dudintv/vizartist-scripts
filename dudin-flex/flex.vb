RegisterPluginVersion(1,3,1)
Dim info As String = "
Flex-position. Copies flex-logic from CSS3 / HTML5.
Developer: Dmitry Dudin. 
http://dudin.tv/scripts/flex
"

'SETTING
Dim treshhold As Double = 0.001

'STUFF
Dim c_gabarit, c_root As Container
Dim children As Array[Container]
Dim min_gap_param, mult_gap_param, shift_gap_param, gap_shift, power_magnetic_gap, gaps, freespace As Double
Dim gabarit, child_gabarit, v1, v2, child_v1, child_v2, item_gabarit As Vertex
Dim mode_axis, mode_gabarit_source, mode_justify, mode_align As Integer
Dim width_step, sum_children_width, sum_children_height, total_children_width, total_children_height, gap, start As Double
Dim arr_width, arr_height, arr_shift_x, arr_shift_y As Array[Double]
Dim childrenCount, prev_childrenCount As Integer

'ANIMATION STUFF
Structure Transition
	target_pos, prev_pos As Vertex
End Structure
Dim arr_transitions As Array[Transition]
Dim playhead As Double = 1.0 
' playhead = [0.0 ... 1.0]
' playhead = 1.0 means STOP
Dim currentAnimValue As Double
Dim hasNewState As Boolean
Dim handle_x_pos, handle_y_pos As Boolean
Dim prev_pos_gabarit, prev_bb_gabarit AS Vertex
Dim prev_pos_child, prev_bb_child As Array[Vertex]


'INTERFACE
Dim arr_axis As Array[String]
arr_axis.Push(" X ")
arr_axis.Push(" Y ")
Dim arr_gabarit_source As Array[String]
arr_gabarit_source.Push("whole container")
arr_gabarit_source.Push("first sub-container")
Dim arr_justify As Array[String]
arr_justify.Push("start")
arr_justify.Push("end")
arr_justify.Push("center")
arr_justify.Push("space-between")
arr_justify.Push("space-around")
Dim arr_align As Array[String]
arr_align.Push("free")
arr_align.Push("min")
arr_align.Push("center")
arr_align.Push("max")
Dim arr_ease As Array[String]
arr_ease.Push("linear")
arr_ease.Push("quad")
arr_ease.Push("cubic")
arr_ease.Push("quart")
arr_ease.Push("sine")
arr_ease.Push("circ")
arr_ease.Push("back")
arr_ease.Push("bounce")

sub OnInitParameters()
	RegisterParameterContainer("root", "Root (or this)")
	RegisterParameterContainer("gabarit", "Area")
	RegisterRadioButton("axis", "Axis", 0, arr_axis)
	RegisterRadioButton("gabarit_source", "Size of children", 0, arr_gabarit_source)
	RegisterRadioButton("justify", "Justify", 0, arr_justify)
	RegisterRadioButton("align", "Align", 0, arr_align)
	RegisterParameterDouble("mult_gap", "Multiply gap, %", 0, -1000, 1000)
	RegisterParameterDouble("power_gap", "Magnetic gap", 0, -100, 10000)
	RegisterParameterDouble("min_gap", "Min gap", 0, 0, 1000)
	RegisterParameterDouble("shift_gap", "Shift gap", 0, -1000, 1000)
	RegisterParameterBool("collapse_if_overflow", "Collapse gap if overflow", true)
	RegisterParameterBool("is_animated", "Animate transitions", false)
	RegisterParameterDouble("transition_duration", "Transition duration (sec)", 1.0, 0, 1000)
	RegisterRadioButton("ease_fn", "Ease function", 0, arr_ease)
end sub

sub OnInit()
	c_root = GetParameterContainer("root")
	if c_root == null then c_root = this
	c_gabarit = GetParameterContainer("gabarit")
	mode_axis = GetParameterInt("axis")
	mode_gabarit_source = GetParameterInt("gabarit_source")
	mode_justify = GetParameterInt("justify")
	mode_align = GetParameterInt("align")
	mult_gap_param = GetParameterDouble("mult_gap")
	power_magnetic_gap = GetParameterDouble("power_gap")/100.0 + 1.0
	shift_gap_param = GetParameterDouble("shift_gap")
	min_gap_param = GetParameterDouble("min_gap")
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
	
	if mode_justify == 4 then
		SendGuiParameterShow("mult_gap", SHOW)
		SendGuiParameterShow("power_gap", SHOW)
	else
		SendGuiParameterShow("mult_gap", HIDE)
		SendGuiParameterShow("power_gap", HIDE)
	end if
	
	SendGuiParameterShow("transition_duration", CInt(GetParameterBool("is_animated")))
	SendGuiParameterShow("ease_fn", CInt(GetParameterBool("is_animated")))
	hasNewState = true
end sub

'''''''''''''''''''''''''''''''''''''''''''''''''''''

Function GetChildGabarit(child As Container) As Vertex
	if mode_gabarit_source == 1 AND child.ChildContainerCount > 0 then
		GetChildGabarit = child.FirstChildContainer.GetBoundingBoxDimensions()
		GetChildGabarit.x *= child.scaling.x * child.FirstChildContainer.scaling.x
		GetChildGabarit.y *= child.scaling.y * child.FirstChildContainer.scaling.y
	else
		GetChildGabarit = child.GetBoundingBoxDimensions()
		GetChildGabarit.x *= child.scaling.x
		GetChildGabarit.y *= child.scaling.y
	end if
End Function

Sub CalcGapAndStart(gabarit_size As Double, sum_children_size As Double)
	freespace = gabarit_size - sum_children_size
	gap_shift = freespace*(mult_gap_param/100)/(childrenCount-1)
	
	'CALC BASE GAP
	Select Case mode_justify
	Case 0 To 2
		'0 - start
		'1 - end
		'2 - center
		gap = 0 + gap_shift
	Case 3
		'3 - space-between
		gap = freespace/(childrenCount-1)
	Case 4
		'4 - space-around
		gap = freespace/(childrenCount+1)
		'I want the expecting accurate of position with "Shift of gap" = 100.0
		gap += (2*gap)/(childrenCount-1)*(mult_gap_param/100)
	End Select
	
	gaps = gap*(childrenCount-1)
	
	'ADD MAGNETIC
	Select Case mode_justify
	Case 0 To 2
		'0 - start
		'1 - end
		'2 - center
		gap = (freespace/(childrenCount-1))*(gaps/freespace)^power_magnetic_gap
	Case 3
		'3 - space-between
		'no magnetic!
	Case 4
		'4 - space-around
		gap = (freespace/(childrenCount-1))*(gaps/freespace)^power_magnetic_gap
	End Select
	
	'CONSIDER MIN GAP
	if gap < min_gap_param then
		if GetParameterBool("collapse_if_overflow") AND (gabarit_size - sum_children_size)/(childrenCount-1) < min_gap_param then
			gap = 0
		else
			gap = min_gap_param
		end if
	end if
	gap += shift_gap_param
	
	'CACL START SHIFT
	Select Case mode_justify
	Case 0
		'start
		start = 0
	Case 1
		'end
		start = gabarit_size - sum_children_size - gap*(childrenCount-1)
	Case 2 to 4
		'2 - center
		'3 - space-between
		'4 - space-around
		start = (gabarit_size - sum_children_size)/2.0 - gap*(childrenCount-1)/2.0
	End Select
	
'	println("")
'	println("childrenCount = " & childrenCount)
'	println("gabarit_size = " & gabarit_size & " | sum_children_size = " & sum_children_size & " | gap = " & gap & " | start = " & start)
End Sub

Dim visibleChildrenCount As Integer
Function GetVisibleChildContainerCount() As Integer
	visibleChildrenCount = 0
	for i=0 to c_root.ChildContainerCount-1
		item_gabarit = c_root.GetChildContainerByIndex(i).GetTransformedBoundingBoxDimensions()
		if c_root.GetChildContainerByIndex(i).active AND item_gabarit.X > treshhold AND item_gabarit.Y > treshhold then
			visibleChildrenCount += 1
		end if
	next
	GetVisibleChildContainerCount = visibleChildrenCount
End Function

Sub ChildrenCountWasChanged()
	children.clear()
	arr_transitions.clear()
	prev_pos_child.clear()
	prev_bb_child.clear()
	for i=0 to c_root.ChildContainerCount-1
		item_gabarit = c_root.GetChildContainerByIndex(i).GetTransformedBoundingBoxDimensions()
		
		if c_root.GetChildContainerByIndex(i).active AND item_gabarit.X > treshhold AND item_gabarit.Y > treshhold then
			children.push(c_root.GetChildContainerByIndex(i))
			Dim t As Transition
			t.prev_pos = children[i].position.xyz
			arr_transitions.Push(t)
			prev_pos_child.Push(children[i].position.xyz)
			prev_bb_child.Push(children[i].GetTransformedBoundingBoxDimensions())
		end if
	next
	childrenCount = children.size
End Sub

Dim tv1, tv2, cntr As Vertex
Sub Update()
	' c_gabarit.GetTransformedBoundingBox(v1,v2)
	' v1 = c_root.WorldPosToLocalPos(v1)
	' v2 = c_root.WorldPosToLocalPos(v2)
	
	' v1.x = (v1.x - c_root.position.x)/c_root.scaling.x
	' v1.y = (v1.y - c_root.position.y)/c_root.scaling.y
	' v1.z = (v1.z - c_root.position.z)/c_root.scaling.z
	' v2.x = (v2.x - c_root.position.x)/c_root.scaling.x
	' v2.y = (v2.y - c_root.position.y)/c_root.scaling.y
	' v2.z = (v2.z - c_root.position.z)/c_root.scaling.z
	
	gabarit = GetBountingBoxWithinAnotherContainer(c_gabarit, c_root, v1, v2)
	
	arr_width.clear
	arr_height.clear
	arr_shift_x.clear
	arr_shift_y.clear
	total_children_width = 0
	total_children_height = 0
	for i=0 to children.ubound
		child_gabarit = SetChildrenVertexes(children[i])   'set child_v1 and child_v2
		
		arr_width.push(child_gabarit.x)
		arr_height.push(child_gabarit.y)
		total_children_width += child_gabarit.x
		total_children_height += child_gabarit.y
		cntr = ProjectVertexFromOneContainerToAnother(-children[i].center.xyz, children[i], c_root) 

		arr_shift_x.push( child_v1.x - cntr.x )
		arr_shift_y.push( child_v1.y - cntr.y )
	next
	
	if mode_axis == 0 then 'X
		CalcGapAndStart(gabarit.X, total_children_width)
	elseif mode_axis == 1 then 'Y
		CalcGapAndStart(gabarit.Y, total_children_height)
	end if
End Sub

Function SetChildrenVertexes(child As Container) As Vertex
  if mode_gabarit_source == 1 AND child.ChildContainerCount > 0 then
    SetChildrenVertexes = GetBountingBoxWithinAnotherContainer(child.FirstChildContainer, c_root, child_v1, child_v2)
  else
    SetChildrenVertexes = GetBountingBoxWithinAnotherContainer(child, c_root, child_v1, child_v2)
  end if
End Function

'''''''''''''''''''''''''''''''''''''''''''''''''''''

Sub StartTransition()
	playhead = 0
	for i=0 to arr_transitions.ubound
		arr_transitions[i].prev_pos.x = children[i].position.x
		arr_transitions[i].prev_pos.y = children[i].position.y
	next
End Sub

Sub StopTransition()
	playhead = 1.0
	hasNewState = false
	for i=0 to arr_transitions.ubound
		arr_transitions[i].prev_pos = arr_transitions[i].target_pos
		prev_pos_child[i] = children[i].position.xyz
		prev_bb_child[i]  = children[i].GetTransformedBoundingBoxDimensions()
	next
End Sub

Dim x As Double
Dim PI As Double = 3.1415926535
Sub CalcCurrentAnimValue()
	x = playhead
	Select Case GetParameterInt("ease_fn")
	Case 0
		'linear
		currentAnimValue = x
	Case 1
		'quad
		currentAnimValue = 1 - (1 - x)*(1 - x)
	Case 2
		'cubic
		currentAnimValue = 1 - (1 - x)*(1 - x)*(1 - x)
	Case 3
		'quart
		currentAnimValue = 1 - (1 - x)*(1 - x)*(1 - x)*(1 - x)
	Case 4
		'sine
		currentAnimValue = sin((x * PI) / 2)
	Case 5
		'circ
		currentAnimValue = sqrt(1 - (x - 1)*(x - 1))
	Case 6
		'back
		Dim c1 As Double = 1.70158
		Dim c3 As Double = c1 + 1
		currentAnimValue = 1 + c3*(x - 1)*(x - 1)*(x - 1) + c1*(x - 1)*(x - 1)
	Case 7
		'bounce
		Dim s As Double = 7.5625
		Dim p As Double = 2.75
		Dim l As Double
		if x < 1/p then
			currentAnimValue = s * x * x
		else
			if x < 2/p then
				x -= 1.5/p
				currentAnimValue = s * x * x + 0.75
			else
				if x < 2.5/p then
					x -= 2.25/p
					currentAnimValue = s * x * x + 0.9375
				else
					x -= 2.625/p
					currentAnimValue = s * x * x + 0.984375
				end if
			end if
		end if
	End Select
End Sub

'''''''''''''''''''''''''''''''''''''''''''''''''''''

sub OnExecPerField()
	' check count of children
	if GetVisibleChildContainerCount() <> prev_childrenCount then
		ChildrenCountWasChanged()
		hasNewState = true
		prev_childrenCount = GetVisibleChildContainerCount()
	end if
	
	Update()

	' check new state
	if c_gabarit.position.xyz <> prev_pos_gabarit OR c_gabarit.GetTransformedBoundingBoxDimensions() <> prev_bb_gabarit then
		hasNewState = true
		prev_pos_gabarit = c_gabarit.position.xyz
		prev_bb_gabarit  = c_gabarit.GetTransformedBoundingBoxDimensions()
	else
		for i=0 to children.ubound
			if children[i].GetTransformedBoundingBoxDimensions() <> prev_bb_child[i] then
				hasNewState = true
				prev_pos_child[i] = children[i].position.xyz
				prev_bb_child[i]  = children[i].GetTransformedBoundingBoxDimensions()
			end if
		next
	end if

	if hasNewState then
		' trigger to start — hasNewState = true AND playhead >= 1.0
		if playhead >= 1.0 then StartTransition()

		' calc playhead
		if GetParameterBool("is_animated") AND playhead < 1.0 then
			playhead += System.CurrentRefreshRate / GetParameterDouble("transition_duration")
			CalcCurrentAnimValue()
			if playhead >= 1.0 then
				StopTransition()
			end if
		end if

		sum_children_width = 0
		sum_children_height = 0
		for i=0 to children.ubound
			SetChildrenVertexes(children[i])   'set child_v1 and child_v2
			
			If mode_axis == 0 then
				'X
				handle_x_pos = true
				handle_y_pos = mode_align <> 0

				arr_transitions[i].target_pos.x = v1.x + start + i*gap - arr_shift_x[i] + sum_children_width
				sum_children_width += arr_width[i]
				Select Case mode_align
				Case 1
					'min align
					arr_transitions[i].target_pos.y = v1.y - child_v1.y*children[i].scaling.y
				Case 2
					'center align
					arr_transitions[i].target_pos.y = (v1.y+v2.y)/2.0 - (child_v1.y+child_v2.y)*children[i].scaling.y/2.0
				Case 3
					'max align
					arr_transitions[i].target_pos.y = v2.y - child_v2.y*children[i].scaling.y
				End Select 
			Elseif mode_axis == 1 then
				'Y
				handle_x_pos = mode_align <> 0
				handle_y_pos = true

				arr_transitions[i].target_pos.y = v2.y - start - i*gap - arr_shift_y[i]
				sum_children_height += arr_height[i]
				Select Case mode_align
				Case 1
					'min align
					arr_transitions[i].target_pos.x = v1.x - child_v1.x*children[i].scaling.x
				Case 2
					'center align
					arr_transitions[i].target_pos.x = (v1.x+v2.x)/2.0 - (child_v1.x+child_v2.x)*children[i].scaling.x/2.0
				Case 3
					'max align
					arr_transitions[i].target_pos.x = v2.x - child_v2.x*children[i].scaling.x
				End Select
			end if
		next

		for i=0 to children.ubound
			if GetParameterBool("is_animated") then
				if handle_x_pos then children[i].position.x = arr_transitions[i].prev_pos.x + (arr_transitions[i].target_pos.x-arr_transitions[i].prev_pos.x) * currentAnimValue
				if handle_y_pos then children[i].position.y = arr_transitions[i].prev_pos.y + (arr_transitions[i].target_pos.y-arr_transitions[i].prev_pos.y) * currentAnimValue
			else
				'no animation in transition
				if handle_x_pos then children[i].position.x = arr_transitions[i].target_pos.x
				if handle_y_pos then children[i].position.y = arr_transitions[i].target_pos.y
				StopTransition()
			end if
		next
	end if
end sub


'''''''''''''''''''''''''''''''''''''''''''''''''''''

Function GetBountingBoxWithinAnotherContainer(_c_bb As Container, _c_con As Container, ByRef _v1 As Vertex, ByRef _v2 As Vertex) As Vertex
	if _c_bb <> null then
		_c_bb.GetBoundingBox(_v1, _v2)
		_v1 *= _c_bb.matrix
		_v2 *= _c_bb.matrix
		Dim _m As Matrix = _c_con.matrix
		_m.Invert()
		_v1 *= _m
		_v2 *= _m
		GetBountingBoxWithinAnotherContainer = _v2 - _v1
	else
		GetBountingBoxWithinAnotherContainer = CVertex(0,0,0)
	end if
End Function

Function ProjectVertexFromOneContainerToAnother(ByVal _v As Vertex, _c_from As Container, _c_to As Container) As Vertex
	Dim _m_from As Matrix = _c_from.matrix
	Dim _m_to As Matrix = _c_to.matrix
	_m_to.Invert()
	
	_v *= _c_from.matrix
	_v *= _m_to
	ProjectVertexFromOneContainerToAnother = _v
End Function
