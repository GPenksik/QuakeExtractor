#ifndef Q1SHADING_INCLUDED
#define Q1SHADING_INCLUDED

float Unity_Posterize_float(float In, float Steps)
{
    return floor(In / (1 / Steps)) * (1 / Steps);
}

float3 Unity_Posterize3_float(float3 In, float Steps)
{
    return floor(In / (1 / Steps)) * (1 / Steps);
}

void SampleLightmap_float(const struct UnityTexture2DArray Lightmap, float3 UV, struct UnitySamplerState SS, out float LightLevel) {

    float4 lightmapSamples = Lightmap.SampleLevel(SS, UV, 0.0);
    LightLevel = Unity_Posterize_float(lightmapSamples[3], 64.0);
}

void MapIndexToPalette_float(const struct UnityTexture2DArray MainTex, const struct UnityTexture2DArray Palette, float3 UV, float LOD, struct UnitySamplerState SS, out float4 ColorOut, out float ColorIndexOut)
{
    float LODint = LOD;
    ColorIndexOut = MainTex.SampleLevel(SS, UV, LOD);
    ColorOut = Palette.SampleLevel(SS, ColorIndexOut, 0.0);

}


void GetLitColor_float(float3 ColorIn, float ColorIndex, float LightLevel, float Offset, out float3 ColorOut)
{
    float toInt = 256.0;
    float toFloat = 1.0/toInt;
    // Invert LightLevel

    float to64Int = 64.0;
    float to64Float = 1.0/to64Int;

    LightLevel = (1-LightLevel)*to64Int;

    // Offset Lightlevel by Scale
    LightLevel = LightLevel - (Offset);

    ColorOut = ColorIn;
    float fullBrightThresh = (256.0-32.0)*toFloat;

    if (ColorIndex < fullBrightThresh) {
        if (LightLevel > 64) {
            LightLevel = 64;
        }
        if (LightLevel < 0) {
            LightLevel = 0;
        }
    
        ColorIn = ColorIn*toInt;

        ColorOut = ((ColorIn * (63.0 - LightLevel) + 16)/32.0);
        
        ColorOut = ColorOut*toFloat;

        ColorOut = Unity_Posterize3_float(ColorOut, 256);
    }
}





#endif