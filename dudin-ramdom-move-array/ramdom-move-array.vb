Dim version As String = "1.01 (21 сентября 2017)"
Dim info As String = "Колебания непосредственных потомков по оси Y"

Dim speed, amplitude, random As Double
Dim rand_amplitudes, cur_amplitudes As Array[Double]
Dim ticks As Array[Integer]
Dim speeds As Array[Double]
Dim childs As Array[Container]

sub OnInitParameters()
	RegisterInfoText(version & "\n" & info)
	RegisterParameterDouble("amplitude", "Амплитуда колебаний", 1, 0, 9999)
	RegisterParameterDouble("speed", "Скорость", 50, 0, 9999)
	RegisterParameterDouble("random", "Фактор случайности", 50, 0, 100)
end sub

sub OnInit()
	amplitude = GetParameterDouble("amplitude")
	speed = GetParameterDouble("speed")
	random = GetParameterDouble("random")
	
	childs.Clear()
	for i=1 to this.ChildContainerCount
		childs.Push(this.GetChildContainerByIndex(i))
	next
	
	SetAllRandom()
end sub
sub OnGeometryChanged(geom As Geometry)
	OnInit()
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecPerField()
	for i=0 to childs.UBound-1
		'println("i = " & i)
		if ticks[i] > speeds[i] then
			ticks[i] = 0
			SetRandom(i)
		end if
		ticks[i] += 1
		cur_amplitudes[i] = cur_amplitudes[i] + (rand_amplitudes[i] - cur_amplitudes[i])*0.01
		childs[i].Position.Y = cur_amplitudes[i] * Sin( 2*3.14*ticks[i]/speeds[i] )
	next
end sub


Sub SetAllRandom()
	rand_amplitudes.Clear()
	cur_amplitudes.Clear()
	ticks.Clear()
	for i=0 to childs.UBound
		ticks.Push(0)
		speeds.Push(speed)
		rand_amplitudes.Push(0)
		SetRandom(i)
		cur_amplitudes.Push(rand_amplitudes[i])
	next
End Sub

Sub SetRandom(num as integer)
	rand_amplitudes[num] = amplitude*(100.0-random)/100.0 + (amplitude*random/100.0)*Random()
	speeds[num] = speed*(100.0-random)/100.0 + (speed*random/100.0)*Random()
End Sub