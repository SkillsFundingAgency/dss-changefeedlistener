using System.ComponentModel;

namespace NCS.DSS.Customer.ReferenceData
{
    public enum Gender
    {
        [Description("Female")]
        Female = 1,

        [Description("Male")]
        Male = 2,

        [Description("Not Applicable")]
        NotApplicable = 3,

        [Description("Not Provided")]
        NotProvided = 99

    }
}
