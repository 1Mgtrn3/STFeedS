using STFeedS.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace STFeedS
{
   public class Tweet
    {
        public string Text { get; set; }
        public string CleanText { get; set; }

        public List<string> Links { get; set; }
        public DateTime creationDate { get; set; }

        public DateTime creationDateTime { get; set; }

        public TweetType TweetType { get; set; }
    }
}
