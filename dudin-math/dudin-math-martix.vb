Function MatrixMult(_m1 As Matrix, _m2 As Matrix) As Matrix
	'Matrix [0, 1, 2, 3] is first column, [4, 5, 6, 7] is the second, etc.
	'0 4 8  12
	'1 5 9  13
	'2 6 10 14
	'3 7 11 15
	
	Dim _m_new As Matrix
	
	Dim _arr1_r1, _arr1_r2, _arr1_r3, _arr1_r4, _arr2_c1, _arr2_c2, _arr2_c3, _arr2_c4 As Array[Double]
	
	'first row 1
	_arr1_r1.Push(_m1[0])
	_arr1_r1.Push(_m1[4])
	_arr1_r1.Push(_m1[8])
	_arr1_r1.Push(_m1[12])
	
	'second row 1
	_arr1_r2.Push(_m1[1])
	_arr1_r2.Push(_m1[5])
	_arr1_r2.Push(_m1[9])
	_arr1_r2.Push(_m1[13])
	
	'third row 1
	_arr1_r3.Push(_m1[2])
	_arr1_r3.Push(_m1[6])
	_arr1_r3.Push(_m1[10])
	_arr1_r3.Push(_m1[14])
	
	'forth row 1
	_arr1_r4.Push(_m1[3])
	_arr1_r4.Push(_m1[7])
	_arr1_r4.Push(_m1[11])
	_arr1_r4.Push(_m1[15])
	
	
	'first column 2
	_arr2_c1.Push(_m2[0])
	_arr2_c1.Push(_m2[1])
	_arr2_c1.Push(_m2[2])
	_arr2_c1.Push(_m2[3])
	
	'second column 2
	_arr2_c2.Push(_m2[4])
	_arr2_c2.Push(_m2[5])
	_arr2_c2.Push(_m2[6])
	_arr2_c2.Push(_m2[7])
	
	'third column 2
	_arr2_c3.Push(_m2[8])
	_arr2_c3.Push(_m2[9])
	_arr2_c3.Push(_m2[10])
	_arr2_c3.Push(_m2[11])
	
	'forth column 2
	_arr2_c4.Push(_m2[12])
	_arr2_c4.Push(_m2[13])
	_arr2_c4.Push(_m2[14])
	_arr2_c4.Push(_m2[15])
	
	'0 4 8  12
	'1 5 9  13
	'2 6 10 14
	'3 7 11 15
	_m_new[0] = DotProduct(_arr1_r1, _arr2_c1)
	_m_new[1] = DotProduct(_arr1_r2, _arr2_c1)
	_m_new[2] = DotProduct(_arr1_r3, _arr2_c1)
	_m_new[3] = DotProduct(_arr1_r4, _arr2_c1)
	
	_m_new[4] = DotProduct(_arr1_r1, _arr2_c2)
	_m_new[5] = DotProduct(_arr1_r2, _arr2_c2)
	_m_new[6] = DotProduct(_arr1_r3, _arr2_c2)
	_m_new[7] = DotProduct(_arr1_r4, _arr2_c2)
	
	_m_new[8]  = DotProduct(_arr1_r1, _arr2_c3)
	_m_new[9]  = DotProduct(_arr1_r2, _arr2_c3)
	_m_new[10] = DotProduct(_arr1_r3, _arr2_c3)
	_m_new[11] = DotProduct(_arr1_r4, _arr2_c3)
	
	_m_new[12] = DotProduct(_arr1_r1, _arr2_c4)
	_m_new[13] = DotProduct(_arr1_r2, _arr2_c4)
	_m_new[14] = DotProduct(_arr1_r3, _arr2_c4)
	_m_new[15] = DotProduct(_arr1_r4, _arr2_c4)
	
	MatrixMult = _m_new
End Function

Function DotProduct(_arr1 As Array[Double], _arr2 As Array[Double]) As Double
	'(1, 2, 3) • (8, 10, 12) = 1×8 + 2×10 + 3×12 = 64
	if _arr1.size == _arr2.size then
		Dim _sum As Double = 0
		for i=0 to _arr1.ubound
			_sum += _arr1[i]*_arr2[i]
		next
		DotProduct = _sum
	else
		println("Dot Product ERROR: different count elements.")
		DotProduct = 0
	end if
End Function

'------------------------------------------------

Sub PrintMatrix4(_m As Matrix)
	Dim _max_width As Integer = DoubleToString(_m[0], 2).length
	for i=1 to 15
		_max_width = max(_max_width, DoubleToString(_m[i], 2).length)
	next
	for i=0 to 3
		println( "[" & DoubleToString(_m[i], 2, _max_width) & "] [" & DoubleToString(_m[i+4], 2, _max_width) & "] [" & DoubleToString(_m[i+8], 2, _max_width) & "] [" & DoubleToString(_m[i+12], 2, _max_width) & "]" )
	next
End Sub
