namespace InstrukcijeDotNet.ViewModels
{
    public class InstructionDisplay
    {
        public int id { get; set; }
        public string name { get; set; }

        public string surname {  get; set; }

        public string? profilePictureUrl { get; set; }
        public string? subjectTitle { get; set; }
        public int? professorId { get; set; }
        public DateTime? time { get; set; }
        public string? status { get; set; }
    }
}
