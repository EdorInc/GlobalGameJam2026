Shader "Custom/SeeThroughShader"
{
	Properties{}

	SubShader{

		Tags {
			"RenderType" = "Opaque"
		}

		Pass {
			ZWrite Off
		}
	}
}