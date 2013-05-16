using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Octgn.Effects
{
	public class GlowEffect : ShaderEffect
	{
		private static PixelShader _pixelShader = new PixelShader();

		static GlowEffect()
		{
			_pixelShader.UriSource = new Uri("pack://application:,,,/Effects/GlowEffect.ps");
		}
		
		public GlowEffect()
		{
			this.PixelShader = _pixelShader;
			this.DdxUvDdyUvRegisterIndex = 5;
			this.PaddingBottom = this.PaddingLeft =
				this.PaddingTop = this.PaddingRight = 5;
			// Update each DependencyProperty that's registered with a shader register.  This
			// is needed to ensure the shader gets sent the proper default value.
			UpdateShaderValue(InputProperty);
			UpdateShaderValue(ColorFilterProperty);
		}

		public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(GlowEffect), 0);
		public Brush Input
		{
			get { return (Brush)GetValue(InputProperty); }
			set { SetValue(InputProperty, value); }
		}

		public static readonly DependencyProperty ColorFilterProperty = DependencyProperty.Register("ColorFilter", typeof(Color), typeof(GlowEffect), 
			new UIPropertyMetadata(Colors.Yellow, PixelShaderConstantCallback(0)));
		public Color ColorFilter
		{
			get { return (Color)GetValue(ColorFilterProperty); }
			set { SetValue(ColorFilterProperty, value); }
		}		
	}
}
