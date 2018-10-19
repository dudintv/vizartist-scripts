Dim version As String = "1.02 (19 октября 2018)"
Dim info As String = "Авто масштабирование картинки или rect согласно пропорции. Удобно для портретов."

Dim return as String
Dim arr_size as array[string]
Dim x, y As Integer
Dim image_prop, rect_prop As Double
Dim name, name_prev As String

Dim buttons As Array[String]
buttons.Push("Off")
buttons.Push("Scale rect width")
buttons.Push("Scale image x")
sub OnInitParameters()
	RegisterRadioButton("mode", "Mode", 0, buttons)
end sub
sub OnInit()
	Rescale()
end sub
sub OnParameterChanged(parameterName As String)
	OnInit()
end sub

sub OnExecPerField()
	name = this.texture.Image.Name
	if name <> name_prev then
		Rescale()
		name_prev = name
	end if
end sub

Sub Rescale()
	return = SendCommand("#"&this.texture.vizid&"*INFO GET")  	'(получаем "1024 512 20565", где x=1024, y=512, 20564 kBit)
	return.split(" ",arr_size)	
	x = cInt(arr_size[0])																			
	y = cInt(arr_size[1])																			
	image_prop = x/(y*1.0)
	rect_prop = this.geometry.plugininstance.GetParameterDouble("width") / this.geometry.plugininstance.GetParameterDouble("height")
	
	if GetParameterInt("mode") == 0 then
		'Off
	elseif GetParameterInt("mode") == 1 then
		'Scale rect width
		this.geometry.plugininstance.SetParameterDouble("width", 100*image_prop)
	elseif GetParameterInt("mode") == 2 then
		'Scale image x
		this.texture.MapScaling.x = image_prop / rect_prop
	end if
end Sub
