using System.Collections.Generic;
using backend.Models;

namespace backend.Services
{
    public static class ReviewValidator
    {
        public const int MaxNameLength = 60;
        public const int MaxTextLength = 500;

        public static List<string> Validate(ReviewCreateRequest? request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("Name is required.");
                errors.Add("Rating must be between 1 and 5.");
                errors.Add("Text is required.");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add("Name is required.");
            else if (request.Name.Trim().Length > MaxNameLength)
                errors.Add($"Name must be {MaxNameLength} characters or fewer.");

            if (request.Rating < 1 || request.Rating > 5)
                errors.Add("Rating must be between 1 and 5.");

            if (string.IsNullOrWhiteSpace(request.Text))
                errors.Add("Text is required.");
            else if (request.Text.Trim().Length > MaxTextLength)
                errors.Add($"Text must be {MaxTextLength} characters or fewer.");

            return errors;
        }
    }
}
