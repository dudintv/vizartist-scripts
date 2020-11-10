function hsl2rgb (h As Double, s As Double, l As Double)  As Color
    Dim r, g, b, m, c, x As Double

    h *= 6.0
    if h < 0 then h = 6 - (-h Mod 6)
    h = h Mod 6

    s = Max(0, Min(1, s))
    l = Max(0, Min(1, l))

    c = (1 - Abs((2 * l) - 1)) * s
    x = c * (1 - Abs((h Mod 2) - 1))

    if h < 1 then
        r = c
        g = x
        b = 0
    elseif h < 2 then
        r = x
        g = c
        b = 0
    elseif h < 3 then
        r = 0
        g = c
        b = x
    elseif h < 4 then
        r = 0
        g = x
        b = c
    elseif h < 5 then
        r = x
        g = 0
        b = c
    else
        r = c
        g = 0
        b = x
    end if

    m = l - c / 2.0
    r = (r + m)
    g = (g + m)
    b = (b + m)

    hsl2rgb = CColor(r, g, b)

End Function