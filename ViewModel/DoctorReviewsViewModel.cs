using System;
using System.Collections.Generic;

namespace Midix.ViewModel
{
    public class DoctorReviewsViewModel
    {
        public string DoctorId { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        public double AverageRate { get; set; }

        public int TotalReviews { get; set; }

        public List<ReviewItemVM> Reviews { get; set; } = new();
    }

    public class ReviewItemVM
    {
        public int RatingId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public int Rate { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}