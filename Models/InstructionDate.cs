namespace InstrukcijeDotNet.Models
{
    public class InstructionDate
    {
        public int id { get; set; }
        public int studentId { get; set; }
        public int professorId { get; set; }
        public DateTime? dateTime { get; set; }
        public string status { get; set; }
    }
}
