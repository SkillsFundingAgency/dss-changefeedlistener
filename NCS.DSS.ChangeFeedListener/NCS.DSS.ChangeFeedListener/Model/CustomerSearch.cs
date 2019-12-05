using NCS.DSS.Customer.ReferenceData;
using System;

namespace NCS.DSS.ChangeFeedListener.Model
{
    public class CustomerSearch
    {
        public Guid? CustomerId { get; set; }

        public DateTime? DateOfRegistration { get; set; }

        public Title? Title { get; set; }

        public string GivenName { get; set; }

        public string FamilyName { get; set; }

        public DateTime? DateofBirth { get; set; }

        public Gender? Gender { get; set; }

        public string UniqueLearnerNumber { get; set; }

        public bool? OptInUserResearch { get; set; }

        public bool? OptInMarketResearch { get; set; }

        public DateTime? DateOfTermination { get; set; }

        public ReasonForTermination? ReasonForTermination { get; set; }

        public IntroducedBy? IntroducedBy { get; set; }

        public string IntroducedByAdditionalInfo { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        public string LastModifiedTouchpointId { get; set; }

    }

}