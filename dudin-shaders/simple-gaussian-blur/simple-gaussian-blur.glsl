@registerParametersBegin
@registerParameterSampler2D(iChannel0, "Texture0")
@registerParameterFloat(directions, "directions", 16.0f, 1.0f, 48.f)
@registerParameterFloat(quality, "quality", 4.0f, 1.0f, 32.f)
@registerParameterFloat(size, "size", 1.0f, 0.0f, 100.0f)
@registerParametersEnd
 
#define iResolution vec2(1920,1080)
#define PI 3.14159265359

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    float Pi = 6.28318530718; // Pi*2
    
    // GAUSSIAN BLUR SETTINGS {{{
    float Directions = userParametersFS.directions; // BLUR DIRECTIONS (Default 16.0 - More is better but slower)
    float Quality = userParametersFS.quality; // BLUR QUALITY (Default 4.0 - More is better but slower)
    float Size = userParametersFS.size; // BLUR SIZE (Radius)
    // GAUSSIAN BLUR SETTINGS }}}
   
    vec2 Radius = Size/iResolution.xy;
    
    // Normalized pixel coordinates (from 0 to 1)
    vec2 uv = fragCoord/iResolution.xy;
    // Pixel colour
    vec4 Color = texture(iChannel0, uv);
    
    // Blur calculations
    for( float d=0.0; d<Pi; d+=Pi/Directions)
    {
		for(float i=1.0/Quality; i<=1.0; i+=1.0/Quality)
        {
			Color += texture( iChannel0, uv+vec2(cos(d),sin(d))*Radius*i);		
        }
    }
    
    // Output to screen
    Color /= Quality * Directions;
    fragColor =  Color;
}


 
void fragmentMain()
{
	vec4 fragColor = vec4(0);
	vec2 fragCoord = viz_getTexCoord() * iResolution;
    mainImage(fragColor, fragCoord);
	vec3 col = fragColor.rgb;
    viz_setFragment(vec4(col, 1.0));
}
