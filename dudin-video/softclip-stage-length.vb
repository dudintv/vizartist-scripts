Dim p_softclip As PluginInstance
Dim fps As Integer
Dim k_end, k_loop As Keyframe

sub OnInitParameters()
	RegisterParameterContainer("softclip", "Softclip")
end sub

sub OnInit()
	if GetParameterContainer("softclip") <> null then
		p_softclip = GetParameterContainer("softclip").GetFunctionPluginInstance("SoftClip")
	else
		p_softclip = this.GetFunctionPluginInstance("SoftClip")
	end if
	
	k_end = GetParameterContainer("softclip").FindKeyframeOfObject("end")
	k_loop = Stage.FindDirector(GetParameterContainer("softclip").name).FindActionChannel("loop").FindKeyframe("loop")
end sub

sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecPerField()
	fps = p_softclip.GetParameterInt("fps")
	k_end.Time = CDbl(p_softclip.GetParameterString("clip_length"))/fps
	k_end.IntValue = CInt( fps*k_end.Time - 1 )
	k_loop.Time = k_end.Time
	
	if p_softclip.GetParameterBool("loop") then
		k_loop.ActionString = "THIS_DIRECTOR START"
	else
		k_loop.ActionString = ""
	end if
end sub
