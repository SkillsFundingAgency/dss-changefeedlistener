using System.ComponentModel;

namespace NCS.DSS.Customer.ReferenceData
{
    public enum Title
    {
        [Description("Mr")]
        Mr = 1,

        [Description("Mrs")]
        Mrs = 2,

        [Description("Miss")]
        Miss = 3,

        [Description("Dr")]
        Dr = 4,

        [Description("Other")]
        Other = 5,

        [Description("Not Provided")]
        NotProvided = 99
    }
}
