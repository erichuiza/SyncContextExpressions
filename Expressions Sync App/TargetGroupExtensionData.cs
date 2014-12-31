using System.Runtime.Serialization;

namespace Tridion.Extensions.ContextExpressions
{
    [DataContract(Namespace = "http://wwww.sdltridion.com/ContentManager/Extensions/ContextExpressions/2013")]
    public class TargetGroupExtensionData
    {
        public const string ApplicationId = "ce:TargetGroupExtension";

        [DataMember]
        public string ContextExpression;

        [DataMember]
        public string SyncLabel;
    }
}
