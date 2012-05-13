//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- GlowEffect
//
//--------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
// Shader constant register mappings (scalars - float, double, Point, Color, Point3D, etc.)
//-----------------------------------------------------------------------------------------

float4 color: register(C0);
float4 dduv : register(C5);

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D src : register(S0);


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

float4 main(float2 uv : TEXCOORD) : COLOR
{
	float4 res = tex2D(src, uv);
	if (res.a == 0)
	{
		float accum = 0;  
		float2 pos = uv - 2 * (dduv.xy + dduv.zw);		
		for (int i=-2; i <= 2; i += 1)
		{			
			for (int j=-2; j <= 2; j += 1)		
			{
				accum += tex2D(src, pos).a;
				pos += dduv.xy;
			}
			pos += dduv.zw;
		}	
		res.rgb = color.rgb;
		res.a = accum / 2;
	}
	return res;
}


