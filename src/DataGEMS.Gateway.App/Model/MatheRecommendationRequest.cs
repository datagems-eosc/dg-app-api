namespace DataGEMS.Gateway.App.Model
{
    public class MatheRecommendationRequest
    {
        public string QuestionId { get; set; }
        public string Question { get; set; }
        public int RecommendedMaterialsCount { get; set; }
    }
}
