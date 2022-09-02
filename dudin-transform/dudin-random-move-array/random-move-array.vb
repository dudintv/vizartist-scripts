Dim version As String = "1.2 (2 September 2021)"
Dim info As String = "Oscillates direct children only by Y axis"

Dim speed, amplitudeX, amplitudeY, amplitudeZ, random As Double
Dim randAmplitudesX, randAmplitudesY, randAmplitudesZ As Array[Double]
Dim curAmplitudesX, curAmplitudesY, curAmplitudesZ As Array[Double]
Dim ticks As Array[Integer]
Dim speeds As Array[Double]
Dim childs As Array[Container]

sub OnInitParameters()
	RegisterInfoText(version & "\n" & info)
	RegisterParameterString("childName", "Name of sub-child", "", 100, 999, "")
	RegisterParameterDouble("amplitudeX", "X Amplitude", 1, 0, 9999)
	RegisterParameterDouble("amplitudeY", "Y Amplitude", 1, 0, 9999)
	RegisterParameterDouble("amplitudeZ", "Z Amplitude", 1, 0, 9999)
	RegisterParameterDouble("speed", "Loop duration", 50, 0, 9999)
	RegisterParameterDouble("random", "Random level", 50, 0, 100)
end sub

sub OnInit()
	amplitudeX = GetParameterDouble("amplitudeX")
	amplitudeY = GetParameterDouble("amplitudeY")
	amplitudeZ = GetParameterDouble("amplitudeZ")
	speed = GetParameterDouble("speed")
	random = GetParameterDouble("random")
	
	childs.Clear()
	for i=0 to this.ChildContainerCount
		if GetParameterString("childName").Length > 0 then
			childs.Push(this.GetChildContainerByIndex(i).FindSubContainer(GetParameterString("childName")))
		else
			childs.Push(this.GetChildContainerByIndex(i))
		end if
	next
	
	SetupAllRandom()
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecPerField()
	for i=0 to childs.UBound-1
		if ticks[i] > speeds[i] then
			ticks[i] = 0
			SetRandom(i)
		end if
		ticks[i] += 1
		curAmplitudesX[i] = curAmplitudesX[i] + (randAmplitudesX[i] - curAmplitudesX[i])*0.01
		curAmplitudesY[i] = curAmplitudesY[i] + (randAmplitudesY[i] - curAmplitudesY[i])*0.01
		curAmplitudesZ[i] = curAmplitudesZ[i] + (randAmplitudesZ[i] - curAmplitudesZ[i])*0.01
		childs[i].Position.X = curAmplitudesX[i] * Sin( 2*3.14*ticks[i]/speeds[i] )
		childs[i].Position.Y = curAmplitudesY[i] * Sin( 2*3.14*ticks[i]/speeds[i] )
		childs[i].Position.Z = curAmplitudesZ[i] * Sin( 2*3.14*ticks[i]/speeds[i] )
	next
end sub


Sub SetupAllRandom()
	randAmplitudesX.Clear()
	randAmplitudesY.Clear()
	randAmplitudesZ.Clear()
	curAmplitudesX.Clear()
	curAmplitudesY.Clear()
	curAmplitudesZ.Clear()
	ticks.Clear()
	for i=0 to childs.UBound
		ticks.Push(0)
		speeds.Push(speed)
		randAmplitudesX.Push(0)
		randAmplitudesY.Push(0)
		randAmplitudesZ.Push(0)
		SetRandom(i)
		curAmplitudesX.Push(randAmplitudesX[i])
		curAmplitudesY.Push(randAmplitudesY[i])
		curAmplitudesZ.Push(randAmplitudesZ[i])
	next
End Sub

Sub SetRandom(num as integer)
	randAmplitudesX[num] = amplitudeX*(100.0-random)/100.0 + (amplitudeX*random/100.0)*Random()
	randAmplitudesY[num] = amplitudeY*(100.0-random)/100.0 + (amplitudeY*random/100.0)*Random()
	randAmplitudesZ[num] = amplitudeZ*(100.0-random)/100.0 + (amplitudeZ*random/100.0)*Random()
	speeds[num] = speed*(100.0-random)/100.0 + (speed*random/100.0)*Random()
End Sub
