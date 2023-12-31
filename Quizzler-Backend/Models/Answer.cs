﻿using System.ComponentModel.DataAnnotations;

namespace Quizzler_Backend.Models
{
    public class Answer
    {
        [Key]
        public int AnswerId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        [StringLength(255)]
        public string? AnswerText { get; set; }

        [Required]
        public bool IsCorrect { get; set; }

        public virtual Question Question { get; set; } = null!;
        public virtual Media? AnswerMedia { get; set; } = null!;
    }
}
