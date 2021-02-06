Dim info As String = "Analogue of Temo plugin, only for Viz4 Fusion material.

It implements the classic technique of texture sprite.
The texture is prepared as a chessboard with an arbitrary count of cells by the vertical and horizontal axis.
Each horizontal or vertical cell should be equal to each other.

Author — Dmitry Dudin, dudin.tv"

Dim tiles_count_x, tiles_count_y, show_tile_x, show_tile_y As Integer
Dim offset_x, offset_y, scale_x, scale_y As Double

Dim main_axis As Integer
Dim MAIN_AXIS_X = 1
Dim buttonNames As Array[String]
buttonNames.Push("X")
buttonNames.Push("Y")

sub OnInitParameters()
	RegisterInfoText(info)
	RegisterParameterInt("num_tiles_horizontal", "Number of tiles horizontal", 1, 1, 999)
	RegisterParameterInt("num_tiles_vertical", "Number of tiles vertical", 1, 1, 999)
	RegisterRadioButton("main_axis", "Main axis", 1, buttonNames)
	RegisterParameterInt("show_tile_number", "Show tile number", 1, 1, 99999)
end sub

sub OnInit()
	CalcTexturePosition()
end sub

sub OnParameterChanged(parameterName As String)
	CalcTexturePosition()
end sub

Sub CalcTexturePosition()
	tiles_count_x = GetParameterInt("num_tiles_horizontal")
	tiles_count_y = GetParameterInt("num_tiles_vertical")
	
	if GetParameterInt("main_axis") == MAIN_AXIS_X then
		show_tile_x = GetParameterInt("show_tile_number") Mod tiles_count_x
		show_tile_y = GetParameterInt("show_tile_number")/tiles_count_x
	else
		show_tile_x = GetParameterInt("show_tile_number")/tiles_count_y
		show_tile_y = GetParameterInt("show_tile_number") Mod tiles_count_y
	end if
	
	scale_x = 1.0/tiles_count_x
	scale_y = 1.0/tiles_count_y
	offset_x = 1 - scale_x*show_tile_x
	offset_y = 1 - scale_y*show_tile_y
	
	SendCommand("#" & this.VizId & "*MATERIAL_DEFINITION*SCALE_UV SET " & DoubleToString(scale_x, 5) & " " & DoubleToString(scale_y, 5))
	SendCommand("#" & this.VizId & "*MATERIAL_DEFINITION*OFFSET_UV SET " & DoubleToString(offset_x, 5) & " " & DoubleToString(offset_y, 5))
End Sub
