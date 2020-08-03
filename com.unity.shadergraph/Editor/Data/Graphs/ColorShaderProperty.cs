using System;
using System.Text;
using UnityEditor.Graphing;
using UnityEngine;

namespace UnityEditor.ShaderGraph.Internal
{
    [Serializable]
    [FormerName("UnityEditor.ShaderGraph.ColorShaderProperty")]
    public sealed class ColorShaderProperty : AbstractShaderProperty<Color>
    {
        public override int latestVersion => 1;

        [SerializeField]
        private int m_Version = 0;

        public override int version
        {
            get => m_Version;
            protected set => m_Version = value;
        }

        internal ColorShaderProperty()
        {
            displayName = "Color";
            version = latestVersion;
        }

        internal ColorShaderProperty(int version) : this()
        {
            this.version = version;
        }
        
        public override PropertyType propertyType => PropertyType.Color;
        
        internal override bool isBatchable => true;
        internal override bool isExposable => true;
        internal override bool isRenamable => true;
        internal override bool isGpuInstanceable => true;
        
        internal string hdrTagString => colorMode == ColorMode.HDR ? "[HDR]" : "";

        internal override string GetPropertyBlockString()
        {
            return $"{hideTagString}{hdrTagString}{referenceName}(\"{displayName}\", Color) = ({NodeUtils.FloatToShaderValue(value.r)}, {NodeUtils.FloatToShaderValue(value.g)}, {NodeUtils.FloatToShaderValue(value.b)}, {NodeUtils.FloatToShaderValue(value.a)})";
        }

        public override string GetDefaultReferenceName()
        {
            return $"Color_{objectId}";
        }
        
        [SerializeField]
        ColorMode m_ColorMode;

        public ColorMode colorMode
        {
            get => m_ColorMode;
            set => m_ColorMode = value;
        }

        internal override AbstractMaterialNode ToConcreteNode()
        {
            return new ColorNode { color = new ColorNode.Color(value, colorMode) };
        }

        internal override PreviewProperty GetPreviewMaterialProperty()
        {
            return new PreviewProperty(propertyType)
            {
                name = referenceName,
                colorValue = value
            };
        }        

        internal override ShaderInput Copy()
        {
            return new ColorShaderProperty()
            {
                version = version,
                displayName = displayName,
                hidden = hidden,
                value = value,
                colorMode = colorMode,
                precision = precision,
                gpuInstanced = gpuInstanced,
            };
        }
    }
}
