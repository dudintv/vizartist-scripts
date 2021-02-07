Dim info As String = "Analogue of Temo plugin, only for Viz4 Fusion material.

It implements the classic technique of texture sprite.
The texture is prepared as a chessboard with an arbitrary count of cells by the vertical and horizontal axis.
Each horizontal or vertical cell should be equal to each other.

Author — Dmitry Dudin, dudin.tv"

'--------------------------------------------------------------------

Dim tiles_count_x, tiles_count_y, show_tile_x, show_tile_y, show_index As Integer
Dim offset_x, offset_y, scale_x, scale_y As Double

Dim s As String
Dim arr_names, arr_line_names As Array[String]
Dim arr_arr_names As Array[Array[String]]

Dim main_axis As Integer
Dim MAIN_AXIS_X = 0
Dim main_axis_button_names As Array[String]
main_axis_button_names.Push("X")
main_axis_button_names.Push("Y")

Dim selection_mode As Integer
Dim SELECTION_MODE_NUMBER = 0
Dim SELECTION_MODE_NAME = 1
Dim selection_mode_button_names As Array[String]
selection_mode_button_names.Push("Number")
selection_mode_button_names.Push("Name")

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterInt("num_tiles_horizontal", "Number of tiles horizontal", 1, 1, 999)
	RegisterParameterInt("num_tiles_vertical", "Number of tiles vertical", 1, 1, 999)	
	RegisterRadioButton("selection_mode", "Selection mode", 0, selection_mode_button_names)
	' for NUMBER mode:
	RegisterRadioButton("main_axis", "Main axis", 0, main_axis_button_names)
	RegisterParameterInt("show_tile_number", "Show tile number", 0, 1, 99999)
	' for NAME mode:
	RegisterParameterText("names", "", 40, 20)
	RegisterParameterString("show_name", "Show name", "", 99, 99, "")
	RegisterParameterBool("hide_if_cant_name", "Hide if cant find name", true)
end sub

sub OnInit()
	CalcTexturePosition()
	ParseNames()
end sub

sub OnParameterChanged(parameterName As String)
	if parameterName == "selection_mode" then
		if GetParameterInt("selection_mode") == SELECTION_MODE_NUMBER then
			SendGuiParameterShow("main_axis", SHOW)
			SendGuiParameterShow("show_tile_number", SHOW)
			SendGuiParameterShow("names", HIDE)
			SendGuiParameterShow("show_name", HIDE)
			SendGuiParameterShow("hide_if_cant_name", HIDE)
		elseif GetParameterInt("selection_mode") == SELECTION_MODE_NAME then
			SendGuiParameterShow("main_axis", HIDE)
			SendGuiParameterShow("show_tile_number", HIDE)
			SendGuiParameterShow("names", SHOW)
			SendGuiParameterShow("show_name", SHOW)
			SendGuiParameterShow("hide_if_cant_name", SHOW)
		end if
	elseif parameterName == "names" then
		ParseNames()
	end If	
	CalcTexturePosition()
end sub

Sub ParseNames()
	s = GetParameterString("names")
	s.Trim()
	s.Split("\n", arr_names)
	arr_arr_names.Clear()
	for i=0 to arr_names.ubound
		arr_names[i].Trim()
		arr_names[i].Split("|", arr_line_names)
		for y=0 to arr_line_names.ubound
			arr_line_names[y].Trim()
		next
		arr_arr_names.Push(arr_line_names)
	next
End Sub

Function FindIndexByName(name As String) As Integer
	name.Trim()
	if name == "" then
		FindIndexByName = -1
		Exit Function
	end if
	for i=0 to arr_arr_names.ubound
		for y=0 to arr_arr_names[i].ubound
			if arr_arr_names[i][y] == name then
				FindIndexByName = i*GetParameterInt("num_tiles_horizontal") + y
				Exit Function
			end if
		next
	next
	FindIndexByName = -1
End Function

Sub CalcTexturePosition()
	if GetParameterInt("selection_mode") == SELECTION_MODE_NUMBER then
		show_index = GetParameterInt("show_tile_number") - 1
	elseif GetParameterInt("selection_mode") == SELECTION_MODE_NAME then
		show_index = FindIndexByName(GetParameterString("show_name"))
	end if
	
	if GetParameterInt("selection_mode") == SELECTION_MODE_NAME AND GetParameterBool("hide_if_cant_name") then
		this.active = (show_index >= 0)
	else
		this.active = true
		if show_index < 0 then show_index = 0
	end if
	
	tiles_count_x = GetParameterInt("num_tiles_horizontal")
	tiles_count_y = GetParameterInt("num_tiles_vertical")
	
	if GetParameterInt("selection_mode") == SELECTION_MODE_NAME OR GetParameterInt("main_axis") == MAIN_AXIS_X then
		show_tile_x = show_index Mod tiles_count_x
		show_tile_y = show_index/tiles_count_x
	else
		show_tile_x = show_index/tiles_count_y
		show_tile_y = show_index Mod tiles_count_y
	end if
	
	scale_x = 1.0/tiles_count_x
	scale_y = 1.0/tiles_count_y
	offset_x = scale_x*show_tile_x
	offset_y = scale_y*(1-show_tile_y)
	
	SendCommand("#" & this.VizId & "*MATERIAL_DEFINITION*SCALE_UV SET " & DoubleToString(scale_x, 5) & " " & DoubleToString(scale_y, 5))
	SendCommand("#" & this.VizId & "*MATERIAL_DEFINITION*OFFSET_UV SET " & DoubleToString(offset_x, 5) & " " & DoubleToString(offset_y, 5))
End Sub
