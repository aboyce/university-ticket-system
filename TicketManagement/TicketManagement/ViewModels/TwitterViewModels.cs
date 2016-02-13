﻿using System;

namespace TicketManagement.ViewModels
{
    class TwitterViewModels
    {
    }

    public class TwitterIndexViewModel
    {
        public bool IsLoggedIn { get; set; } = false;
    }

    public class TwitterProfileSummaryViewModel
    {
        public string Name { get; set; }
        public string ScreenName { get; set; }
        public int FavouritesCount { get; set; }
        public int FollowersCount { get; set; }
        public int FriendsCount { get; set; }
        public string ProfileImageUrl { get; set; }
    }

    public class TwitterTweetViewModel
    {
        public string Text { get; set; }
        public Tweetinvi.Core.Interfaces.IUser CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int FavouriteCount { get; set; }
        public int HashtagCount { get; set; }
        public int TweetLength { get; set; }
    }
}