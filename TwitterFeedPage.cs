using System;
using System.Collections.Generic;
using System.Text;

namespace STFeedS
{
    class TwitterFeedPage
    {
        public List<Tweet> ListOfTweets { get; set; }
        public string NextPageUrl { get; set; }

        public TwitterFeedPage()
        {
            ListOfTweets = new List<Tweet>();
        }
    }
}
