using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Customer.ReferenceData;

namespace NCS.DSS.ChangeFeedListener.Model
{
    public class Customer : ICustomer
    {
        [Display(Description = "Unique identifier of a customer")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public Guid? CustomerId { get; set; }

        [Display(Description = "Date and time the customer was first recognised by the National Careers Service")]
        public DateTime? DateOfRegistration { get; set; }

        [Display(Description = "Customers given title   :   " +
                                "1 - Dr,   " +
                                "2 - Miss,   " +
                                "3 - Mr,   " +
                                "4 - Mrs,   " +
                                "5 - Ms,   " +
                                "99 - Not provided")]
        public Title? Title { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z]+((['\,\.\-][a-zA-Z])?[a-zA-Z]*)*$")]
        [Display(Description = "Customers first or given name")]
        [StringLength(100)]
        public string GivenName { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z]+((['\,\.\-][a-zA-Z])?[a-zA-Z]*)*$")]
        [Display(Description = "Customers family or surname")]
        [StringLength(100)]
        public string FamilyName { get; set; }

        [Display(Description = "Customers date of birth")]
        public DateTime? DateofBirth { get; set; }

        [Display(Description = "Customers gender  :   " +
                                "1 - Female,  " +
                                "2 - Male,  " +
                                "3 - Not applicable,  " +
                                "99 - Not provided,  ")]
        public Gender? Gender { get; set; }
 
        [Display(Description = "Customers unique learner number as issued by the learning record service")]
        [StringLength(10)]
        public string UniqueLearnerNumber { get; set; }

        [Display(Description = "An indicator to show whether an individual wishes to participate in User Research or not")]
        public bool? OptInUserResearch { get; set; }

        [Display(Description = "An indicator to show whether an individual wishes to participate in Market Research or not")]
        public bool? OptInMarketResearch { get; set; }

        [Display(Description = "Date the customer terminated their account")]
        public DateTime? DateOfTermination { get; set; }

        [Display(Description = "Reason for why the customer terminated their account   :   " +
                                "1 - Customer choice,  " +
                                "2 - Deceased,  " +
                                "3 - Duplicate,  " +
                                "99 - Other")]
        public ReasonForTermination? ReasonForTermination { get; set; }

        [Display(Description = "Introduced By   :   " +
                                "1 - Advanced Learning Loans,  " +
                                "2 - Apprenticeship Service,  " +
                                "3 - Careers Fair / Activity,  " +
                                "4 - Charity,  " +
                                "5 - Citizens Advice,  " +
                                "6 - College / 6th Form,  " +
                                "7 - Community Centre / Library,  " +
                                "8 - Employer,  " +
                                "9 - Facebook,  " +
                                "10 - Job Centre Plus,  " +
                                "11 - LEP,  " +
                                "12 - National careers service website,  " +
                                "13 - Newspaper / magazine,  " +
                                "14 - Billboard or Public Transport Advert,  " +
                                "15 - Professional Body or Organisation,  " +
                                "16 - Radio,  " +
                                "17 - School,  " +
                                "18 - Training Provider,  " +
                                "19 - TV,  " +
                                "20 - Twitter,  " +
                                "21 - University / School / College / Training Provider,  " +
                                "22 - University,  " +
                                "23 - Word of Mouth,  " +
                                "24 - World Skills UK Live,  " +
                                "98 - Other,  " +
                                "99 - Not provided  ")]
        public IntroducedBy? IntroducedBy { get; set; }

        [Display(Description = "Additional information on how the customer was introduced to the National Careers Service")]
        [StringLength(100)]
        public string IntroducedByAdditionalInfo { get; set; }

        [Display(Description = "Date and time of the last modification to the record")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        public string LastModifiedTouchpointId { get; set; }
    }
}
