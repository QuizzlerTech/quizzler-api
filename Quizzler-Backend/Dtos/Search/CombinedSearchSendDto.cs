﻿namespace Quizzler_Backend.Dtos.Search
{
    public class CombinedSearchSendDto
    {
        public List<UserSendDto> Users { get; set; } = new List<UserSendDto>();
        public List<LessonSearchSendDto> Lessons { get; set; } = new List<LessonSearchSendDto>();
    }
}