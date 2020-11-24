using System.ComponentModel;

namespace NCS.DSS.Customer.ReferenceData
{
    public enum IntroducedBy
    {
        [Description("Advanced Learning Loans")]
        AdvancedLearningLoans = 1,
        [Description("Apprenticeship Service")]
        ApprenticeshipService = 2,
        [Description("Careers Fair/Activity")]
        CareersFairActivity = 3,
        [Description("Charity")]
        Charity = 4,
        [Description("Citizens Advice")]
        CitizensAdvice = 5,
        [Description("College/6th Form")]
        College6thForm = 6,
        [Description("Community Centre/Library")]
        CommunityCentreLibrary = 7,
        [Description("Employer")]
        Employer = 8,
        [Description("Facebook")]
        Facebook = 9,
        [Description("Job Centre Plus")]
        JobCentrePlus = 10,
        [Description("LEP")]
        LEP = 11,
        [Description("National careers service website")]
        Nationalcareersservicewebsite = 12,
        [Description("Newspaper/magazine")]
        Newspapermagazine = 13,
        [Description("Billboard or Public Transport Advert")]
        BillboardorPublicTransportAdvert = 14,
        [Description("Professional Body or Organisation")]
        ProfessionalBodyorOrganisation = 15,
        [Description("Radio")]
        Radio = 16,
        [Description("School")]
        School = 17,
        [Description("Training Provider")]
        TrainingProvider = 18,
        [Description("TV")]
        TV = 19,
        [Description("Twitter")]
        Twitter = 20,
        [Description("University/School/College/Training Provider")]
        UniversitySchoolCollegeTrainingProvider = 21,
        [Description("University")]
        University = 22,
        [Description("Word of Mouth")]
        WordofMouth = 23,
        [Description("World Skills UK Live")]
        WorldSkillsUKLive = 24,
        [Description("Other")]
        Other = 98,
        [Description("Not provided")]
        NotProvided = 99,
        
    }
}
