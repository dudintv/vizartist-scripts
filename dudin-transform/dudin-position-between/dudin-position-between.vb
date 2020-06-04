RegisterPluginVersion(1,0,0)
'Author: Dmitry Dudin, http://dudin.tv

Dim c_first, c_second As Container
Dim vFirst1, vFirst2, vSecond1, vSecond2, vThis1, vThis2 As Vertex
Dim first1, first2, second1, second2, this1, this2, thisPivot As Double
Dim first_border, second_border As Double

Dim typeCenterNames As Array[String]
typeCenterNames.Push("Pivot point")
typeCenterNames.Push("Bounding Box")
Dim axesNames As Array[String]
axesNames.Push(" X ")
axesNames.Push(" Y ")
sub OnInitParameters()
	RegisterParameterContainer("first", "First Container")
	RegisterParameterContainer("second", "Second Container")
	RegisterRadioButton("axis", "Axis", 0, axesNames)
	RegisterRadioButton("this_center", "This Center", 0, typeCenterNames)
end sub

sub OnInit()
	c_first = GetParameterContainer("first")
	c_second = GetParameterContainer("second")
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecPerField()
	c_first.GetTransformedBoundingBox(vFirst1, vFirst2)
	c_second.GetTransformedBoundingBox(vSecond1, vSecond2)
	vFirst1 = this.WorldPosToLocalPos(vFirst1)
	vFirst2 = this.WorldPosToLocalPos(vFirst2)
	vSecond1 = this.WorldPosToLocalPos(vSecond1)
	vSecond2 = this.WorldPosToLocalPos(vSecond2)
	if GetParameterInt("axis") == 0 then
		'x
		first1 = vFirst1.x
		first2 = vFirst2.x
		second1 = vSecond1.x
		second2 = vSecond2.x
	elseif GetParameterInt("axis") == 1 then
		'y
		first1 = vFirst1.y
		first2 = vFirst2.y
		second1 = vSecond1.y
		second2 = vSecond2.y
	end if
	
	if (first1 + first2)/2.0 < (second1 + second2)/2.0 then
		' the FIRST is to the left of the SECOND
		first_border = first2
		second_border = second1
	else
		' the FIRST is to the right of the SECOND
		first_border = first1
		second_border = second2
	end if
	
	
	if GetParameterInt("this_center") == 0 then
		' Pivot point
		SetCenterPosition( (first_border + second_border)/2.0 )
	elseif GetParameterInt("this_center") == 1 then
		' Bounding Box
		this.GetBoundingBox(vThis1, vThis2)
		if GetParameterInt("axis") == 0 then
			'x
			this1 = vThis1.x
			this2 = vThis2.x
			thisPivot = this.center.x * this.scaling.x
		elseif GetParameterInt("axis") == 1 then
			'y
			this1 = vThis1.y
			this2 = vThis2.y
			thisPivot = this.center.y * this.scaling.y
		end if
		SetCenterPosition( (first_border + second_border)/2.0 + (this1 + this2)/2.0 - thisPivot )
	end if
end sub

Sub SetCenterPosition(_v As Double)
	if GetParameterInt("axis") == 0 then
		'x
		this.position.x = _v
	elseif GetParameterInt("axis") == 1 then
		'y
		this.position.y = _v
	end if
End Sub
